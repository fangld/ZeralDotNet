using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Have message
    /// </summary>
    public class HaveMessage : Message
    {
        #region Properties

        /// <summary>
        /// The index of piece
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The length of message
        /// </summary>
        public override int BytesLength
        {
            get { return 9; }
        }

        /// <summary>
        /// The type of message
        /// </summary>
        public override MessageType Type
        {
            get { return MessageType.Have; }
        }

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

        #region Methods

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
            return result;
        }

        /// <summary>
        /// Parse the array of byte to the message
        /// </summary>
        /// <param name="buffer">the array of byte</param>
        /// <returns>Return whether parse successfully</returns>
        public override bool Parse(byte[] buffer)
        {
            Index = Globals.BytesToInt32(buffer, 1);
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

        /// <summary>
        /// Handle the message
        /// </summary>
        /// <param name="peer">Modify the state of peer</param>
        public override void Handle(Peer peer)
        {
            peer.SetBit(Index);
        }

        public override string ToString()
        {
            string result = string.Format("Have {0}", Index);
            return result;
        }

        #endregion
    }
}
