using System;
using ZeraldotNet.LibBitTorrent.Downloads;
using ZeraldotNet.LibBitTorrent.Encrypters;
using ZeraldotNet.LibBitTorrent.Uploads;

namespace ZeraldotNet.LibBitTorrent.Connecters
{
    public interface IConnection
    {
        void Close();
        SingleDownload Download { get; set; }
        IEncryptedConnection EncryptedConnection { get; set; }
        bool GetAnything { get; }
        byte[] ID { get; }
        string IP { get; }
        bool Flushed { get; }
        bool IsLocallyInitiated { get; }
        void SendBitfield(bool[] bitfield);
        void SendCancel(int index, int begin, int length);
        void SendChoke();
        void SendHave(int index);
        void SendInterested();
        void SendNotInterested();
        void SendPiece(int index, int begin, byte[] pieces);
        void SendPort(ushort port);
        void SendRequest(int index, int begin, int length);
        void SendUnchoke();
        IUpload Upload { get; set; }
    }
}
