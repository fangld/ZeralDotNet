using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    public class BitTorrentMessageDecoder
    {
        #region Private Field

        /// <summary>
        /// 连接类
        /// </summary>
        private Connection connection;

        /// <summary>
        /// 封装连接类
        /// </summary>
        private EncryptedConnection encryptedConnection;

        /// <summary>
        /// 连接管理类
        /// </summary>
        private Connecter connecter;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connection">连接类</param>
        /// <param name="encryptedConnection">封装连接类</param>
        /// <param name="connecter">连接管理类</param>
        public BitTorrentMessageDecoder(Connection connection, EncryptedConnection encryptedConnection, Connecter connecter)
        {
            this.connection = connection;
            this.encryptedConnection = encryptedConnection;
            this.connecter = connecter;
        }

        #endregion

        #region Methods

        public bool Decode(byte[] bytes)
        {
            BitTorrentMessageType firstByte = (BitTorrentMessageType)bytes[0];

            BitTorrentMessage message;

            switch (firstByte)
            {
                case BitTorrentMessageType.BitField: message = new BitFieldMessage(); break;
                case BitTorrentMessageType.Choke: message = new ChokeMessage(); break;
                case BitTorrentMessageType.Unchoke: message = new UnchokeMessage(); break;
                case BitTorrentMessageType.Interested: message = new InterestedMessage(); break;
                case BitTorrentMessageType.NotInterested: message = new NotInterestedMessage(); break;
                case BitTorrentMessageType.Have: message = new HaveMessage(); break;
                case BitTorrentMessageType.Request: message = new RequestMessage(); break;
                case BitTorrentMessageType.Cancel: message = new CancelMessage(); break;
                case BitTorrentMessageType.Piece: message = new PieceMessage(); break;
                case BitTorrentMessageType.Port: message = new PortMessage(); break;
                default: encryptedConnection.Close(); return false;
            }

            return message.Handle(bytes);

            //if (firstByte == BitTorrentMessageType.BitField && connection.GotAnything)
            //{
            //    encryptedConnection.Close();
            //    return;
            //}

            //connection.GotAnything = true;

            

            //if ((firstByte == BitTorrentMessageType.Choke || firstByte == BitTorrentMessageType.Unchoke ||
            //    firstByte == BitTorrentMessageType.Interested || firstByte == BitTorrentMessageType.NotInterested)
            //    && message.Length != 1)
            //{
            //    encryptedconnectionection.Close();
            //    return;
            //}

            //if (firstByte == BitTorrentMessageType.Choke)
            //{
            //    connection.Download.GetChoke();
            //}

            //else if (firstByte == BitTorrentMessageType.Unchoke)
            //{
            //    connection.Download.GetUnchoke();
            //    connecter.CheckEndgame();
            //}

            //else if (firstByte == BitTorrentMessageType.Interested)
            //{
            //    connection.Upload.GetInterested();
            //}

            //else if (firstByte == BitTorrentMessageType.NotInterested)
            //{
            //    connection.Upload.GetNotInterested();
            //}

            //else if (firstByte == BitTorrentMessageType.Have)
            //{
            //    if (message.Length != 5)
            //    {
            //        encryptedconnectionection.Close();
            //        return;
            //    }

            //    int index = Globals.BytesToInt32(message, 1);

            //    if (index > this.piecesNumber)
            //    {
            //        encryptedconnectionection.Close();
            //        return;
            //    }

            //    connection.Download.GetHave(index);
            //    connecter.CheckEndgame();
            //}

            //else if (firstByte == BitTorrentMessageType.BitField)
            //{
            //    bool[] booleans = BitField.FromBitField(message, 1, piecesNumber);
            //    if (booleans == null)
            //    {
            //        encryptedconnectionection.Close();
            //        return;
            //    }
            //    connection.Download.GetHaveBitField(booleans);
            //    connecter.CheckEndgame();
            //}

            //else if (firstByte == BitTorrentMessageType.Request)
            //{
            //    if (message.Length != 13)
            //    {
            //        encryptedconnectionection.Close();
            //        return;
            //    }
            //    int index = Globals.BytesToInt32(message, 1);
            //    if (index >= piecesNumber)
            //    {
            //        encryptedconnectionection.Close();
            //        return;
            //    }
            //    int begin = Globals.BytesToInt32(message, 5);
            //    int length = Globals.BytesToInt32(message, 9);
            //    connection.Upload.GetRequest(index, begin, length);
            //}

            //else if (firstByte == BitTorrentMessageType.Cancel)
            //{
            //    if (message.Length != 13)
            //    {
            //        encryptedconnectionection.Close();
            //        return;
            //    }
            //    int index = Globals.BytesToInt32(message, 1);
            //    if (index >= piecesNumber)
            //    {
            //        encryptedconnectionection.Close();
            //        return;
            //    }
            //    int begin = Globals.BytesToInt32(message, 5);
            //    int length = Globals.BytesToInt32(message, 9);
            //    connection.Upload.GetCancel(index, begin, length);
            //}

            //else if (firstByte == BitTorrentMessageType.Piece)
            //{
            //    if (message.Length <= 9)
            //    {
            //        encryptedconnectionection.Close();
            //        return;
            //    }

            //    int index = Globals.BytesToInt32(message, 1);

            //    if (index >= piecesNumber)
            //    {
            //        encryptedconnectionection.Close();
            //        return;
            //    }

            //    byte[] pieces = new byte[message.Length - 9];
            //    Globals.CopyBytes(message, 9, pieces);
            //    int begin = Globals.BytesToInt32(message, 5);
            //    if (connection.Download.GetPiece(index, begin, pieces))
            //    {
            //        foreach (encryptedconnectionection item in encryptedconnectionections.Values)
            //        {
            //            item.SendHave(index);
            //        }
            //    }
            //    connecter.CheckEndgame();
            //}

            //else if (firstByte == BitTorrentMessageType.Port)
            //{
            //    //还没有实现
            //    if (message.Length != 3)
            //    {
            //        encryptedconnectionection.Close();
            //        return;
            //    }

            //    ushort port = Globals.BytesToUInt16(message, 1);
            //}

            //else
            //{
            //    encryptedconnectionection.Close();
            //}
        }

        #endregion
    }
}
