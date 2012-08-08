using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    public class KeepAliveMessage : Message
    {
        #region Fields

        private static readonly byte[] Byte = new byte[] {0x00, 0x00, 0x00, 0x00};
        private const string MessageString = "Keep alive";

        #endregion

        #region Properties

        /// <summary>
        /// The instance of not keep alive message
        /// </summary>
        public static KeepAliveMessage Instance { get; private set; }

        /// <summary>
        /// The length of message
        /// </summary>
        public override int BytesLength
        {
            get { return 4; }
        }

        /// <summary>
        /// The type of message
        /// </summary>
        public override MessageType Type
        {
            get { return MessageType.KeepAlive; }
        }

        #endregion

        #region Constructors

        static KeepAliveMessage()
        {
            Instance = new KeepAliveMessage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the array of byte that corresponds the message
        /// </summary>
        /// <returns>Return the array of byte</returns>
        public override byte[] GetByteArray()
        {
            return Byte;
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
            bool isByte1Zero = (buffer[offset] == 0x00);
            bool isByte2Zero = (buffer[offset + 1] == 0x00);
            bool isByte3Zero = (buffer[offset + 2] == 0x00);
            bool isByte4Zero = (buffer[offset + 3] == 0x00);
            return (isByte1Zero & isByte2Zero & isByte3Zero & isByte4Zero);
        }

        public override string ToString()
        {
            return MessageString;
        }

        #endregion
    }
}
