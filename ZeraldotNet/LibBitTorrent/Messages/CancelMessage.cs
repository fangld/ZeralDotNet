using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    public class CancelMessage : HaveMessage
    {
        #region Protected Field

        /// <summary>
        /// 片断起始位置
        /// </summary>
        protected int begin; 

        /// <summary>
        /// 片断长度
        /// </summary>
        protected int length;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问和设置片断起始位置
        /// </summary>
        public int Begin
        {
            get { return this.begin; }
            set { this.begin = value; }
        }

        /// <summary>
        /// 访问和设置片断长度
        /// </summary>
        public int Length
        {
            get { return this.length; }
            set { this.length = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public CancelMessage(EncryptedConnection encryptedConnection, Connection connection, Connecter connecter)
            : base(encryptedConnection, connection, connecter) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">片断起始位置</param>
        /// <param name="length">片断长度</param>
        public CancelMessage(int index, int begin, int length)
            : base(index)
        {
            this.begin = begin;
            this.length = length;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 长度为13的网络信息编码函数
        /// </summary>
        /// <param name="type">网络信息类型</param>
        /// <returns>返回编码后的字节流</returns>
        protected byte[] Encode(MessageType type)
        {
            byte[] result = new byte[BytesLength];
            result[0] = (byte)type;

            //写入片断索引号
            Globals.Int32ToBytes(index, result, 1);

            //写入片断起始位置
            Globals.Int32ToBytes(begin, result, 5);

            //写入片断长度
            Globals.Int32ToBytes(length, result, 9);

            //返回解码的字节流
            return result;
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            return this.Encode(MessageType.Cancel);
        }

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public override bool Decode(byte[] buffer)
        {
            //如果长度不等于13，则返回false
            if (buffer.Length != BytesLength)
            {
                return false;
            }

            //解码片断索引
            index = Globals.BytesToInt32(buffer, 1);

            if (Index > this.Connecter.PiecesNumber)
            {
                return false;
            }

            //解码片断起始位置
            begin = Globals.BytesToInt32(buffer, 5);

            //解码片断长度
            length = Globals.BytesToInt32(buffer, 9);

            //否则返回true
            return true;
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override bool Handle(byte[] buffer)
        {
            //如果解码成功，则cancel相应子片断。
            bool isDecodeSuccess = this.IsDecodeSuccess(buffer);
            if (isDecodeSuccess)
            {
                Connection.Upload.GetCancel(index, begin, length);
            }

            //返回是否解码成功
            return isDecodeSuccess;
        }

        /// <summary>
        /// 网络信息的长度
        /// </summary>
        public override int BytesLength
        {
            get { return 13; }
        }

        #endregion
    }
}
