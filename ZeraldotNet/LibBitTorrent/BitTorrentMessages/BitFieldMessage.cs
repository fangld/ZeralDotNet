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
        /// 片断的数量
        /// </summary>
        private int pieceNumber;

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
        /// 访问和设置片断的数量
        /// </summary>
        public int PieceNumber
        {
            get { return this.pieceNumber; }
            set { this.pieceNumber = value; }
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
        /// <param name="booleans">片断的BitField信息</param>
        public BitFieldMessage(bool[] booleans)
        {
            this.booleans = booleans;
            this.pieceNumber = booleans.Length;
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
            //如果信息长度小于等于1或者信息ID不为7，则返回false
            if (buffer.Length <= 1 || buffer[0] != (byte)BitTorrentMessageType.BitField)
            {
                return false;
            }            

            //解码BitField信息
            booleans = BitField.FromBitField(buffer, 1, pieceNumber);

            //如果BitField信息为空，返回true
            if (booleans != null)
            {
                return true;
            }

            //否则，返回false
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override void Handle()
        {
            throw new NotImplementedException("The method or operation is not implemented.");
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override int BytesLength
        {
            get { return (pieceNumber >> 3) + 2; }
        }

        #endregion
    }
}
