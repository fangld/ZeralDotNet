using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.BitTorrentMessages;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate void StatusDelegate(string message, double downloadRate, double uploadRate, double fractionDone, double timeEstimate);
    public delegate void ErrorDelegate(string message);

    public class Connecter
    {
        private IDownloader downloader;

        private bool rateCapped;

        public bool RateCapped
        {
            get { return this.rateCapped; }
            set { this.rateCapped = value; }
        }

        private SchedulerDelegate scheduleFunction;

        private Dictionary<EncryptedConnection, Connection> connections;

        private PendingDelegate isEverythingPending;

        private int pieceNumber;

        private Choker choker;

        private Measure totalUp;

        private int maxUploadRate;

        private bool endgame;

        public void UpdateUploadRate(int amount)
        {

            throw new NotImplementedException();
            totalUp.UpdateRate(amount);
            if (maxUploadRate > 0 && totalUp.NonUpdatedRate > maxUploadRate)
            {
                rateCapped = true;

            }
        }

        public void UnCap()
        {
            rateCapped = false;
            while (!rateCapped)
            {
                Upload upload = null;

                double minRate = 0;
                foreach (Connection item in connections.Values)
                {
                    if (!item.Upload.Choked && item.Upload.HasQueries() && item.EncryptedConnection.IsFlushed)
                    {
                        double rate = item.Upload.Rate;
                        if (upload == null || rate < minRate)
                        {
                            upload = item.Upload;
                            minRate = rate;
                        }
                    }
                }

                if (upload == null)
                {
                    break;
                }

                upload.Flush();
                if (totalUp.NonUpdatedRate > maxUploadRate)
                {
                    break;
                }
            }
        }

        public int ConnectionsCount
        {
            get { return connections.Count; }
        }

        public void MakeConnection(EncryptedConnection connection)
        {
            Connection conn = new Connection(connection, this);
            connections[connection] = conn;
            conn.Upload = MakeUpload(conn);
            conn.Download = downloader.MakeDownload(conn);
            choker.MakeConnection(conn);
        }

        public void LoseConnection(EncryptedConnection connection)
        {
            Connection conn = connections[connection];
            ISingleDownload singleDownload = conn.Download;
            connections.Remove(connection);
            singleDownload.Disconnected();
            choker.LoseConnection(conn);
        }

        private Upload MakeUpload(Connection connection)
        {
            return new Upload(connection, Download.Choker, Download.StorageWrapper, Download.Parameters.MaxSliceLength,
                       Download.Parameters.MaxRatePeriod, Download.Parameters.UploadRateFudge);
        }

        public void GetMessage(EncryptedConnection connnection, byte[] message)
        {
            Connection conn = connections[connnection];
            BitTorrentMessageType firstByte = (BitTorrentMessageType)message[0];

            //如果已经获得BitField
            if (firstByte == BitTorrentMessageType.BitField && conn.GotAnything)
            {
                connnection.Close();
                return;
            }

            conn.GotAnything = true;

            if ((firstByte == BitTorrentMessageType.Choke || firstByte == BitTorrentMessageType.Unchoke ||
                firstByte == BitTorrentMessageType.Interested || firstByte == BitTorrentMessageType.NotInterested)
                && message.Length != 1)
            {
                connnection.Close();
                return;
            }

            if (firstByte == BitTorrentMessageType.Choke)
            {
                conn.Download.GotChoke();
            }

            else if (firstByte == BitTorrentMessageType.Unchoke)
            {
                conn.Download.GotUnchoke();
                CheckEndgame();
            }

            else if (firstByte == BitTorrentMessageType.Have)
            {
                if (message.Length != 5)
                {
                    connnection.Close();
                    return;
                }

                int index = BytesToInt32(message, 1);

                if (index > this.pieceNumber)
                {
                    connnection.Close();
                    return;
                }

                conn.Download.GotHave(index);
                CheckEndgame();
            }

            else if (firstByte == BitTorrentMessageType.BitField)
            {
                bool[] booleans = BitField.FromBitField(message, 1, pieceNumber);
                if (booleans == null)
                {
                    connnection.Close();
                    return;
                }
                conn.Download.GotHaveBitField(booleans);
                CheckEndgame();
            }

            else if (firstByte == BitTorrentMessageType.Cancel)
            {
                if (message.Length != 13)
                {
                    connnection.Close();
                    return;
                }
                int index = BytesToInt32(message, 1);
                if (index >= pieceNumber)
                {
                    connnection.Close();
                    return;
                }
                int begin = BytesToInt32(message, 5);
                int length = BytesToInt32(message, 9);
                conn.Upload.GotCancel(index, begin, length);
            }

            else if (firstByte == BitTorrentMessageType.Piece)
            {
                if (message.Length <= 9)
                {
                    connnection.Close();
                    return;
                }

                int index = BytesToInt32(message, 1);

                if (index >= pieceNumber)
                {
                    connnection.Close();
                    return;
                }

                byte[] pieces = new byte[message.Length - 9];
                Globals.CopyBytes(message, 9, pieces);
                int begin = BytesToInt32(message, 5);
                if (conn.Download.GotPiece(index, begin, pieces))
                {
                    foreach (Connection item in connections.Values)
                    {
                        item.SendHave(index);
                    }
                }
                CheckEndgame();
            }

            else if (firstByte == BitTorrentMessageType.Port)
            {
                //还没有实现
                if (message.Length != 3)
                {
                    connnection.Close();
                    return;
                }

                ushort port = BytesToUInt16(message, 1);
            }

            else
            {
                connnection.Close();
            }

        }

        private int BytesToInt32(byte[] buffer, int startOffset)
        {
            int result = 0x0;
            result |= ((int)buffer[startOffset]) << 24;
            result |= ((int)buffer[++startOffset]) << 16;
            result |= ((int)buffer[++startOffset]) << 8;
            result |= ((int)buffer[++startOffset]);
            return result;
        }

        private ushort BytesToUInt16(byte[] buffer, int startOffset)
        {
            ushort result = 0x0;
            result |= ((ushort)buffer[startOffset]);
            result <<= 8;
            result |= ((ushort)buffer[++startOffset]);
            return result;
        }

        public void CheckEndgame()
        {
            if (!endgame && isEverythingPending())
            {
                endgame = true;
                downloader = new EndgameDownloader(downloader);
            }
        }
    }
}
