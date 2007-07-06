using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.ReadFunctions
{
    /// <summary>
    /// 分析网络信息类
    /// </summary>
    public class ReadMessage : ReadPeerID
    {
        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="length">分析的长度</param>
        /// <param name="next">下一个分析字节流类</param>
        /// <param name="encrypter">封装连接器类</param>
        /// <param name="encryptedConnection">封装连接类</param>
        public ReadMessage(int length, ReadLength next, Encrypter encrypter, EncryptedConnection encryptedConnection)
            : base(length, next, encrypter, encryptedConnection) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="length">分析的长度</param>
        /// <param name="encrypter">封装连接器类</param>
        /// <param name="encryptedConnection">封装连接类</param>
        public ReadMessage(int length, Encrypter encrypter, EncryptedConnection encryptedConnection)
            : base(length, null, encrypter, encryptedConnection) { }

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
                    Encrypter.Connecter.GetMessage(EncryptedConnection, bytes);
                }
            }
            catch { }
            return true;
        }

        #endregion
    }
}
