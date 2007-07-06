using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    /// <summary>
    /// BitField网络信息类
    /// </summary>
    public class BitFieldMessage : BitTorrentMessage
    {
        #region Private Field

        /// <summary>
        /// 片断的BitField信息
        /// </summary>
        private bool[] booleans;

        /// <summary>
        /// 连接类
        /// </summary>
        private Connection connection;

        /// <summary>
        /// 连接管理类
        /// </summary>
        private Connecter connecter;

        /// <summary>
        /// 网络信息的字节长度
        /// </summary>
        private int bytesLength;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问和设置片断的BitField信息
        /// </summary>
        public bool[] Booleans
        {
            get { return this.booleans; }
            set { this.booleans = value; }
        }

        /// <summary>
        /// 访问和设置连接类
        /// </summary>
        public Connection Connection
        {
            get { return this.connection; }
            set { this.connection = value; }
        }

        /// <summary>
        /// 访问和设置连接管理类
        /// </summary>
        public Connecter Connecter
        {
            get { return this.connecter; }
            set { this.connecter = value; }
        }

        #endregion

        #region Construcotrs

        /// <summary>
        /// 构造函数
        /// </summary>
        public BitFieldMessage() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="pieceNumber">片断的数量</param>
        public BitFieldMessage(int pieceNumber)
        {
            bytesLength = pieceNumber >> 3;
            bytesLength++;
            if ((pieceNumber & 7) != 0)
            {
                bytesLength++;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="booleans">片断的BitField信息</param>
        public BitFieldMessage(bool[] booleans) : this(booleans.Length)
        {
            this.booleans = booleans;
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public override byte[] Encode()
        {
            byte[] bitFieldBytes = BitField.ToBitField(booleans);

            this.bytesLength = bitFieldBytes.Length;

            byte[] result = new byte[bitFieldBytes.Length + 1];

            //信息ID为5
            result[0] = (byte)BitTorrentMessageType.BitField;

            //写入BitField
            bitFieldBytes.CopyTo(result, 1);

            return result;
        }

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public override bool Decode(byte[] buffer)
        {
            //如果信息长度不等于所需字节长度，则返回false
            if (buffer.Length != BytesLength)
            {
                return false;
            }

            //解码BitField信息
            booleans = BitField.FromBitField(buffer, 1, connecter.PieceNumber);

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
                connection.Download.GetHaveBitField(booleans);
                connecter.CheckEndgame();
            }
            return isDecodeSuccess;
        }

        /// <summary>
        /// 网络信息的字节长度
        /// </summary>
        public override int BytesLength
        {
            get { return this.bytesLength; }
        }

        #endregion
    }
}
