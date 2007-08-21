using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Connecters;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public class SingleDownload : ISingleDownload
    {
        #region Private Fields

        private Downloader downloader;
        private IConnection connection;
        private bool choked;
        private bool interested;
        private List<ActiveRequest> requests;
        private Measure measure;
        private bool[] have;
        private DateTime last;

        #endregion

        #region Public Properties

        public bool Choked
        {
            get
            {
                return choked;
            }
        }

        public bool Interested
        {
            get
            {
                return interested;
            }
        }

        #endregion

        public SingleDownload(Downloader downloader, IConnection connection)
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
            foreach (SingleDownload download in downloader.Downloads)
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

            int piece = downloader.PiecePicker.Next(new WantDelegate(this.Want));

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

                    piece = downloader.PiecePicker.Next(new WantDelegate(this.Want));
                }

                if (hit)
                {
                    foreach (SingleDownload download in downloader.Downloads)
                    {
                        download.FixDownload();
                    }
                }
            }
        }

        #endregion

        #region ISingleDownload Members

        public bool Snubbed
        {
            get
            {
                return (DateTime.Now.Subtract(last).TotalSeconds > downloader.SnubTime);
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public double Rate
        {
            get
            {
                return measure.UpdatedRate;
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void Disconnect()
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

        public void GetChoke()
        {
            if (!choked)
            {
                choked = true;
                LetGo();
            }
        }

        public void GetUnchoke()
        {
            if (choked)
            {
                choked = false;
                this.FixDownload();
            }
        }

        public void GetHave(int index)
        {
            if (have[index])
            {
                return;
            }
            have[index] = true;
            downloader.PiecePicker.GotHave(index);
            this.FixDownload();
        }

        public void GetHaveBitField(bool[] have)
        {
            this.have = have;
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

        public bool GetPiece(int index, int begin, byte[] piece)
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
