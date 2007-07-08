using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Messages;
using ZeraldotNet.TestLibBitTorrent.TestConnecter;


namespace ZeraldotNet.TestLibBitTorrent.DummyMessages
{
    /// <summary>
    /// 网络信息解码器
    /// </summary>
    public class DummyMessageDecoder
    {
        #region Methods

        /// <summary>
        /// 解码函数
        /// </summary>
        /// <param name="bytes">待解码的字节流</param>
        /// <returns>如果解码成功，返回true，否则返回false</returns>
        public static bool Decode(byte[] bytes, DummyEncryptedConnection encryptedConnection, DummyConnection connection, DummyConnecter connecter)
        {
            //取得字节流首位
            MessageType firstByte = (MessageType)bytes[0];

            DummyMessage message;

            //根据字节流首位，决定处理哪种网络信息
            switch (firstByte)
            {
                case MessageType.BitField: message = new DummyBitfieldMessage(encryptedConnection, connection, connecter); break;
                case MessageType.Choke: message = new DummyChokeMessage(encryptedConnection, connection); break;
                case MessageType.Unchoke: message = new DummyUnchokeMessage(encryptedConnection, connection, connecter); break;
                case MessageType.Interested: message = new DummyInterestedMessage(encryptedConnection, connection); break;
                case MessageType.NotInterested: message = new DummyNotInterestedMessage(encryptedConnection, connection); break;
                case MessageType.Have: message = new DummyHaveMessage(encryptedConnection, connection, connecter); break;
                case MessageType.Request: message = new DummyRequestMessage(encryptedConnection, connection, connecter); break;
                case MessageType.Cancel: message = new DummyCancelMessage(encryptedConnection, connection, connecter); break;
                case MessageType.Piece: message = new DummyPieceMessage(encryptedConnection, connection, connecter); break;
                case MessageType.Port: message = new DummyPortMessage(encryptedConnection); break;
                default: encryptedConnection.Close(); return false;
            }

            //处理网络信息
            return message.Handle(bytes);
        }

        #endregion
    }
}
