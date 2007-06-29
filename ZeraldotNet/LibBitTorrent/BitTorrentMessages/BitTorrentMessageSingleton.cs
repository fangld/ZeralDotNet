using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BitTorrentMessages
{
    public class BitTorrentMessageSingleton
    {
        private static ChokeMessage chokeMessage;
        private static UnchokeMessage unchokeMessage;
        private static InterestedMessage interestedMessage;
        private static NotInterestedMessage notInterestedMessage;
        private static HaveMessage haveMessage;
        private static BitFieldMessage bitFieldMessage;
        private static RequestMessage requestMessage;
        private static PieceMessage pieceMessage;
        private static CancelMessage cancelMessage;
        private static PortMessage portMessage;

        static BitTorrentMessageSingleton()
        {
            chokeMessage = new ChokeMessage();
            unchokeMessage = new UnchokeMessage();
            interestedMessage = new InterestedMessage();
            notInterestedMessage = new NotInterestedMessage();
            haveMessage = new HaveMessage();
            bitFieldMessage = new BitFieldMessage();
            requestMessage = new RequestMessage();
            pieceMessage = new PieceMessage();
            cancelMessage = new CancelMessage();
            portMessage = new PortMessage();
        }

        public static ChokeMessage CreateChokeMessage()
        {
            return chokeMessage;
        }

        public static UnchokeMessage CreateUnchokeMessage()
        {
            return unchokeMessage;
        }

        public static InterestedMessage CreateInterestedMessage()
        {
            return interestedMessage;
        }

        public static NotInterestedMessage CreateNotInterestedMessage()
        {
            return notInterestedMessage;
        }

        public static BitFieldMessage CreateBitFieldMessage(bool[] bitField)
        {
            bitFieldMessage.Booleans = bitField;
            return bitFieldMessage;
        }

        public static RequestMessage CreateRequestMessage(int index, int begin, int length)
        {
            requestMessage.Index = index;
            requestMessage.Begin = begin;
            requestMessage.Length = length;
            return requestMessage;
        }

        public static HaveMessage CreateHaveMessage(int index)
        {
            haveMessage.Index = index;
            return haveMessage;
        }

        public static PieceMessage CreatePieceMessage(int index, int begin, byte[] pieces)
        {
            pieceMessage.Index = index;
            pieceMessage.Begin = begin;
            pieceMessage.Pieces = pieces;
            return pieceMessage;
        }

        public static CancelMessage CreateCancelMessage(int index, int begin, int length)
        {
            cancelMessage.Index = index;
            cancelMessage.Begin = begin;
            cancelMessage.Length = length;
            return cancelMessage;
        }

        public static PortMessage CreatePortMessage(ushort port)
        {
            portMessage.Port = port;
            return portMessage;
        }
    }
}
