using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    public abstract class BitTorrentMessage
    {
        public abstract byte[] Encode();

        public abstract bool Decode(byte[] buffer);

        public abstract void Handle();

        public abstract int BytesLength { get; }
    }
}
