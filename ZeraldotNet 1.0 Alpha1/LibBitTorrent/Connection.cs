using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZeraldotNet.LibBitTorrent
{
    public class Connection
    {
        private EncryptedConnection encryptedConnection;

        public EncryptedConnection EncryptedConnection
        {
            get { return this.encryptedConnection; }
            set { this.encryptedConnection = value; }
        }

        private Connecter connecter;

        public Connecter Connecter
        {
            set { this.connecter = value; }
        }

        private bool gotAnything;

        public bool GotAnything
        {
            get { return this.gotAnything; }
            set { this.gotAnything = value; }
        }

        private ISingleDownload download;

        public ISingleDownload Download
        {
            get { return this.download; }
            set { this.download = value; }
        }

        private Upload upload;

        public Upload Upload
        {
            get { return this.upload; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="connecter"></param>
        public Connection(EncryptedConnection encryptedConnection, Connecter connecter)
        {
            this.EncryptedConnection = encryptedConnection;
            this.Connecter = connecter;
            this.gotAnything = false;
        }

        public string IP
        {
            get { throw new NotImplementedException(); }
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public bool IsFlushed()
        {
            throw new NotImplementedException();
        }

        #region 发送网络信息

        /// <summary>
        /// 发送choke信息
        /// choke: <len=0001><id=0>
        /// </summary>
        public void SendChoke()
        {
            //发送choke信息
            encryptedConnection.SendMessage((byte)BitTorrentMessage.Choke);
        }

        /// <summary>
        /// 发送unchoke信息
        /// unchoke: <len=0001><id=1>
        /// </summary>
        public void SendUnchoke()
        {
            //发送unchoke信息
            encryptedConnection.SendMessage((byte)BitTorrentMessage.Unchoke);
        }

        /// <summary>
        /// 发送interested信息
        /// interested: <len=0001><id=2>
        /// </summary>
        public void SendInterested()
        {
            //发送interested信息
            encryptedConnection.SendMessage((byte)BitTorrentMessage.Interested);
        }

        /// <summary>
        /// 发送not interested信息
        /// not interested: <len=0001><id=3>
        /// </summary>
        public void SendNotInterested()
        {
            //发送not interested信息
            encryptedConnection.SendMessage((byte)BitTorrentMessage.NotInterested);
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
            message[0] = (byte)BitTorrentMessage.Have;

            //写入片断索引号
            Int32ToBytes(index, message, 1);

            //发送have信息
            encryptedConnection.SendMessage(message);
        }

        /// <summary>
        /// 发送bitfield信息
        /// bitfield: <len=0001+X><id=5><bitfield> 
        /// </summary>
        /// <param name="bitField">已经下载的文件片断</param>
        public void SendBitField(bool[] bitField)
        {
            byte[] bitFieldBytes = BitField.ToBitField(bitField);

            byte[] message = new byte[bitFieldBytes.Length + 1];

            //信息ID为5
            message[0] = (byte)(BitTorrentMessage.BitField);

            //写入BitField
            bitFieldBytes.CopyTo(message, 1);

            //发送bitfield信息
            encryptedConnection.SendMessage(message);
        }

        /// <summary>
        /// 发送request信息
        /// request: <len=0013><id=6><index><begin><lengthBytes> 
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断的起始位置</param>
        /// <param name="lengthBytes">子片断的长度</param>
        public void SendRequest(int index, int begin, int length)
        {
            byte[] message = new byte[13];
            
            //信息ID为6
            message[0] = (byte)BitTorrentMessage.Request;

            //写入片断索引号
            Int32ToBytes(index, message, 1);

            //写入子片断的起始位置
            Int32ToBytes(begin, message, 5);

            //写入子片断的数据
            Int32ToBytes(length, message, 9);

            //发送request信息
            encryptedConnection.SendMessage(message);
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

            byte[] message = new byte[9 + pieceLength];

            //信息ID为7
            message[0] = (byte)BitTorrentMessage.Piece;

            //写入片断索引号
            Int32ToBytes(index, message, 1);

            //写入子片断的起始位置
            Int32ToBytes(begin, message, 5);

            //写入子片断的数据
            piece.CopyTo(message, 9);

            //发送piece信息
            encryptedConnection.SendMessage(message);
        }

        /// <summary>
        /// 发送cancel信息
        /// cancel: <len=0013><id=8><index><begin><lengthBytes> 
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断的起始位置</param>
        /// <param name="lengthBytes">子片断的长度</param>
        public void SendCancel(int index, int begin, int length)
        {
            byte[] message = new byte[13];

            //信息ID为8
            message[0] = (byte)BitTorrentMessage.Cancel;

            //写入片断索引号
            Int32ToBytes(index, message, 1);

            //写入子片断的起始位置
            Int32ToBytes(begin, message, 5);
             
            //写入子片断的长度
            Int32ToBytes(length, message, 9);

            //发送cancel信息
            encryptedConnection.SendMessage(message);
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
            message[0] = (byte)BitTorrentMessage.Port;

            //写入DHT监听端口
            UInt16ToBytes(port, message, 1);

            //发送port信息
            encryptedConnection.SendMessage(message);
        }

        /// <summary>
        /// 将16位无符号整数写入字节流
        /// </summary>
        /// <param name="value">需要写入的16位无符号整数</param>
        /// <param name="buffer">待写入的字节流</param>
        /// <param name="offset">写入字节流的位置</param>
        private void UInt16ToBytes(ushort value, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)(value >> 8);
            buffer[++offset] = (byte)(value & 0xFF);
        }

        /// <summary>
        /// 将32位有符号整数写入字节流
        /// </summary>
        /// <param name="value">需要写入的32位有符号整数</param>
        /// <param name="buffer">待写入的字节流</param>
        /// <param name="offset">写入字节流的位置</param>
        private void Int32ToBytes(int value, byte[] buffer, int offset)
        {
            buffer[offset] = (byte)(value >> 24);
            buffer[++offset] = (byte)((value >> 16) & 0xFF);
            buffer[++offset] = (byte)((value >> 8) & 0xFFFF);
            buffer[++offset] = (byte)(value & 0xFFFFFF);
        }

        #endregion


    }
}
