using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    public class RequestMessage : CancelMessage
    {
        public RequestMessage() : base() { }

        public RequestMessage(int index, int begin, int length) : base(index, begin, length) { }

        public override byte[] Encode()
        {
            return this.Encode(BitTorrentMessageType.Request);
        }
    }
}
