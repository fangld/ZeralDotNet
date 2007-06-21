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

        public PortMessage() { }

        public PortMessage(ushort port)
        {
            this.port = port;
        }

        public override byte[] Encode()
        {
            byte[] result = new byte[3];

            //信息ID为9
            result[0] = (byte)BitTorrentMessageType.Port;

            //写入DHT监听端口
            Globals.UInt16ToBytes(port, result, 1);

            return result;
        }

        public override bool Decode(byte[] buffer)
        {
            if (buffer.Length != BytesLength)
            {
                return false;
            }

            port = Globals.BytesToUInt16(buffer, 1);

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


    }
}
