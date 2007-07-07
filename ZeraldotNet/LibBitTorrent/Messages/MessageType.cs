using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// The different types of messages that can be sent or received to/from peers
    /// </summary>
    public enum MessageType : byte
    {
        Choke = 0,
        Unchoke = 1,
        Interested = 2,
        NotInterested = 3,
        // index
        Have = 4,
        // index, bitfield
        BitField = 5,
        // index, begin, lengthBytes
        Request = 6,
        // index, begin, index
        Piece = 7,
        // index, begin, index
        Cancel = 8,
        // port
        Port = 9,

        SuggestPiece = 0x0D,

        HaveAll = 0x0E,

        HaveNone = 0x0F,

        RejectRequest = 0x10,

        AllowedFast = 0x11,

        ExtendedList = 20
    }
}
