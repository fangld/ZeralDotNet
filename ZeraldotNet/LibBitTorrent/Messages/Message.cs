using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// 网络信息基类
    /// </summary>
    public abstract class Message
    {
        #region Constructors

        #endregion

        #region Base Methods

        /// <summary>
        /// 网络信息的编码函数
        /// </summary>
        /// <returns>返回编码后的字节流</returns>
        public abstract byte[] Encode();

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="buffer">待解码的字节流</param>
        /// <returns>返回是否解码成功</returns>
        public abstract bool Parse(byte[] buffer);

        public abstract bool Parse(byte[] buffer, int offset, int count);

        /// <summary>
        /// 网络信息的解码函数
        /// </summary>
        /// <param name="ms">待解码的内存流</param>
        /// <returns>返回是否解码成功</returns>
        public abstract bool Parse(MemoryStream ms);

        ///// <summary>
        ///// 网络信息的处理函数
        ///// </summary>
        //public abstract bool Handle(byte[] buffer, int offset);

        /// <summary>
        /// 网络信息的处理函数
        /// </summary>
        public abstract int BytesLength { get; }

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
            result |= bytes[offset];
            result |= (bytes[offset + 1] << 8);
            result |= (bytes[offset + 2] << 16);
            result |= (bytes[offset + 3] << 24);
            return result;
        }

        public abstract MessageType Type { get; }

        #endregion

        #region Methods

        public static Message Parse(BufferPool bufferPool)
        {
            Message result = null;

            if (bufferPool.Length < 4)
            {
                return result;
            }

            byte firstByte = bufferPool.GetFirstByte();
            if (firstByte == 19)
            {
                if (bufferPool.Length > firstByte)
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

            int length = Message.GetLength(lengthBytes, 0);
            bufferPool.Seek(-4);
            if (bufferPool.Length < length)
            {
                return null;
            }

            byte[] contentBytes = new byte[length + 4];
            bufferPool.Read(contentBytes, 0, length + 4);

            if (length == 0)
            {
                result = new KeepAliveMessage();
            }
            else if (length == 68)
            {
                if (contentBytes[0] == (byte) MessageType.BitField)
                {
                    result = new BitfieldMessage();
                }

                else if (contentBytes[0] == 'B')
                {
                    result = new HandshakeMessage();
                }
            }
            else
            {
                switch ((MessageType) contentBytes[0])
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
                        result = new BitfieldMessage();
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
            }

            if (result != null)
            {
                result.Parse(contentBytes);
            }
            return result;
        }

        #endregion
    }
}
