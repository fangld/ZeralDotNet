using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    /// <summary>
    /// choke: <len=0001><id=0>
    /// </summary>
    public class ChokeMessage : BitTorrentMessage
    {
        public override byte[] Encode()
        {
            return this.Encode(BitTorrentMessageType.Choke);
        }

        public override bool Decode(byte[] buffer)
        {
            if (buffer.Length != BytesLength)
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
            get { return 1; }
        }

        public override void Handle()
        {
            throw new NotImplementedException();
        }

        protected virtual byte[] Encode(BitTorrentMessageType type)
        {
            byte[] result = new byte[1];
            result[0] = (byte)type;
            return result;
        }
    }
}
