using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessage
{
    /// <summary>
    /// choke: <len=0001><id=0>
    /// </summary>
    public class ChokeMessage : BitTorrentMessage
    {
        public override byte[] Encode()
        {
            byte[] result = new byte[1];
            result[0] = (byte)BitTorrentMessageType.Choke;
            return result;
        }

        public override bool Decode(byte[] buffer)
        {
            if (buffer.Length != 1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override int BytesLength
        {
            get
            {
                return 1;
            }
        }

        public override void Handle()
        {
            base.Handle();
        }
    }
}
