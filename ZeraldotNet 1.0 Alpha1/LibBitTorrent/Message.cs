using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// The different types of messages that can be sent or received to/from peers
    /// </summary>
    public enum Message
    {
        CHOKE = 0,
        UNCHOKE = 1,
        INTERESTED = 2,
        NOT_INTERESTED = 3,
        // index
        HAVE = 4,
        // index, bitfield
        BITFIELD = 5,
        // index, begin, length
        REQUEST = 6,
        // index, begin, piece
        PIECE = 7,
        // index, begin, piece
        CANCEL = 8
    }
}
