using System;
using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent.Connecters;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public class NormalSingleDownload : SingleDownload
    {
        #region Fields

        private readonly NormalDownloader downloader;
        private readonly IConnection connection;
        private bool choked;
        private bool interested;
        private List<ActiveRequest> requests;
        private readonly Measure measure;
        private bool[] have;
        private DateTime last;

        #endregion

        #region Properties

        public bool Choked
        {
            get { return choked; }
        }

        public IConnection Connection
        {
            get { return this.connection; }
        }

        public bool Interested
        {
            get { return interested; }
        }

        public bool[] Have
        {
            get { return have; }
        }

        public DateTime Last
        {
            get { return last; }
        }

        public Measure Measure
        {
            get { return measure; }
        }

        public List<ActiveRequest> Requests
        {
            get { return this.requests; }
        }

        #endregion

        #region Constructors

        public NormalSingleDownload(NormalDownloader downloader, IConnection connection)
        {
            this.downloader = downloader;
            this.connection = connection;
            this.choked = true;
            this.interested = false;
            this.requests = new List<ActiveRequest>();
            this.measure = new Measure(downloader.MaxRatePeriod);
            this.have = new bool[downloader.PiecesNumber];
            int i;
            for (i = 0; i < have.Length; i++)
            {
                have[i] = false;
            }
            this.last = DateTime.MinValue;
        }

        #endregion

        #region Methods

        private void LetGo()
        {
            if (requests == null)
            {
                return;
            }

            foreach (ActiveRequest request in requests)
            {
                downloader.StorageWrapper.RequestLost(request.Index, new InactiveRequest(request.Begin, request.Length));
            }
            requests = new List<ActiveRequest>();
            foreach (NormalSingleDownload download in downloader.Downloads)
            {
                download.FixDownload();
            }
        }

        private bool Want(int piece)
        {
            return this.have[piece] && downloader.StorageWrapper.DoIHaveRequests(piece);
        }

        public void FixDownload()
        {
            if (requests.Count == downloader.BackLog)
            {
                return;
            }

            int piece = downloader.PiecePicker.Next(this.Want);

            if (piece == -1)
            {
                if (interested && requests.Count == 0)
                {
                    interested = false;
                    connection.SendNotInterested();
                }
            }

            else
            {
                if (!interested)
                {
                    interested = true;
                    connection.SendInterested();
                }

                if (choked)
                {
                    return;
                }

                bool hit = false;

                while (piece != -1)
                {
                    while (requests.Count < downloader.BackLog)
                    {
                        InactiveRequest inactiveRequest = downloader.StorageWrapper.NewRequest(piece);
                        downloader.PiecePicker.Requested(piece);
                        requests.Add(new ActiveRequest(piece, inactiveRequest.Begin, inactiveRequest.Length));
                        connection.SendRequest(piece, inactiveRequest.Begin, inactiveRequest.Length);
                        if (!downloader.StorageWrapper.DoIHaveRequests(piece))
                        {
                            hit = true;
                            break;
                        }
                    }

                    if (requests.Count == downloader.BackLog)
                    {
                        break;
                    }

                    piece = downloader.PiecePicker.Next(this.Want);
                }

                if (hit)
                {
                    foreach (NormalSingleDownload download in downloader.Downloads)
                    {
                        download.FixDownload();
                    }
                }
            }
        }

        #endregion

        #region ISingleDownload Members

        public override bool Snubbed
        {
            get { return (DateTime.Now.Subtract(last).TotalSeconds > downloader.SnubTime); }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public override double Rate
        {
            get { return measure.UpdatedRate; }
            set { throw new Exception("The method or operation is not implemented."); }
        }

        public override void Disconnect()
        {
            this.downloader.Downloads.Remove(this);
            int i;
            for (i = 0; i < have.Length; i++)
            {
                if (have[i])
                {
                    downloader.PiecePicker.LostHave(i);
                }
            }
            this.LetGo();
        }

        public override void GetChoke()
        {
            if (!choked)
            {
                choked = true;
                LetGo();
            }
        }

        public override void GetUnchoke()
        {
            if (choked)
            {
                choked = false;
                this.FixDownload();
            }
        }

        public override void GetHave(int index)
        {
            if (have[index])
            {
                return;
            }
            have[index] = true;
            downloader.PiecePicker.GotHave(index);
            this.FixDownload();
        }

        public override void GetHaveBitField(bool[] haves)
        {
            this.have = haves;
            int i;
            for (i = 0; i < have.Length; i++)
            {
                if (have[i])
                {
                    downloader.PiecePicker.GotHave(i);
                }
            }
            this.FixDownload();
        }

        public override bool GetPiece(int index, int begin, byte[] piece)
        {
            bool isFind = false;
            int pieceLength = piece.Length;
            bool doIHave;
            ActiveRequest newRequest = new ActiveRequest(index, begin, pieceLength);

            foreach (ActiveRequest request in requests)
            {
                if (request.Equals(newRequest))
                {
                    requests.Remove(request);
                    isFind = true;
                    break;
                }
            }

            if (!isFind)
            {
                return false;
            }
            last = DateTime.Now;
            measure.UpdateRate(pieceLength);

            if (downloader.MeasureFunction != null)
            {
                downloader.MeasureFunction(pieceLength);
            }
            
            downloader.DownloadMeasure.UpdateRate(pieceLength);           
            downloader.StorageWrapper.PieceCameIn(index, begin, piece);

            doIHave = downloader.StorageWrapper.DoIHave(index);
            if (doIHave)
            {
                downloader.PiecePicker.Complete(index);
            }
            this.FixDownload();
            return doIHave;
        }

        #endregion
    }
}
