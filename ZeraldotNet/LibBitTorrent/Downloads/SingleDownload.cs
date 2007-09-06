namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public abstract class SingleDownload
    {
        public abstract bool Snubbed { get; set; }
        public abstract double Rate { get; set; }
        public abstract void Disconnect();
        public abstract void GetChoke();
        public abstract void GetUnchoke();
        public abstract void GetHave(int index);
        public abstract void GetHaveBitField(bool[] have);
        public abstract bool GetPiece(int index, int begin, byte[] piece);
    }
}
