using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Request网络信息类
    /// </summary>
    public class RequestMessage : CancelMessage
    {
        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public RequestMessage(IEncryptedConnection encryptedConnection, IConnection connection, IConnecter connecter)
            : base(encryptedConnection, connection, connecter) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">片断起始位置</param>
        /// <param name="length">片断长度</param>
        public RequestMessage(int index, int begin, int length) 
            : base(index, begin, length) { }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            return this.Encode(MessageType.Request);
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override bool Handle(byte[] buffer)
        {
            //如果
            bool isDecodeSuccess = this.IsDecodeSuccess(buffer);
            if (isDecodeSuccess)
            {
                connection.Upload.GetRequest(index, begin, length);
            }
            return isDecodeSuccess;
        }

        #endregion
    }
}
