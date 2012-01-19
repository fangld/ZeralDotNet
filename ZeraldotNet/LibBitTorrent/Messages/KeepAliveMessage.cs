﻿using System;
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

        #endregion

        #region Properties

        public static KeepAliveMessage Instance { get; private set; }

        #endregion

        #region Constructors

        static KeepAliveMessage()
        {
            Instance = new KeepAliveMessage();
        }

        #endregion

        #region Methods

        public override byte[] Encode()
        {
            return Byte;
        }

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

        public override bool Parse(MemoryStream ms)
        {
            throw new NotImplementedException();
        }

        //public override bool Handle(byte[] buffer, int offset)
        //{
        //    throw new NotImplementedException();
        //}

        public override int BytesLength
        {
            get { return 0; }
        }

        public override MessageType Type
        {
            get { return MessageType.KeepAlive; }
        }

        public override string ToString()
        {
            const string result = "Keep alive message";
            return result;
        }

        #endregion
    }
}
