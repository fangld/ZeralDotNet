using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Choke message
    /// </summary>
    public class ChokeMessage : Message
    {
        #region Fields

        private static readonly byte[] Bytes = new byte[5] { 0x00, 0x00, 0x00, 0x01, 0x00 };
        private const string MessageString = "Choke";

        #endregion

        #region Properties

        /// <summary>
        /// The instance of choke message
        /// </summary>
        public static ChokeMessage Instance { get; private set; }

        /// <summary>
        /// The length of message
        /// </summary>
        public override int BytesLength
        {
            get { return 5; }
        }

        /// <summary>
        /// The type of message
        /// </summary>
        public override MessageType Type
        {
            get { return MessageType.Choke; }
        }

        #endregion

        #region Constructors

        static ChokeMessage()
        {
            Instance = new ChokeMessage();
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
        /// Parse the array of byte to the message
        /// </summary>
        /// <param name="buffer">the array of byte</param>
        /// <returns>Return whether parse successfully</returns>
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

        /// <summary>
        /// Handle the message
        /// </summary>
        /// <param name="peer">Modify the state of peer</param>
        public override void Handle(Peer peer)
        {
            peer.PeerChoking = true;
        }

        public override string ToString()
        {
            return MessageString;
        }

        #endregion
    }
}
