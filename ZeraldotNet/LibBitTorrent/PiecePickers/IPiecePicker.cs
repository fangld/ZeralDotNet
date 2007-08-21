using System;

namespace ZeraldotNet.LibBitTorrent.PiecePickers
{
    public interface IPiecePicker
    {
        void Complete(int index);
        void GotHave(int index);
        void LostHave(int index);
        int Next(WantDelegate haveFunction);
        int PiecesNumber { get; set; }
        void Requested(int index);
    }
}
