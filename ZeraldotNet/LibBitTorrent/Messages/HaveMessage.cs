using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    public class HaveMessage : Message
    {
        #region Properties

        public int Index { get; set; }

        #endregion

        #region Constructors

        public HaveMessage()
        {
        }

        public HaveMessage(int index)
        {
            Index = index;
        }

        #endregion

        public override byte[] Encode()
        {
            byte[] result = new byte[BytesLength + 4];
            Globals.Int32ToBytes(BytesLength, result, 0);
            result[4] = (byte) Type;
            Globals.Int32ToBytes(Index, result, 5);
            return result;
        }

        public override bool Parse(byte[] buffer)
        {
            Index = Globals.BytesToInt32(buffer, 1);
            //_readIndex = BitConverter.ToInt32(buffer, 1);
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
            get { return 5; }
        }

        public override MessageType Type
        {
            get { return MessageType.Have; }
        }

        public override string ToString()
        {
            string result = string.Format("Have message: Index:{0}", Index);
            return result;
        }
    }
}
