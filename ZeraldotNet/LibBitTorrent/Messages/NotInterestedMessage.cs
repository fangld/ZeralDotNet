using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// NotInterested网络信息类
    /// </summary>
    public class NotInterestedMessage : ChokeMessage
    {
        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public NotInterestedMessage()
            : this(null, null) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="encryptedConnection">封装连接类</param>
        /// <param name="connection">连接类</param>
        public NotInterestedMessage(IEncryptedConnection encryptedConnection, IConnection connection)
            : base(encryptedConnection, connection) { }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            //信息ID为3
            return Encode(MessageType.NotInterested);
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override bool Handle(byte[] buffer)
        {
            bool isDecodeSuccess = this.IsDecodeSuccess(buffer);
            if (isDecodeSuccess)
            {
               connection.Upload.GetNotInterested();
            }
            return isDecodeSuccess;
        }

        #endregion
    }
}
