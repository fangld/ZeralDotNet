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
        Have = 4,
        BitField = 5,
        Request = 6,
        Piece = 7,
        Cancel = 8,
        Port = 9,
        SuggestPiece = 0x0D,
        HaveAll = 0x0E,
        HaveNone = 0x0F,
        RejectRequest = 0x10,
        AllowedFast = 0x11,
        ExtendedList = 20,
        KeepAlive,
        Handshake
    }
}
