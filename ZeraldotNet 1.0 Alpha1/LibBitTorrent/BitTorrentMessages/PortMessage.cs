using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    public class PortMessage : BitTorrentMessage
    {
        private ushort port;

        public ushort Port
        {
            get { return port; }
            set { port = value; }
        }

        public override byte[] Encode()
        {
            byte[] result = new byte[3];

            //信息ID为9
            result[0] = (byte)BitTorrentMessageType.Port;

            //写入DHT监听端口
            UInt16ToBytes(port, result, 1);

            return result;
        }

        public override bool Decode(byte[] buffer)
        {
            if (buffer.Length != BytesLength)
            {
                return false;
            }

            port = BytesToUInt16(buffer, 1);

            return true;
        }

        public override void Handle()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override int BytesLength
        {
            get { return 3; }
        }

        /// <summary>
        /// 将16位无符号整数写入字节流
        /// </summary>
        /// <param name="value">需要写入的16位无符号整数</param>
        /// <param name="buffer">待写入的字节流</param>
        /// <param name="startIndex">写入字节流的位置</param>
        private void UInt16ToBytes(ushort value, byte[] buffer, int startIndex)
        {
            buffer[startIndex] = (byte)(value >> 8);
            buffer[++startIndex] = (byte)(value & 0xFF);
        }

        private ushort BytesToUInt16(byte[] buffer, int startOffset)
        {
            ushort result = 0x0;
            result |= ((ushort)buffer[startOffset]);
            result <<= 8;
            result |= ((ushort)buffer[++startOffset]);
            return result;
        }



    }
}
