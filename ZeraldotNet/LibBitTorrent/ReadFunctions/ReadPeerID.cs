using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.ReadFunctions
{
    /// <summary>
    /// 分析对方ID号类
    /// </summary>
    public class ReadPeerID : ReadLength
    {
        #region Private Field

        /// <summary>
        /// 封装连接类
        /// </summary>
        private EncryptedConnection encryptedConnection;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问和设置封装连接类
        /// </summary>
        public EncryptedConnection EncryptedConnection
        {
            get { return this.encryptedConnection; }
            set { this.encryptedConnection = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="length">分析的长度</param>
        /// <param name="next">下一个分析字节流类</param>
        /// <param name="encrypter">封装连接器类</param>
        /// <param name="encryptedConnection">封装连接类</param>
        protected ReadPeerID(int length, ReadFunction next, Encrypter encrypter, EncryptedConnection encryptedConnection)
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
        public ReadPeerID(ReadFunction next, Encrypter encrypter, EncryptedConnection encryptedConnection)
            : this(20, next, encrypter, encryptedConnection){}

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 分析字节流
        /// </summary>
        /// <param name="bytes">待分析的字节流</param>
        /// <returns>如果字节流正确，返回true，否则返回false</returns>
        public override bool ReadBytes(byte[] bytes)
        {
            int i;

            byte[] id = encryptedConnection.ID;

            //如果封装连接类的ID号为空，则将bytes写入ID号
            if (id == null)
            {
                id = new byte[20];
                for (i = 0; i < id.Length; i++)
                {
                    id[i] = bytes[i];
                }
            }

            //否则判断两者是否相等，相等返回true，否则返回false
            else
            {
                for (i = 0; i < 20; i++)
                {
                    if (bytes[i] != id[i])
                    {
                        return false;
                    }
                }
            }

            //进行连接
            encryptedConnection.Complete = true;
            Encrypter.Connecter.MakeConnection(encryptedConnection);
            return true;
        }

        #endregion
    }
}
