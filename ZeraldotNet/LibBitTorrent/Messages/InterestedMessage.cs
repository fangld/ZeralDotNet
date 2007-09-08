using ZeraldotNet.LibBitTorrent.Encrypters;
using ZeraldotNet.LibBitTorrent.Connecters;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Interested网络信息类
    /// </summary>
    public class InterestedMessage : ChokeMessage
    {
        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public InterestedMessage()
            : this(null, null) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="encryptedConnection">封装连接类</param>
        /// <param name="connection">连接类</param>
        public InterestedMessage(IEncryptedConnection encryptedConnection, IConnection connection)
            : base(encryptedConnection, connection) { }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            //信息ID为2
            return Encode(MessageType.Interested);
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        /// <param name="buffer">待处理的字节流</param>
        /// <returns>返回是否处理成功</returns>
        public override bool Handle(byte[] buffer)
        {
            //如果解码成功，则interested上传者。
            bool isDecodeSuccess = this.IsDecodeSuccess(buffer);
            if (isDecodeSuccess)
            {
                connection.Upload.GetInterested();
            }

            //返回是否解码成功
            return isDecodeSuccess;
        }

        #endregion
    }
}
