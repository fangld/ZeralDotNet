﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Not interested message
    /// </summary>
    public class NotInterestedMessage : ChokeMessage
    {
        #region Fields

        private static readonly byte[] Bytes = new byte[] { 0x00, 0x00, 0x00, 0x01, 0x03 };
        private const string MessageString = "NotInterested";
              
        #endregion

        #region Properties

        /// <summary>
        /// The instance of not interested message
        /// </summary>
        public new static NotInterestedMessage Instance { get; private set; }

        /// <summary>
        /// The type of message
        /// </summary>
        public override MessageType Type
        {
            get { return MessageType.NotInterested; }
        }

        #endregion

        #region Constructors

        static NotInterestedMessage()
        {
            Instance = new NotInterestedMessage();
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

        public override bool Parse(byte[] buffer, int offset, int count)
        {
            //if buffer is all zero, it is true, else it is false
            bool isByte1Right = (buffer[offset] == 0x00);
            bool isByte2Right = (buffer[offset + 1] == 0x00);
            bool isByte3Right = (buffer[offset + 2] == 0x00);
            bool isByte4Right = (buffer[offset + 3] == 0x01);
            bool isByte5Right = (buffer[offset + 4] == 0x03);
            return (isByte1Right & isByte2Right & isByte3Right & isByte4Right & isByte5Right);
        }

        /// <summary>
        /// Handle the message
        /// </summary>
        /// <param name="peer">Modify the state of peer</param>
        public override void Handle(Peer peer)
        {
            peer.PeerInterested = false;
        }

        public override string ToString()
        {
            return MessageString;
        }

        #endregion
    }
}
