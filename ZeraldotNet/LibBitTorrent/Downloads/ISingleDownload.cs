using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public interface ISingleDownload
    {
        bool Snubbed { get; set; }
        double Rate { get; set; }
        void Disconnect();
        void GetChoke();
        void GetUnchoke();
        void GetHave(int index);
        void GetHaveBitField(bool[] have);
        bool GetPiece(int index, int begin, byte[] piece);
    }
}
