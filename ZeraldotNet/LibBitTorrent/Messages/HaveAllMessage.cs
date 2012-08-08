using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Have all message
    /// </summary>
    public class HaveAllMessage : ChokeMessage
    {
        #region Fields

        private static readonly byte[] Bytes = new byte[5] { 0x00, 0x00, 0x00, 0x01, 0x0E };
        private const string MessageString = "Have all";

        #endregion

        #region Properties

        /// <summary>
        /// The instance of have all message
        /// </summary>
        public new static HaveAllMessage Instance { get; private set; }

        /// <summary>
        /// The type of message
        /// </summary>
        public override MessageType Type
        {
            get { return MessageType.HaveAll; }
        }

        #endregion

        #region Constructors

        static HaveAllMessage()
        {
            Instance = new HaveAllMessage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the array of byte that corresponds the message
        /// </summary>
        /// <returns>Return the array of byte</returns>
        public override byte[] GetByteArray()
        {
            return Bytes;
        }

        /// <summary>
        /// Handle the message
        /// </summary>
        /// <param name="peer">Modify the state of peer</param>
        public override void Handle(Peer peer)
        {
            peer.SetBitfield();
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

        public override string ToString()
        {
            return MessageString;
        }

        #endregion
    }
}
