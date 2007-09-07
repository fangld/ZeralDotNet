using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// 网络信息基类
    /// </summary>
    public abstract class Message
    {
        #region Fields

        /// <summary>
        /// 封装连接类
        /// </summary>
        protected IEncryptedConnection encryptedConnection;

        #endregion

        #region Properties

        /// <summary>
        /// 访问和设置封装连接类
        /// </summary>
        public IEncryptedConnection EncryptedConnection
        {
            get { return this.encryptedConnection; }
            set { this.encryptedConnection = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public Message() : this(null) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="encryptedConnection">封装连接类</param>
        public Message(IEncryptedConnection encryptedConnection)
        {
            this.encryptedConnection = encryptedConnection;
        }

        #endregion

        #region Base Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public abstract byte[] Encode();

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public abstract bool Decode(byte[] buffer);

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public abstract bool Handle(byte[] buffer);

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public abstract int BytesLength { get; }

        #endregion

        #region Methods

        /// <summary>
        /// 判断是否解码成功
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>如果解码成功，返回true，否则返回false</returns>
        public bool IsDecodeSuccess(byte[] buffer)
        {
            if (!Decode(buffer))
            {
                this.encryptedConnection.Close();
                return false;
            }
            return true;
        }

        #endregion
    }
}
