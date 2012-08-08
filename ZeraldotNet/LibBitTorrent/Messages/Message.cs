using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// The type of messages that can be sent to or received from peers
    /// </summary>
    public enum MessageType : byte
    {
        Choke = 0,
        Unchoke = 1,
        Interested = 2,
        NotInterested = 3,
        Have = 4,
        BitField = 5,
        Request = 6,
        Piece = 7,
        Cancel = 8,
        Port = 9,
        SuggestPiece = 0x0D,
        HaveAll = 0x0E,
        HaveNone = 0x0F,
        RejectRequest = 0x10,
        AllowedFast = 0x11,
        ExtendedList = 20,
        KeepAlive,
        Handshake
    }

    /// <summary>
    /// Message that catch network information
    /// </summary>
    public abstract class Message
    {
        #region Properties

        /// <summary>
        /// The length of message
        /// </summary>
        public abstract int BytesLength { get; }

        /// <summary>
        /// The type of message
        /// </summary>
        public abstract MessageType Type { get; }

        #endregion

        #region Constructors

        #endregion

        #region Base Methods

        /// <summary>
        /// Get the array of byte that corresponds the message
        /// </summary>
        /// <returns>Return the array of byte</returns>
        public abstract byte[] GetByteArray();

        /// <summary>
        /// Parse the array of byte to the message
        /// </summary>
        /// <param name="buffer">the array of byte</param>
        /// <returns>Return whether parse successfully</returns>
        public abstract bool Parse(byte[] buffer);

        public abstract bool Parse(byte[] buffer, int offset, int count);

        /// <summary>
        /// Handle the message
        /// </summary>
        /// <param name="peer">Modify the state of peer</param>
        public virtual void Handle(Peer peer)
        {
            
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Parse the message from the buffer pool
        /// </summary>
        /// <param name="bufferPool">The buffer pool that saves the bytes from network</param>
        /// <param name="booleansLength">The length of booleans</param>
        /// <returns>Return the message that is parsed from the buffer pool</returns>
        public static Message Parse(BufferPool bufferPool, int booleansLength)
        {
            Message result = null;

            if (bufferPool.Length < 4)
            {
                return null;
            }

            byte firstByte = bufferPool.GetFirstByte();
            if (firstByte == 19)
            {
                if (bufferPool.Length >= 68)
                {
                    byte[] buffer = new byte[68];
                    result = new HandshakeMessage();
                    bufferPool.Read(buffer, 0, 68);
                    result.Parse(buffer);
                    return result;
                }
                return null;
            }

            byte[] lengthBytes = new byte[4];

            bufferPool.Read(lengthBytes, 0, 4);

            int length = GetLength(lengthBytes, 0);
            Debug.Assert(length >= 0);

            if (length == 0)
            {
                result = new KeepAliveMessage();
                return result;
            }

            if (bufferPool.Length < length)
            {
                bufferPool.Seek(-4);
                return null;
            }

            byte[] contentBytes = new byte[length];
            bufferPool.Read(contentBytes, 0, length);

            switch ((MessageType)contentBytes[0])
            {
                case MessageType.Choke:
                    result = new ChokeMessage();
                    break;
                case MessageType.Unchoke:
                    result = new UnchokeMessage();
                    break;
                case MessageType.Interested:
                    result = new InterestedMessage();
                    break;
                case MessageType.NotInterested:
                    result = new NotInterestedMessage();
                    break;
                case MessageType.Have:
                    result = new HaveMessage();
                    break;
                case MessageType.BitField:
                    result = new BitfieldMessage(booleansLength);
                    break;
                case MessageType.Request:
                    result = new RequestMessage();
                    break;
                case MessageType.Piece:
                    result = new PieceMessage();
                    break;
                case MessageType.Cancel:
                    result = new CancelMessage();
                    break;
                case MessageType.Port:
                    result = new PortMessage();
                    break;
                case MessageType.SuggestPiece:
                    result = new SuggestPieceMessage();
                    break;
                case MessageType.HaveAll:
                    result = new HaveAllMessage();
                    break;
                case MessageType.HaveNone:
                    result = new HaveNoneMessage();
                    break;
                case MessageType.RejectRequest:
                    result = new RejectRequestMessage();
                    break;
                case MessageType.AllowedFast:
                    result = new AllowedFastMessage();
                    break;
                case MessageType.ExtendedList:
                    result = new ExtendedListMessage();
                    break;
            }

            if (result != null)
            {
                result.Parse(contentBytes);
            }
            return result;
        }

        public static void SetBytesLength(byte[] bytes, int length)
        {
            bytes[0] = (byte)(length >> 24);
            bytes[1] = (byte)(length >> 16);
            bytes[2] = (byte)(length >> 8);
            bytes[3] = (byte)(length);
        }

        public static int GetLength(byte[] bytes, int offset)
        {
            int result = 0;
            result |= (bytes[offset] << 24);
            result |= (bytes[offset + 1] << 16);
            result |= (bytes[offset + 2] << 8);
            result |= (bytes[offset + 3]);
            return result;
        }

        #endregion
    }
}
