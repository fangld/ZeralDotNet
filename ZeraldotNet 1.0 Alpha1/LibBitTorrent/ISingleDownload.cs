using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public interface ISingleDownload
    {
        bool IsSnubbed();
        double GetRate();
        void Disconnected();
        void GotChoke();
        void GotUnchoke();
        void GotHave(int index);
        void GotHaveBitField(bool[] have);
        bool GotPiece(int index, int begin, byte[] piece);
    }
}
