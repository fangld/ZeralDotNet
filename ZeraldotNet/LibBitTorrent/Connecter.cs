﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.BitTorrentMessages;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate void StatusDelegate(string message, double downloadRate, double uploadRate, double fractionDone, double timeEstimate);
    public delegate void ErrorDelegate(string message);

    /// <summary>
    /// 连接管理类
    /// </summary>
    public class Connecter
    {
        #region Private Field

        private IDownloader downloader;

        private bool rateCapped;

        public bool RateCapped
        {
            get { return this.rateCapped; }
            set { this.rateCapped = value; }
        }

        private SchedulerDelegate scheduleFunction;

        private Dictionary<EncryptedConnection, Connection> connectionDictionary;

        private PendingDelegate isEverythingPending;

        private int piecesNumber;

        private Choker choker;

        private Measure totalUp;

        private int maxUploadRate;

        private bool endgame;

        #endregion

        #region Public Properties

        public int PieceNumber
        {
            get { return this.piecesNumber; }
        }

        public int ConnectionsCount
        {
            get { return connectionDictionary.Count; }
        }

        public ICollection<Connection> Connections
        {
            get { return this.connectionDictionary.Values; }
        }

        #endregion

        #region Constructors

        public Connecter(IDownloader downloader, Choker choker, int piecesNumber, PendingDelegate isEverythingPending,
            Measure totalUp, int maxUploadRate, SchedulerDelegate scheduleFunction)
        {
            this.downloader = downloader;
            this.choker = choker;
            this.piecesNumber = piecesNumber;
            this.isEverythingPending = isEverythingPending;
            this.maxUploadRate = maxUploadRate;
            this.scheduleFunction = scheduleFunction;
            this.totalUp = totalUp;
            this.rateCapped = false;
            this.connectionDictionary = new Dictionary<EncryptedConnection, Connection>();
            this.endgame = false;
            this.CheckEndgame();
        }

        #endregion

        #region Methods

        public void UpdateUploadRate(int amount)
        {
            totalUp.UpdateRate(amount);
            if (maxUploadRate > 0 && totalUp.NonUpdatedRate > maxUploadRate)
            {
                rateCapped = true;
                scheduleFunction(new TaskDelegate(UnCap), totalUp.TimeUntilRate(maxUploadRate), "Update Upload Rate");
            }
        }

        public void UnCap()
        {
            rateCapped = false;
            while (!rateCapped)
            {
                Upload upload = null;

                double minRate = 0;
                foreach (Connection item in connectionDictionary.Values)
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


        public void MakeConnection(EncryptedConnection connection)
        {
            Connection conn = new Connection(connection, this);
            connectionDictionary[connection] = conn;
            conn.Upload = MakeUpload(conn);
            conn.Download = downloader.MakeDownload(conn);
            choker.MakeConnection(conn);
        }

        public void LoseConnection(EncryptedConnection connection)
        {
            Connection conn = connectionDictionary[connection];
            ISingleDownload singleDownload = conn.Download;
            connectionDictionary.Remove(connection);
            singleDownload.Disconnect();
            choker.LoseConnection(conn);
        }

        public void FlushConnection(EncryptedConnection connection)
        {
            connectionDictionary[connection].Upload.Flush();
        }

        private Upload MakeUpload(Connection connection)
        {
            return new Upload(connection, Download.Choker, Download.StorageWrapper, Download.Parameters.MaxSliceLength,
                       Download.Parameters.MaxRatePeriod, Download.Parameters.UploadRateFudge);
        }

        public void GetMessage(EncryptedConnection connection, byte[] message)
        {
            Connection conn = connectionDictionary[connection];
            BitTorrentMessageType firstByte = (BitTorrentMessageType)message[0];

            //如果已经获得BitField
            if (firstByte == BitTorrentMessageType.BitField && conn.GotAnything)
            {
                connection.Close();
                return;
            }

            conn.GotAnything = true;

            if ((firstByte == BitTorrentMessageType.Choke || firstByte == BitTorrentMessageType.Unchoke ||
                firstByte == BitTorrentMessageType.Interested || firstByte == BitTorrentMessageType.NotInterested)
                && message.Length != 1)
            {
                connection.Close();
                return;
            }

            if (firstByte == BitTorrentMessageType.Choke)
            {
                conn.Download.GetChoke();
            }

            else if (firstByte == BitTorrentMessageType.Unchoke)
            {
                conn.Download.GetUnchoke();
                CheckEndgame();
            }

            else if (firstByte == BitTorrentMessageType.Interested)
            {
                conn.Upload.GetInterested();
            }

            else if (firstByte == BitTorrentMessageType.NotInterested)
            {
                conn.Upload.GetNotInterested();
            }

            else if (firstByte == BitTorrentMessageType.Have)
            {
                if (message.Length != 5)
                {
                    connection.Close();
                    return;
                }

                int index = Globals.BytesToInt32(message, 1);

                if (index > this.piecesNumber)
                {
                    connection.Close();
                    return;
                }

                conn.Download.GetHave(index);
                CheckEndgame();
            }

            else if (firstByte == BitTorrentMessageType.BitField)
            {
                bool[] booleans = BitField.FromBitField(message, 1, piecesNumber);
                if (booleans == null)
                {
                    connection.Close();
                    return;
                }
                conn.Download.GetHaveBitField(booleans);
                this.CheckEndgame();
            }

            else if (firstByte == BitTorrentMessageType.Request)
            {
                if (message.Length != 13)
                {
                    connection.Close();
                    return;
                }
                int index = Globals.BytesToInt32(message, 1);
                if (index >= piecesNumber)
                {
                    connection.Close();
                    return;
                }
                int begin = Globals.BytesToInt32(message, 5);
                int length = Globals.BytesToInt32(message, 9);
                conn.Upload.GetRequest(index, begin, length);
            }

            else if (firstByte == BitTorrentMessageType.Cancel)
            {
                if (message.Length != 13)
                {
                    connection.Close();
                    return;
                }
                int index = Globals.BytesToInt32(message, 1);
                if (index >= piecesNumber)
                {
                    connection.Close();
                    return;
                }
                int begin = Globals.BytesToInt32(message, 5);
                int length = Globals.BytesToInt32(message, 9);
                conn.Upload.GetCancel(index, begin, length);
            }

            else if (firstByte == BitTorrentMessageType.Piece)
            {
                if (message.Length <= 9)
                {
                    connection.Close();
                    return;
                }

                int index = Globals.BytesToInt32(message, 1);

                if (index >= piecesNumber)
                {
                    connection.Close();
                    return;
                }

                byte[] pieces = new byte[message.Length - 9];
                Globals.CopyBytes(message, 9, pieces);
                int begin = Globals.BytesToInt32(message, 5);
                if (conn.Download.GetPiece(index, begin, pieces))
                {
                    foreach (Connection item in connectionDictionary.Values)
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
                    connection.Close();
                    return;
                }

                ushort port = Globals.BytesToUInt16(message, 1);
            }

            else
            {
                connection.Close();
            }

        }

        public void CheckEndgame()
        {
            if (!endgame && isEverythingPending())
            {
                endgame = true;
                downloader = new EndgameDownloader(downloader);
            }
        }

        #endregion
    }
}
