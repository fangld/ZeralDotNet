using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    public class HaveMessage : BitTorrentMessage
    {
        private int index;

        public int Index
        {
            get { return this.index; }
            set { this.index = value; }
        }

        public override bool Decode(byte[] buffer)
        {
            if (buffer.Length != BytesLength)
            {
                return false;
            }

            index = this.BytesToInt32(buffer, 1);

            return true;
        }

        public override byte[] Encode()
        {
            byte[] result = new byte[BytesLength];

            //信息ID为4
            result[0] = (byte)BitTorrentMessageType.Have;

            //写入片断索引号
            Int32ToBytes(index, result, 1);

            return result;
        }

        public override void Handle()
        {
            throw new NotImplementedException();
        }

        public override int BytesLength
        {
            get { return 5; }
        }

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

        /// <summary>
        /// 将字节流转换为32位有符号整数
        /// </summary>
        /// <param name="buffer">待读入的字节流</param>
        /// <param name="startOffset">待读入字节流的位置</param>
        /// <returns>返回32位有符号整数</returns>
        protected int BytesToInt32(byte[] buffer, int startOffset)
        {
            int result = 0x0;
            result |= ((int)buffer[startOffset]) << 24;
            result |= ((int)buffer[++startOffset]) << 16;
            result |= ((int)buffer[++startOffset]) << 8;
            result |= ((int)buffer[++startOffset]);
            return result;
        }
    }
}
