using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    public class NotInterestedMessage : InterestedMessage
    {
        public override byte[] Encode()
        {
            return Encode(BitTorrentMessageType.NotInterested);
        } 
    }
}
