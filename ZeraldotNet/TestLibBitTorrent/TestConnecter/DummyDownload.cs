using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Downloads;

namespace ZeraldotNet.TestLibBitTorrent.TestConnecter
{
    public class DummyDownload : ISingleDownload
    {
        List<string> events;
        int hit;

        public DummyDownload(List<string> events)
        {
            this.events = events;
            events.Add("make download");
            hit = 0;
        }

        public void Disconnect()
        {
            events.Add("disconnected");
        }

        public void GetChoke()
        {
            events.Add("choke");
        }

        public void GetUnchoke()
        {
            events.Add("unchoke");
        }

        public void GetHave(int index)
        {
            events.Add(string.Format("have:{0}", index));
        }

        public void GetHaveBitField(bool[] bitfield)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("bitfield:");

            byte[] bitfieldBytes = BitField.ToBitField(bitfield);
            foreach (byte item in bitfieldBytes)
            {
                sb.AppendFormat("0x{0:X2},", item);
            }
            events.Add(sb.ToString());
        }

        public bool GetPiece(int index, int begin, byte[] piece)
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

        #endregion
    }
}
