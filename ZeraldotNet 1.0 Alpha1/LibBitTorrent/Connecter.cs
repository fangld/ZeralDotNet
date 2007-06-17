using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            byte firstByte = message[0];

            //如果已经获得BitField
            if (firstByte == (byte)BitTorrentMessage.BitField && conn.GotAnything)
            {
                connnection.Close();
                return;
            }

            conn.GotAnything = true;

            if ((firstByte == (byte)BitTorrentMessage.Choke || firstByte == (byte)BitTorrentMessage.Unchoke ||
                firstByte == (byte)BitTorrentMessage.Interested || firstByte == (byte)BitTorrentMessage.NotInterested)
                && message.Length != 1)
            {
                connnection.Close();
                return;
            }

            if (firstByte == (byte)BitTorrentMessage.Choke)
            {
                conn.Download.GotChoke();
            }

            else if (firstByte == (byte)BitTorrentMessage.Unchoke)
            {
                conn.Download.GotUnchoke();
                CheckEndgame();
            }

            else if (firstByte == (byte)BitTorrentMessage.Have)

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
