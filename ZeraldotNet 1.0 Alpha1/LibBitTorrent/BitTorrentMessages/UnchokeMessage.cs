﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    public class UnchokeMessage : ChokeMessage
    {
        public override byte[] Encode()
        {
            return this.Encode(BitTorrentMessageType.Unchoke);
        }
    }
}