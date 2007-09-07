namespace ZeraldotNet.LibBitTorrent.Encrypters
{
    public interface IEncryptedConnection
    {
        void Close();
        bool Closed { get; set; }
        bool Completed { get; set; }
        void DataCameIn(byte[] bytes);
        byte[] ID { get; set; }
        string IP { get; }
        bool IsFlushed { get; }
        bool IsLocallyInitiated { get; }
        void SendMessage(byte[] message);
        void SendMessage(byte message);
        void Server();
    }
}
