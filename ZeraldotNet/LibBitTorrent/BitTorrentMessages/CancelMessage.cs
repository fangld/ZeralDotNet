﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    public class CancelMessage : HaveMessage
    {
        private int begin;
        public int Begin
        {
            get { return this.begin; }
            set { this.begin = value; }
        }

        private int length;

        public int Length
        {
            get { return this.length; }
            set { this.length = value; }
        }

        public CancelMessage() { }

        public CancelMessage(int index, int begin, int length)
        {
            this.Index = index;
            this.begin = begin;
            this.length = length;
        }

        public override bool Decode(byte[] buffer)
        {
            if (buffer.Length != BytesLength)
            {
                return false;
            }

            Index = Globals.BytesToInt32(buffer, 1);

            begin = Globals.BytesToInt32(buffer, 5);

            length = Globals.BytesToInt32(buffer, 9);

            return true;
        }

        public override byte[] Encode()
        {
            return Encode(BitTorrentMessageType.Cancel);
        }

        public override void Handle()
        {
            base.Handle();
        }

        public override int BytesLength
        {
            get { return 13; }
        }

        protected byte[] Encode(BitTorrentMessageType type)
        {
            byte[] result = new byte[BytesLength];

            result[0] = (byte)type;

            //写入片断索引号
            Globals.Int32ToBytes(Index, result, 1);

            Globals.Int32ToBytes(begin, result, 5);

            Globals.Int32ToBytes(length, result, 9);

            return result;
        }
    }
}