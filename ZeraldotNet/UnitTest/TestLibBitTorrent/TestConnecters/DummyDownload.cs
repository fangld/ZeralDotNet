﻿using System;
using System.Collections.Generic;
using System.Text;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Downloads;

namespace ZeraldotNet.UnitTest.TestLibBitTorrent.TestConnecters
{
    public class DummyDownload : SingleDownload
    {
        private readonly List<string> events;
        int hit;

        public DummyDownload(List<string> events)
        {
            this.events = events;
            events.Add("make StartDownload");
            hit = 0;
        }

        public override void Disconnect()
        {
            events.Add("disconnected");
        }

        public override void GetChoke()
        {
            events.Add("choke");
        }

        public override void GetUnchoke()
        {
            events.Add("unchoke");
        }

        public override void GetHave(int index)
        {
            events.Add(string.Format("have:{0}", index));
        }

        public override void GetHaveBitField(bool[] bitfield)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("bitfield:");

            //byte[] bitfieldBytes = BitField.ToBitField(bitfield);
            //foreach (byte item in bitfieldBytes)
            //{
            //    sb.AppendFormat("0x{0:X2},", item);
            //}
            events.Add(sb.ToString());
        }

        public override bool GetPiece(int index, int begin, byte[] piece)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("request index:{0}, begin:{1}, piece:", index, begin));
            foreach (byte item in piece)
            {
                sb.AppendFormat("0x{0:X2},", item);
            }

            events.Add(sb.ToString());
            hit += 1;
            return hit > 1;
        }

        #region ISingleDownload Members

        public override bool Snubbed
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

        public override double Rate
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

        #endregion
    }
}