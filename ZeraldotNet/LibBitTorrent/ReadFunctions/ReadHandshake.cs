using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.LibBitTorrent.ReadFunctions
{
    public class ReadHandshake : ReadLength
    {
        #region Fields

        /// <summary>
        /// 封装连接类
        /// </summary>
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
        private ReadHandshake(int length, ReadFunction next, IEncrypter encrypter, IEncryptedConnection encryptedConnection)
            : base(length, next, encrypter) 
        {
            this.encryptedConnection = encryptedConnection;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next">下一个分析字节流类</param>
        /// <param name="encrypter">封装连接器类</param>
        /// <param name="encryptedConnection">封装连接类</param>
        public ReadHandshake(ReadFunction next, IEncrypter encrypter, IEncryptedConnection encryptedConnection)
            : this(68, next, encrypter, encryptedConnection){}

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 分析字节流
        /// </summary>
        /// <param name="bytes">待分析的字节流</param>
        /// <returns>如果字节流正确，返回true，否则返回false</returns>
        public override bool ReadBytes(byte[] bytes)
        {
            //分析协议头

            //如果协议长度不等于19，返回false
            if (bytes[0] != Globals.ProtocolHeaderLength)
            {
                return false;
            }

            //如果得到的协议名不为"Bittorrent Protocol"，则返回false
            int index;
            for (index = 0; index < Globals.ProtocolHeader.Length;index++)
            {
                if (Globals.ProtocolHeader[index] != bytes[index + 1])
                {
                    return false;
                }
            }


            //分析下载文件ID
            //如果得到的下载文件ID不相同，则返回false
            for (index = 0; index < encrypter.DownloadID.Length; index++)
            {
                if (bytes[index + 28] != encrypter.DownloadID[index])
                {
                    return false;
                }
            }


            //分析节点ID
            byte[] id = encryptedConnection.ID;

            //如果封装连接类的ID号为空，则将bytes写入ID号
            if (encryptedConnection.ID == null)
            {
                encryptedConnection.ID = new byte[20];
                for (index = 0; index < encryptedConnection.ID.Length; index++)
                {
                    encryptedConnection.ID[index] = bytes[index + 48];
                }
            }

            //否则判断两者是否相等，相等返回true，否则返回false
            else
            {
                for (index = 0; index < id.Length; index++)
                {
                    if (bytes[index + 48] != id
                        [index])
                    {
                        return false;
                    }
                }
            }

            //进行连接
            encryptedConnection.Completed = true;
            encrypter.Connecter.MakeConnection(encryptedConnection);


            //否则返回true
            return true;
        }

        #endregion
    }
}
