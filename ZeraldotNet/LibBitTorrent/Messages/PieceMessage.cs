using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    public class PieceMessage : Message
    {
        #region Fields

        private byte[] _block;

        #endregion

        #region Propereties

        public int Index { get; set; }

        public int Begin { get; set; }

        #endregion

        #region Constructors

        public PieceMessage()
        {}

        public PieceMessage(int index, int begin, byte[] pieces)
        {
            Index = index;
            Begin = begin;
            _block = pieces;
        }


        #endregion

        #region Override Methods

        public void CopyPiece(byte[] piece)
        {
            Buffer.BlockCopy(piece, 0, _block, 0, piece.Length);
        }

        public override byte[] Encode()
        {
            byte[] result = new byte[BytesLength];
            Globals.Int32ToBytes(BytesLength, result, 0);
            result[4] = (byte) Type;
            Globals.Int32ToBytes(Index, result, 5);
            Globals.Int32ToBytes(Begin, result, 9);
            Buffer.BlockCopy(_block, 0, result, 13, _block.Length);
            return result;
        }

        public override bool Parse(byte[] buffer)
        {
            int bytesLength = Globals.BytesToInt32(buffer, 0);
            int blockLength = bytesLength - 9;
            Index = Globals.BytesToInt32(buffer, 5);
            Begin = Globals.BytesToInt32(buffer, 9);
            _block = new byte[blockLength];
            Buffer.BlockCopy(buffer, 13, _block, 0, blockLength);
            return true;
        }

        public override bool Parse(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override bool Parse(MemoryStream ms)
        {
            throw new NotImplementedException();
        }

        //public override bool Handle(byte[] buffer, int offset)
        //{
        //    throw new NotImplementedException();
        //}

        #endregion

        public override int BytesLength
        {
            get { return _block.Length + 9; }
        }

        public override MessageType Type
        {
            get { return MessageType.Piece; }
        }
    }
}
