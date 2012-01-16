using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// 网络信息基类
    /// </summary>
    public abstract class Message
    {
        #region Constructors

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

        public abstract bool Decode(byte[] buffer, int offset, int count);

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="ms">待解码的内存流</param>
        /// <returns>返回是否解码成功</returns>
        public abstract bool Decode(MemoryStream ms);

        ///// <summary>
        ///// 网络信息的处理函数
        ///// </summary>
        //public abstract bool Handle(byte[] buffer, int offset);

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public abstract int BytesLength { get; }

        public static void SetBytesLength(byte[] bytes, int length)
        {
            bytes[0] = (byte)(length >> 24);
            bytes[1] = (byte)(length >> 16);
            bytes[2] = (byte)(length >> 8);
            bytes[3] = (byte)(length);
        }

        public static int GetLength(byte[] bytes, int offset)
        {
            int result = 0;
            result |= bytes[offset];
            result |= (bytes[offset + 1] << 8);
            result |= (bytes[offset + 2] << 16);
            result |= (bytes[offset + 3] << 24);
            return result;
        }

        public abstract MessageType Type { get; }

        #endregion

        #region Methods

        #endregion
    }
}
