using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZeraldotNet.LibBitTorrent
{
    public class Connection
    {
        private EncryptedConnection connection;

        private Upload upload;

        public Upload Upload
        {
            get { return this.upload; }
        }

        private Connecter connecter;

        private bool gotAnything;

        public bool GotAnything
        {
            get { return this.gotAnything; }
            set { this.gotAnything = value; }
        }

        /// <summary>
        /// 发送choke信息
        /// choke: <len=0001><id=0>
        /// </summary>
        public void SendChoke()
        {
            connection.SendMessage((byte)Message.Choke);
        }

        /// <summary>
        /// 发送unchoke信息
        /// unchoke: <len=0001><id=1>
        /// </summary>
        public void SendUnchoke()
        {
            connection.SendMessage((byte)Message.Unchoke);
        }

        /// <summary>
        /// 发送interested信息
        /// interested: <len=0001><id=2>
        /// </summary>
        public void SendInterested()
        {
            connection.SendMessage((byte)Message.Interested);
        }

        /// <summary>
        /// 发送not interested信息
        /// not interested: <len=0001><id=3>
        /// </summary>
        public void SendNotInterested()
        {
            connection.SendMessage((byte)Message.NotInterested);
        }

        /// <summary>
        /// 发送have信息
        /// have: <len=0005><id=4><piece index> 
        /// </summary>
        /// <param name="index">片断索引号</param>
        public void SendHave(int index)
        {
            byte[] message = new byte[5];

            //信息ID为8
            message[0] = (byte)Message.Have;

            Int32ToBytes(index, message, 1);

            connection.SendMessage(message);
        }

        /// <summary>
        /// 发送bitfield信息
        /// bitfield: <len=0001+X><id=5><bitfield> 
        /// </summary>
        /// <param name="bitField">已经下载的文件片断</param>
        public void SendBitField(bool[] bitField)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)(Message.BitField));
            byte[] message = BitField.ToBitField(bitField);
            ms.Write(message, 0, message.Length);
            connection.SendMessage(ms.ToArray());
        }

        /// <summary>
        /// 发送request信息
        /// request: <len=0013><id=6><index><begin><length> 
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断的起始位置</param>
        /// <param name="length">子片断的长度</param>
        public void SendRequest(int index, int begin, int length)
        {
            byte[] message = new byte[13];
            
            //信息ID为6
            message[0] = (byte)Message.Request;

            Int32ToBytes(index, message, 1);

            Int32ToBytes(begin, message, 5);

            Int32ToBytes(length, message, 9);

            connection.SendMessage(message);
        }

        /// <summary>
        /// 发送piece信息
        /// piece: <len=0009+X><id=7><index><begin><block> 
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断的起始位置</param>
        /// <param name="piece">子片断的数据</param>
        public void SendPiece(int index, int begin, byte[] piece)
        {
            int pieceLength = piece.Length;
            connecter.UpdateUploadRate(pieceLength);

            byte[] message = new byte[9 + piece.Length];

            //信息ID为7
            message[0] = (byte)Message.Piece;

            //写入片断索引号
            Int32ToBytes(index, message, 1);

            //写入子片断的起始位置
            Int32ToBytes(begin, message, 5);

            //写入子片断的数据
            piece.CopyTo(message, 9);

            connection.SendMessage(message);
        }

        /// <summary>
        /// 发送cancel信息
        /// cancel: <len=0013><id=8><index><begin><length> 
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断的起始位置</param>
        /// <param name="length">子片断的长度</param>
        public void SendCancel(int index, int begin, int length)
        {
            byte[] message = new byte[13];

            //信息ID为8
            message[0] = (byte)Message.Cancel;

            //写入片断索引号
            Int32ToBytes(index, message, 1);

            //写入子片断的起始位置
            Int32ToBytes(begin, message, 5);
             
            //写入子片断的长度
            Int32ToBytes(length, message, 9);

            //发送cancel信息
            connection.SendMessage(message);
        }

        /// <summary>
        /// 发送port信息
        /// port: <len=0003><id=9><listen-port> 
        /// </summary>
        /// <param name="port">DHT监听端口</param>
        public void SendPort(ushort port)
        {
            byte[] message = new byte[3];

            //信息ID为9
            message[0] = (byte)Message.Port;

            //写入DHT监听端口
            UInt16ToBytes(port, message, 1);

            //发送port信息
            connection.SendMessage(message);
        }

        private void UInt16ToBytes(ushort value, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)(value >> 8);
            buffer[++offset] = (byte)(value & 0xFF);
        }

        /// <summary>
        /// 将32位有符号整数转换为字节流
        /// </summary>
        /// <param name="value">需要转换的32位有符号整数</param>
        /// <param name="buffer">转换后的字节流</param>
        /// <param name="offset"></param>
        private void Int32ToBytes(int value, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)(value >> 24);
            buffer[++offset] = (byte)((value >> 16) & 0xFF);
            buffer[++offset] = (byte)((value >> 8) & 0xFFFF);
            buffer[++offset] = (byte)(value & 0xFFFFFF);
        }
    }
}
