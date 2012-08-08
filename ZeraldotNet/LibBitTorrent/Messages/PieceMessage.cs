using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Piece message
    /// </summary>
    public class PieceMessage : Message
    {
        #region Fields

        /// <summary>
        /// The content of block
        /// </summary>
        private byte[] _block;

        private const int HeadLength = 9;

        #endregion

        #region Propereties

        /// <summary>
        /// The index of piece
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The begin position of block
        /// </summary>
        public int Begin { get; set; }

        public override int BytesLength
        {
            get { return _block.Length + 13; }
        }

        public override MessageType Type
        {
            get { return MessageType.Piece; }
        }

        #endregion

        #region Constructors

        public PieceMessage()
        {}

        public PieceMessage(int index, int begin, byte[] block)
        {
            Index = index;
            Begin = begin;
            _block = block;
        }


        #endregion

        #region Methods

        /// <summary>
        /// Return the block
        /// </summary>
        /// <returns>the block</returns>
        public byte[] GetBlock()
        {
            return _block;
        }

        /// <summary>
        /// Get the array of byte that corresponds the message
        /// </summary>
        /// <returns>Return the array of byte</returns>
        public override byte[] GetByteArray()
        {
            byte[] result = new byte[BytesLength];
            SetBytesLength(result, BytesLength - 4);
            result[4] = (byte) Type;
            Globals.Int32ToBytes(Index, result, 5);
            Globals.Int32ToBytes(Begin, result, 9);
            Buffer.BlockCopy(_block, 0, result, 13, _block.Length);
            return result;
        }

        /// <summary>
        /// Parse the array of byte to the message
        /// </summary>
        /// <param name="buffer">the array of byte</param>
        /// <returns>Return whether parse successfully</returns>
        public override bool Parse(byte[] buffer)
        {
            int blockLength = buffer.Length - HeadLength;
            Index = Globals.BytesToInt32(buffer, 1);
            Begin = Globals.BytesToInt32(buffer, 5);
            _block = new byte[blockLength];
            Buffer.BlockCopy(buffer, 9, _block, 0, blockLength);
            return true;
        }

        public override bool Parse(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            string result = string.Format("Piece {0}:{1}->{2}", Index, Begin, _block.Length);
            return result;
        }

        #endregion
    }
}
