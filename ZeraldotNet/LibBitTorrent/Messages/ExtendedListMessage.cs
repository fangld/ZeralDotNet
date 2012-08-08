using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Extended list message
    /// </summary>
    public class ExtendedListMessage : Message
    {
        #region Properties

        /// <summary>
        /// The length of message
        /// </summary>
        public override int BytesLength
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// The type of message
        /// </summary>
        public override MessageType Type
        {
            get { return MessageType.ExtendedList; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the array of byte that corresponds the message
        /// </summary>
        /// <returns>Return the array of byte</returns>
        public override byte[] GetByteArray()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Parse the array of byte to the message
        /// </summary>
        /// <param name="buffer">the array of byte</param>
        /// <returns>Return whether parse successfully</returns>
        public override bool Parse(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public override bool Parse(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
