using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Connecters;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public class SingleDownload : ISingleDownload
    {
        private Downloader downloader;
        private IConnection connection;
        private bool choked;
        private bool interested;
        private List<ActiveRequest> requests;
        private Measure measure;
        private bool[] have;
        private DateTime last;

        public SingleDownload(Downloader downloader, IConnection connection)
        {
            this.downloader = downloader;
            this.connection = connection;
            this.choked = true;
            this.interested = false;
            this.requests = new List<ActiveRequest>();
            //this.measure = new Measure(downloader.m
        }

        #region ISingleDownload Members

        public bool Snubbed
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
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
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void Disconnect()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetChoke()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetUnchoke()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetHave(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GetHaveBitField(bool[] have)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool GetPiece(int index, int begin, byte[] piece)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
