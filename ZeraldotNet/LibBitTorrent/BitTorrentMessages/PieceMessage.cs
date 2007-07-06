using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    /// <summary>
    /// Piece网络信息类
    /// </summary>
    public class PieceMessage : HaveMessage
    {
        #region Private Field

        /// <summary>
        /// 片断起始位置
        /// </summary>
        private int begin;

        /// <summary>
        /// 片断数据
        /// </summary>
        private byte[] pieces;

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
        /// 访问和设置片断数据
        /// </summary>
        public byte[] Pieces
        {
            get { return this.pieces; }
            set { this.pieces = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public PieceMessage() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">片断起始位置</param>
        /// <param name="pieces">片断数据</param>
        public PieceMessage(int index, int begin, byte[] pieces)
        {
            this.Index = index;
            this.begin = begin;
            this.pieces = pieces;
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

            //信息ID为7
            result[0] = (byte)BitTorrentMessageType.Piece;

            //写入片断索引号
            Globals.Int32ToBytes(Index, result, 1);

            //写入子片断的起始位置
            Globals.Int32ToBytes(begin, result, 5);

            //写入子片断的数据
            pieces.CopyTo(result, 9);

            return result;
        }

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public override bool Decode(byte[] buffer)
        {
            //如果信息长度小于9或者信息ID不为7，则返回false
            if (buffer.Length <= 9)
            {
                return false;
            }

            //解码片断索引
            Index = Globals.BytesToInt32(buffer, 1);

            //解码片断起始位置
            begin = Globals.BytesToInt32(buffer, 5);

            //解码片断数据
            pieces = new byte[buffer.Length - 9];
            Globals.CopyBytes(buffer, 9, pieces);

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
                if (this.Connection.Download.GetPiece(Index, begin, pieces))
                {
                    foreach (Connection item in Connecter.Connections)
                    {
                        item.SendHave(Index);
                    }
                }
                Connecter.CheckEndgame();
            }
            return isDecodeSuccess;
        }

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public override int BytesLength
        {
            get { return 9 + pieces.Length; }
        }

        #endregion
    }
}
