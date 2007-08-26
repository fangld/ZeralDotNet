using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Storages;
using ZeraldotNet.LibBitTorrent.Connecters;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public class EndGameSingleDownload : SingleDownload
    {
        #region Fields

        private EndgameDownloader downloader;
        private int unhave;
        private IConnection connection;
        private bool choked;
        private bool interested;
        private bool[] have;
        private Measure measure;
        private DateTime last;
        private Random ran;

        #endregion

        //#region Properties

        //public bool[] Have
        //{
        //    get { return this.have; }
        //}

        //#endregion

        #region Constructors

        public EndGameSingleDownload(EndgameDownloader downloader, IConnection connection, NormalSingleDownload old)
        {
            this.downloader = downloader;
            this.unhave = downloader.PiecesNumber;
            if (old == null)
            {
                this.connection = connection;
                this.choked = true;
                this.interested = false;
                this.have = new bool[unhave];
                this.measure = new Measure(downloader.MaxRatePeriod);
                this.last = DateTime.MinValue;
            }

            else
            {
                this.connection = old.Connection;
                this.connection.Download = this;
                this.choked = old.Choked;
                this.have = old.Have;
                this.interested = old.Interested;
                this.measure = old.Measure;
                this.last = old.Last;

                ran = new Random();
                List<ActiveRequest> requests = new List<ActiveRequest>();
                int k;
                while (downloader.Requests.Count > 0)
                {
                    k = ran.Next(downloader.Requests.Count);
                    requests.Add(downloader.Requests[k]);
                    downloader.Requests.RemoveAt(k);
                }
                downloader.Requests = requests;

                foreach (bool value in this.have)
                {
                    if (value)
                    {
                        this.unhave--;
                    }
                }

                if (!choked)
                {
                    foreach (ActiveRequest request in downloader.Requests)
                    {
                        if (!old.Requests.Contains(request))
                        {
                            if (have[request.Index])
                            {
                                this.SendRequest(request.Index, request.Begin, request.Length);
                            }
                        }
                    }
                }
            
            }
        }

        #endregion

        #region Methods

        public void SendRequest(int index, int begin, int length)
        {
            if (!interested)
            {
                interested = true;
                connection.SendInterested();
            }
            connection.SendRequest(index, begin, length);
        }

        #endregion


        #region ISingleDownload Members

        public override bool Snubbed
        {
            get { return (DateTime.Now.Subtract(last).TotalSeconds > downloader.SnubTime); }
            set { throw new NotImplementedException(); }
        }

        public override double Rate
        {
            get { return measure.UpdatedRate; }
            set { throw new NotImplementedException(); }
        }

        public override void Disconnect()
        {
            downloader.Downloads.Remove(this);
        }

        public override void GetChoke()
        {
            choked = true;
        }

        public override void GetUnchoke()
        {
            if (!(choked && interested))
            {
                return;
            }

            List<ActiveRequest> requests = new List<ActiveRequest>();
            int k;
            while (downloader.Requests.Count > 0)
            {
                k = ran.Next(downloader.Requests.Count);
                requests.Add(downloader.Requests[k]);
                downloader.Requests.RemoveAt(k);
            }
            downloader.Requests = requests;

            foreach (ActiveRequest request in downloader.Requests)
            {
                if (have[request.Index])
                {
                    connection.SendRequest(request.Index, request.Begin, request.Length);
                }
            }
        }

        public override void GetHave(int index)
        {
            throw new NotImplementedException();
        }

        public override void GetHaveBitField(bool[] have)
        {
            throw new NotImplementedException();
        }

        public override bool GetPiece(int index, int begin, byte[] piece)
        {
            throw new NotImplementedException();
            ActiveRequest comeInRequest = new ActiveRequest(index, begin, piece.Length);
            if (downloader.Requests.Contains(comeInRequest))
            {
                downloader.Requests.Remove(comeInRequest);
            }

            else
            {
                return false;
            }

            last = DateTime.Now;
            measure.UpdateRate(piece.Length);
            downloader.DownloadMeasure.UpdateRate(piece.Length);
            downloader.MeasureFunction(piece.Length);
            IStorageWrapper storageWrapper = downloader.StorageWrapper;
            storageWrapper.PieceCameIn(index, begin, piece);
            if (storageWrapper.DoIHaveRequests(index))
            {
                List<InactiveRequest> inactiveRequests = new List<InactiveRequest>();
                InactiveRequest newRequest;
                while (storageWrapper.DoIHaveRequests(index))
                {
                    newRequest = storageWrapper.NewRequest(index);
                    inactiveRequests.Add(newRequest);
                    downloader.Requests.Add(new ActiveRequest(index, newRequest.Begin, newRequest.Length));
                }

                foreach (EndGameSingleDownload download in downloader.Downloads)
                {
                    if (!download.choked && download.have[index])
                    {
                        List<InactiveRequest> requests = new List<InactiveRequest>();
                        int k;
                        while (inactiveRequests.Count > 0)
                        {
                            k = ran.Next(inactiveRequests.Count);
                            requests.Add(inactiveRequests[k]);
                            inactiveRequests.RemoveAt(k);
                        }

                        inactiveRequests = requests;

                        foreach (InactiveRequest request in inactiveRequests)
                        {
                            if (request.Begin != begin || download == this)
                            {
                                download.SendRequest(index, request.Begin, request.Length);
                            }
                        }

                    }
                }
                return false;
            }

            foreach (EndGameSingleDownload download in downloader.Downloads)
            {
                if (download.have[index] && download != this && !download.choked)
                {
                    download.connection.SendCancel(index, begin, piece.Length);
                }
            }

            foreach (ActiveRequest activeRequest in downloader.Requests)
            {
                if (activeRequest.Index == index)
                {
                    return true;
                }
            }

            //foreach (EndGameSingleDownload download in downloader.Requests)
            //{
            //    //if (
            //}
            
        }

        #endregion
    }
}
