﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    public class NotInterestedMessage : ChokeMessage
    {
        #region Fields

        private static readonly byte[] Bytes = new byte[] { 0x00, 0x00, 0x00, 0x01, 0x03 };
        private const string MessageString = "NotInterested";
              
        #endregion

        #region Properties

        public static NotInterestedMessage Instance { get; private set; }

        #endregion

        #region Constructors

        static NotInterestedMessage()
        {
            Instance = new NotInterestedMessage();
        }

        #endregion
        
        public override byte[] Encode()
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

        public override MessageType Type
        {
            get { return MessageType.NotInterested; }
        }

        public override string ToString()
        {
            return MessageString;
        }
    }
}
