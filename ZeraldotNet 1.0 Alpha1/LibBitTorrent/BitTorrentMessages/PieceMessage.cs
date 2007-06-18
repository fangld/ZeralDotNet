using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    public class PieceMessage : HaveMessage
    {
        private int begin;

        public int Begin;

        private byte[] pieces;

        public override byte[] Encode()
        {
            byte[] result = new byte[BytesLength];

            //信息ID为7
            result[0] = (byte)BitTorrentMessageType.Piece;

            //写入片断索引号
            Int32ToBytes(Index, result, 1);

            //写入子片断的起始位置
            Int32ToBytes(begin, result, 5);

            //写入子片断的数据
            pieces.CopyTo(result, 9);

            return result;
        }

        public override bool Decode(byte[] buffer)
        {
            if (buffer.Length <= 9)
            {
                return false;
            }

            Index = BytesToInt32(buffer, 1);

            begin = BytesToInt32(buffer, 5);

            pieces = new byte[buffer.Length - 9];
            Globals.CopyBytes(buffer, 9, pieces);

            return true;
        }

        public override int BytesLength
        {
            get { return 9 + pieces.Length; }
        }
    }
}
