using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    public static class MessageDecoder
    {
        public static Message Decode(BufferPool bufferPool)
        {
            Message result = null;

            if (bufferPool.Length < 4)
            {
                return result;
            }

            byte firstByte = bufferPool.GetFirstByte();
            if (firstByte == 68)
            {
                if (bufferPool.Length > firstByte)
                {
                    byte[] buffer = new byte[68];
                    result = new HandshakeMessage();
                    bufferPool.Read(buffer, 0, 68);
                    result.Decode(buffer);
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
                if (contentBytes[0] == (byte)MessageType.BitField)
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
                switch((MessageType)contentBytes[0])
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
                result.Decode(contentBytes);
            }
            return result;

            //byte[] 

            //BitConverter.ToInt32(lengthBytes, 0);

            //if (count == 4)
            //        {
            //            result = new KeepAliveMessage();
            //            return result;
            //        }

            //        else if (count == 68)
            //        {
            //            if (bytes[offset + 4] == (byte)MessageType.BitField)
            //            {
            //                result =  new BitfieldMessage();
            //                result.Parse(bytes, offset, count);
            //                return result;
            //            }

            //            if (bytes[offset + 4] == 'B')
            //            {
            //                result = new HandshakeMessage();
            //                result.Parse(bytes, offset, count);
            //                return result;
            //            }

            ////取得字节流首位
            //MessageType firstByte = (MessageType) bytes[offset];

            ////根据字节流首位，决定处理哪种网络信息
            //switch (firstByte)
            //{
            //    case MessageType.BitField:
            //        result = new BitfieldMessage();
            //        break;
            //    case MessageType.Choke:
            //        result = new ChokeMessage();
            //        break;
            //    case MessageType.Unchoke:
            //        result = new UnchokeMessage();
            //        break;
            //    case MessageType.Interested:
            //        result = new InterestedMessage();
            //        break;
            //    case MessageType.NotInterested: 
            //        result = new NotInterestedMessage();
            //        break;
            //    //case MessageType.Have:
            //    //    result = new HaveMessage();
            //    //    break;
            //    default:
            //        return null;
            //        //case MessageType.Have: message = new HaveMessage(encryptedConnection, connection, connecter); break;
            //        //case MessageType.Request: message = new RequestMessage(encryptedConnection, connection, connecter); break;
            //        //case MessageType.Cancel: message = new CancelMessage(encryptedConnection, connection, connecter); break;
            //        //case MessageType.Piece: message = new PieceMessage(encryptedConnection, connection, connecter); break;
            //        //case MessageType.Port: message = new PortMessage(encryptedConnection); break;
            //        //default: encryptedConnection.Close(); return false;
            //}

            ////处理网络信息
            //return result.Parse(bytes, offset, count) ? result : null;
        }

        //public static bool HandshakeMessage(byte firstByte)
        //{
        //    if (firstByte == 68)
        //    {

        //    }
        //}
    }
}
