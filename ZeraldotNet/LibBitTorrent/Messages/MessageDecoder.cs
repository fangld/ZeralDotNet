using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// 网络信息解码器
    /// </summary>
    public class MessageDecoder
    {
        #region Methods

        /// <summary>
        /// 解码函数
        /// </summary>
        /// <param name="bytes">待解码的字节流</param>
        /// <param name="encryptedConnection">封装连接类</param>
        /// <param name="connection">连接类</param>
        /// <param name="connecter">连接管理器</param>
        /// <returns>如果解码成功，返回true，否则返回false</returns>
        public static bool Decode(byte[] bytes, IEncryptedConnection encryptedConnection, IConnection connection, IConnecter connecter)
        {
            //取得字节流首位
            MessageType firstByte = (MessageType)bytes[0];

            Message message;

            //根据字节流首位，决定处理哪种网络信息
            switch (firstByte)
            {
                case MessageType.BitField: message = new BitfieldMessage(encryptedConnection, connection, connecter); break;
                case MessageType.Choke: message = new ChokeMessage(encryptedConnection, connection); break;
                case MessageType.Unchoke: message = new UnchokeMessage(encryptedConnection,connection,connecter); break;
                case MessageType.Interested: message = new InterestedMessage(encryptedConnection,connection); break;
                case MessageType.NotInterested: message = new NotInterestedMessage(encryptedConnection,connection); break;
                case MessageType.Have: message = new HaveMessage(encryptedConnection,connection,connecter); break;
                case MessageType.Request: message = new RequestMessage(encryptedConnection,connection,connecter); break;
                case MessageType.Cancel: message = new CancelMessage(encryptedConnection,connection,connecter); break;
                case MessageType.Piece: message = new PieceMessage(encryptedConnection,connection,connecter); break;
                case MessageType.Port: message = new PortMessage(encryptedConnection); break;
                default: encryptedConnection.Close(); return false;
            }

            //处理网络信息
            return message.Handle(bytes);
        }

        #endregion
    }
}
