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

        public HaveMessage() { }

        public HaveMessage(int index)
        {
            this.index = index;
        }

        public override bool Decode(byte[] buffer)
        {
            if (buffer.Length != BytesLength)
            {
                return false;
            }

            index = Globals.BytesToInt32(buffer, 1);

            return true;
        }

        public override byte[] Encode()
        {
            byte[] result = new byte[BytesLength];

            //信息ID为4
            result[0] = (byte)BitTorrentMessageType.Have;

            //写入片断索引号
            Globals.Int32ToBytes(index, result, 1);

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
    }
}
