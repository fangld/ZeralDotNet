using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    public class ChokeMessage : Message
    {
        private static readonly byte[] Bytes = new byte[5] { 0x00, 0x00, 0x00, 0x01, 0x00 };
             
        public override byte[] Encode()
        {
            return Bytes;
        }

        public override bool Parse(byte[] buffer)
        {
            return true;
        }

        public override bool Parse(byte[] buffer, int offset, int count)
        {
            //if buffer is all zero, it is true, else it is false
            bool isByte1Right = (buffer[offset] == 0x00);
            bool isByte2Right = (buffer[offset + 1] == 0x00);
            bool isByte3Right = (buffer[offset + 2] == 0x00);
            bool isByte4Right = (buffer[offset + 3] == 0x01);
            bool isByte5Right = (buffer[offset + 4] == 0x00);
            return (isByte1Right & isByte2Right & isByte3Right & isByte4Right & isByte5Right);
        }

        public override bool Parse(System.IO.MemoryStream ms)
        {
            throw new NotImplementedException();
        }

        //public override bool Handle(byte[] buffer, int offset)
        //{
        //    throw new NotImplementedException();
        //}

        public override int BytesLength
        {
            get { return 1; }
        }

        public override MessageType Type
        {
            get { return MessageType.Choke; }
        }

        public override string ToString()
        {
            const string result = "Choke message";
            return result;
        }
    }
}
