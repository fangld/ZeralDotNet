using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    public class BitFieldMessage : BitTorrentMessage
    {
        private bool[] booleans;

        private int pieceNumber;

        public int PieceNumber
        {
            get { return this.pieceNumber; }
            set { this.pieceNumber = value; }
        }

        public BitFieldMessage(int pieceNumer) 
        {
            this.PieceNumber = pieceNumber;
        }

        public BitFieldMessage(bool[] booleans)
        {
            this.booleans = booleans;
            this.pieceNumber = booleans.Length;
        }

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

        public override bool Decode(byte[] buffer)
        {
            booleans = BitField.FromBitField(buffer, 1, pieceNumber);

            if (booleans != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override void Handle()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override int BytesLength
        {
            get { return (pieceNumber >> 3) + 2; }
        }
    }
}
