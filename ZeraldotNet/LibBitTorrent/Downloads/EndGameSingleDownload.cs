using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        #endregion

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
                //this.connection = old.con
            }
        }

        #region ISingleDownload Members

        public override bool Snubbed
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override double Rate
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override void Disconnect()
        {
            throw new NotImplementedException();
        }

        public override void GetChoke()
        {
            throw new NotImplementedException();
        }

        public override void GetUnchoke()
        {
            throw new NotImplementedException();
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
        }

        #endregion
    }
}
