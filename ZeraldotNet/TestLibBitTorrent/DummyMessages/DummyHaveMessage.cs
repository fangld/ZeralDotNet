using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.TestLibBitTorrent.TestConnecter;
using ZeraldotNet.LibBitTorrent.Messages;
using ZeraldotNet.LibBitTorrent;

namespace ZeraldotNet.TestLibBitTorrent.DummyMessages
{
    /// <summary>
    /// Have网络信息类
    /// </summary>
    public class DummyHaveMessage : DummyMessage
    {

        #region Protected Field

        /// <summary>
        /// 片断索引号
        /// </summary>
        protected int index;

        /// <summary>
        /// 连接管理类
        /// </summary>
        protected DummyConnecter connecter;

        /// <summary>
        /// 连接类
        /// </summary>
        protected DummyConnection connection;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问片断索引号
        /// </summary>
        public int Index
        {
            get { return this.index; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public DummyHaveMessage(DummyEncryptedConnection encryptedConnection, DummyConnection connection, DummyConnecter connecter)
            : base(encryptedConnection)
        {
            this.connection = connection;
            this.connecter = connecter;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="index">片断索引号</param>
        public DummyHaveMessage(int index)
            : this(null, null, null)
        {
            this.index = index;
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            byte[] result = new byte[BytesLength];

            //信息ID为4
            result[0] = (byte)MessageType.Have;

            //写入片断索引号
            Globals.Int32ToBytes(index, result, 1);

            return result;
        }

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public override bool Decode(byte[] buffer)
        {
            //如果信息长度不等于5或者信息ID不为4，则返回false
            if (buffer.Length != BytesLength)
            {
                return false;
            }

            //解码片断索引
            index = Globals.BytesToInt32(buffer, 1);

            if (this.index > connecter.PiecesNumber)
            {
                return false;
            }

            //否则，返回true
            return true;
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override bool Handle(byte[] buffer)
        {
            bool isDecodeSuccess = this.IsDecodeSuccess(buffer);
            if (isDecodeSuccess)
            {
                this.connection.Download.GetHave(index);
                this.connecter.CheckEndgame();
            }
            return isDecodeSuccess;
        }

        /// <summary>
        /// 网络信息的长度
        /// </summary>
        public override int BytesLength
        {
            get { return 5; }
        }

        #endregion
    }
}
