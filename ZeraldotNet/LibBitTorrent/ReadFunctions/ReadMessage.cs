using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.LibBitTorrent.ReadFunctions
{
    /// <summary>
    /// 分析网络信息类
    /// </summary>
    public class ReadMessage : ReadLength
    {
        #region Fields

        private readonly IEncryptedConnection encryptedConnection;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="length">分析的长度</param>
        /// <param name="next">下一个分析字节流类</param>
        /// <param name="encrypter">封装连接器类</param>
        /// <param name="encryptedConnection">封装连接类</param>
        public ReadMessage(int length, ReadLength next, IEncrypter encrypter, IEncryptedConnection encryptedConnection)
            : base(length, next, encrypter) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="length">分析的长度</param>
        /// <param name="encrypter">封装连接器类</param>
        /// <param name="encryptedConnection">封装连接类</param>
        public ReadMessage(int length, IEncrypter encrypter, IEncryptedConnection encryptedConnection)
            : base(length, null, encrypter) { }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 分析字节流
        /// </summary>
        /// <param name="bytes">待分析的字节流</param>
        /// <returns>如果字节流正确，返回true，否则返回false</returns>
        public override bool ReadBytes(byte[] bytes)
        {
            //进行读取数据
            try
            {
                if (bytes.Length > 0)
                {
                    encrypter.Connecter.GetMessage(encryptedConnection, bytes);
                }
            }
            catch { }
            return true;
        }

        #endregion
    }
}
