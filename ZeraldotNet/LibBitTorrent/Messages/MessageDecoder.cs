//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace ZeraldotNet.LibBitTorrent.Messages
//{
//    public static class MessageDecoder
//    {
        

//            //byte[] 

//            //BitConverter.ToInt32(lengthBytes, 0);

//            //if (count == 4)
//            //        {
//            //            result = new KeepAliveMessage();
//            //            return result;
//            //        }

//            //        else if (count == 68)
//            //        {
//            //            if (bytes[offset + 4] == (byte)MessageType.BitField)
//            //            {
//            //                result =  new BitfieldMessage();
//            //                result.Parse(bytes, offset, count);
//            //                return result;
//            //            }

//            //            if (bytes[offset + 4] == 'B')
//            //            {
//            //                result = new HandshakeMessage();
//            //                result.Parse(bytes, offset, count);
//            //                return result;
//            //            }

//            ////取得字节流首位
//            //MessageType firstByte = (MessageType) bytes[offset];

//            ////根据字节流首位，决定处理哪种网络信息
//            //switch (firstByte)
//            //{
//            //    case MessageType.BitField:
//            //        result = new BitfieldMessage();
//            //        break;
//            //    case MessageType.Choke:
//            //        result = new ChokeMessage();
//            //        break;
//            //    case MessageType.Unchoke:
//            //        result = new UnchokeMessage();
//            //        break;
//            //    case MessageType.Interested:
//            //        result = new InterestedMessage();
//            //        break;
//            //    case MessageType.NotInterested: 
//            //        result = new NotInterestedMessage();
//            //        break;
//            //    //case MessageType.Have:
//            //    //    result = new HaveMessage();
//            //    //    break;
//            //    default:
//            //        return null;
//            //        //case MessageType.Have: message = new HaveMessage(encryptedConnection, connection, connecter); break;
//            //        //case MessageType.Request: message = new RequestMessage(encryptedConnection, connection, connecter); break;
//            //        //case MessageType.Cancel: message = new CancelMessage(encryptedConnection, connection, connecter); break;
//            //        //case MessageType.Piece: message = new PieceMessage(encryptedConnection, connection, connecter); break;
//            //        //case MessageType.Port: message = new PortMessage(encryptedConnection); break;
//            //        //default: encryptedConnection.Close(); return false;
//            //}

//            ////处理网络信息
//            //return result.Parse(bytes, offset, count) ? result : null;
//        }

//        //public static bool HandshakeMessage(byte firstByte)
//        //{
//        //    if (firstByte == 68)
//        //    {

//        //    }
//        //}
//    }
//}
