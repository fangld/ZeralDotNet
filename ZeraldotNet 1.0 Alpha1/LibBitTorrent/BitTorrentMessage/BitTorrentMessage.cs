using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessage
{
    public abstract class BitTorrentMessage
    {
        public virtual byte[] Encode();

        public virtual bool Decode(byte[] buffer);

        public virtual void Handle();

        public virtual int BytesLength { get; }

        /// <summary>
        /// 将32位有符号整数写入字节流
        /// </summary>
        /// <param name="value">需要写入的32位有符号整数</param>
        /// <param name="buffer">待写入的字节流</param>
        /// <param name="startIndex">写入字节流的位置</param>
        protected void Int32ToBytes(int value, byte[] buffer, int startIndex)
        {
            buffer[startIndex] = (byte)(value >> 24);
            buffer[++startIndex] = (byte)((value >> 16) & 0xFF);
            buffer[++startIndex] = (byte)((value >> 8) & 0xFFFF);
            buffer[++startIndex] = (byte)(value & 0xFFFFFF);
        }
    }
}
