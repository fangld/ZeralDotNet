using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ZeraldotNet.LibBitTorrent.BitTorrentMessages;

namespace ZeraldotNet.LibBitTorrent
{
    public class Connection
    {
        #region Private Field
        private EncryptedConnection encryptedConnection;
        private Connecter connecter;
        private bool gotAnything;
        private ISingleDownload download;
        private Upload upload;
        private BitTorrentMessage message;
        #endregion

        #region Public Properties
        public EncryptedConnection EncryptedConnection
        {
            get { return this.encryptedConnection; }
            set { this.encryptedConnection = value; }
        }

        public Connecter Connecter
        {
            set { this.connecter = value; }
        }

        public bool GotAnything
        {
            get { return this.gotAnything; }
            set { this.gotAnything = value; }
        }

        public ISingleDownload Download
        {
            get { return this.download; }
            set { this.download = value; }
        }

        public Upload Upload
        {
            get { return this.upload; }
            set { this.upload = value; }
        }

        public string IP
        {
            get { return this.encryptedConnection.IP; }
        }

        public byte[] ID
        {
            get { return this.encryptedConnection.ID; }
        }

        public bool IsFlushed
        {
            get
            {
                if (connecter.RateCapped)
                {
                    return false;
                }
                return encryptedConnection.IsFlushed;
            }
        }

        public bool IsLocallyInitiated
        {
            get { return this.encryptedConnection.IsLocallyInitiated; }
        }
        #endregion

        #region Constructors
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
        #endregion

        #region Methods
        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            encryptedConnection.Close();
        }

        #region 发送网络信息

        /// <summary>
        /// 发送choke信息
        /// choke: <len=0001><id=0>
        /// </summary>
        public void SendChoke()
        {
            //发送choke信息
            message = BitTorrentMessageSingleton.CreateChokeMessage();
            encryptedConnection.SendMessage(message.Encode());
        }

        /// <summary>
        /// 发送unchoke信息
        /// unchoke: <len=0001><id=1>
        /// </summary>
        public void SendUnchoke()
        {
            //发送unchoke信息
            message = BitTorrentMessageSingleton.CreateUnchokeMessage();
            encryptedConnection.SendMessage(message.Encode());
        }

        /// <summary>
        /// 发送interested信息
        /// interested: <len=0001><id=2>
        /// </summary>
        public void SendInterested()
        {
            //发送interested信息
            message = BitTorrentMessageSingleton.CreateInterestedMessage();
            encryptedConnection.SendMessage(message.Encode());
        }

        /// <summary>
        /// 发送not interested信息
        /// not interested: <len=0001><id=3>
        /// </summary>
        public void SendNotInterested()
        {
            //发送not interested信息
            message = BitTorrentMessageSingleton.CreateNotInterestedMessage();
            encryptedConnection.SendMessage(message.Encode());
        }

        /// <summary>
        /// 发送have信息
        /// have: <len=0005><id=4><pieces index> 
        /// </summary>
        /// <param name="index">片断索引号</param>
        public void SendHave(int index)
        {
            //发送have信息
            message = BitTorrentMessageSingleton.CreateHaveMessage(index);
            encryptedConnection.SendMessage(message.Encode());
        }

        /// <summary>
        /// 发送bitfield信息
        /// bitfield: <len=0001+X><id=5><bitfield> 
        /// </summary>
        /// <param name="bitField">已经下载的文件片断</param>
        public void SendBitField(bool[] bitField)
        {
            message = BitTorrentMessageSingleton.CreateBitFieldMessage(bitField);
            encryptedConnection.SendMessage(message.Encode());
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
            //发送request信息
            message = BitTorrentMessageSingleton.CreateRequestMessage(index, begin, length);
            encryptedConnection.SendMessage(message.Encode());
        }

        /// <summary>
        /// 发送piece信息
        /// pieces: <len=0009+X><id=7><index><begin><block> 
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断的起始位置</param>
        /// <param name="pieces">子片断的数据</param>
        public void SendPiece(int index, int begin, byte[] pieces)
        {
            connecter.UpdateUploadRate(pieces.Length);

            //发送piece信息
            message = BitTorrentMessageSingleton.CreatePieceMessage(index, begin, pieces);
            encryptedConnection.SendMessage(message.Encode());
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
            //发送cancel信息
            message = BitTorrentMessageSingleton.CreateCancelMessage(index, begin, length);
            encryptedConnection.SendMessage(message.Encode());
        }

        /// <summary>
        /// 发送port信息
        /// port: <len=0003><id=9><listen-port> 
        /// </summary>
        /// <param name="port">DHT监听端口</param>
        public void SendPort(ushort port)
        {
            //发送port信息
            message = BitTorrentMessageSingleton.CreatePortMessage(port);
            encryptedConnection.SendMessage(message.Encode());
        }
        #endregion
        #endregion
    }
}
