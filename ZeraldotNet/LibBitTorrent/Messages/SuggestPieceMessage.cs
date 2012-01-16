﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    public class SuggestPieceMessage : HaveMessage
    {
        #region Constructors

        public SuggestPieceMessage()
        {}

        public SuggestPieceMessage(int index):base(index)
        {}

        #endregion

        #region Methods

        public override byte[] Encode()
        {
            throw new NotImplementedException();
        }
        
        public override bool Decode(byte[] buffer, int offset, int count)
        {
            //if buffer is all zero, it is true, else it is false
            bool isByte1Right = (buffer[offset] == 0x00);
            bool isByte2Right = (buffer[offset + 1] == 0x00);
            bool isByte3Right = (buffer[offset + 2] == 0x00);
            bool isByte4Right = (buffer[offset + 3] == 0x01);
            bool isByte5Right = (buffer[offset + 4] == 0x00);
            return (isByte1Right & isByte2Right & isByte3Right & isByte4Right & isByte5Right);
        }

        public override bool Decode(System.IO.MemoryStream ms)
        {
            throw new NotImplementedException();
        }

        #endregion

        //public override bool Handle(byte[] buffer, int offset)
        //{
        //    throw new NotImplementedException();
        //}

        public override int BytesLength
        {
            get { return 5; }
        }

        public override MessageType Type
        {
            get { return MessageType.SuggestPiece; }
        }
    }
}
