using System;
using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public class EndGameSingleDownload : SingleDownload
    {
        #region Fields

        private readonly EndGameDownloader downloader;
        private int unhave;
        private readonly IConnection connection;
        private bool choked;
        private bool interested;
        private bool[] have;
        private readonly Measure measure;
        private DateTime last;
        private readonly Random ran;

        #endregion

        #region Constructors

        public EndGameSingleDownload(EndGameDownloader downloader, IConnection connection, NormalSingleDownload old)
        {
            this.downloader = downloader;
            this.unhave = downloader.PiecesNumber;
            ran = new Random();

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

                List<ActiveRequest> requests = new List<ActiveRequest>();

                while (downloader.Requests.Count > 0)
                {
                    int k = ran.Next(downloader.Requests.Count);
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

            while (downloader.Requests.Count > 0)
            {
                int k = ran.Next(downloader.Requests.Count);
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
            if (have[index])
            {
                return;
            }
            have[index] = true;
            unhave--;
            if (downloader.StorageWrapper.DoIHave(index))
            {
                return;
            }

            List<ActiveRequest> t = new List<ActiveRequest>();

            while (downloader.Requests.Count > 0)
            {
                int k = ran.Next(downloader.Requests.Count);
                t.Add(downloader.Requests[k]);
                downloader.Requests.RemoveAt(k);
            }

            downloader.Requests = t;

            foreach (ActiveRequest request in downloader.Requests)
            {
                if (request.Index == index)
                {
                    this.SendRequest(index, request.Begin, request.Length);
                }
            }

            if (downloader.Requests.Count == 0 && unhave == 0)
            {
                connection.Close();
            }
        }

        public override void GetHaveBitField(bool[] have)
        {
            this.have = have;
            foreach (bool b in have)
            {
                if (b)
                {
                    unhave--;
                }
            }

            List<ActiveRequest> t = new List<ActiveRequest>();

            while (downloader.Requests.Count > 0)
            {
                int k = ran.Next(downloader.Requests.Count);
                t.Add(downloader.Requests[k]);
                downloader.Requests.RemoveAt(k);
            }
            downloader.Requests = t;

            foreach (ActiveRequest request in t)
            {
                if (have[request.Index])
                {
                    this.SendRequest(request.Index, request.Begin, request.Length);
                }
            }
            if (downloader.Requests.Count == 0 && unhave == 0)
            {
                this.connection.Close();
            }
        }

        public override bool GetPiece(int index, int begin, byte[] piece)
        {
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
            if (downloader.MeasureFunction != null)
            {
                downloader.MeasureFunction(piece.Length);
            }
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

                        while (inactiveRequests.Count > 0)
                        {
                            int k = ran.Next(inactiveRequests.Count);
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

            foreach (EndGameSingleDownload download in downloader.Downloads)
            {
                if (download.have[index])
                {
                    bool bFound = false;
                    foreach (ActiveRequest activeRequest in downloader.Requests)
                    {
                        if (download.have[activeRequest.Index])
                        {
                            bFound = true;
                            break;
                        }
                    }
                    if (!bFound)
                    {
                        download.interested = false;
                        download.connection.SendNotInterested();
                    }
                }
            }

            if(downloader.Requests.Count == 0)
            {
                SingleDownload[] activeDownloads =downloader.Downloads.ToArray();
                foreach (EndGameSingleDownload download in activeDownloads)
                {
                    if(download.unhave == 0)
                    {
                        download.connection.Close();
                    }
                }
            }
            return true;
        }

        #endregion
    }
}
