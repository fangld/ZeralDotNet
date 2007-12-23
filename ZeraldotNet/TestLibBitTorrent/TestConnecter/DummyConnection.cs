using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.TestLibBitTorrent.DummyMessages;

namespace ZeraldotNet.TestLibBitTorrent.TestConnecter
{
    /// <summary>
    /// 测试的连接类
    /// </summary>
    public class DummyConnection
    {
        #region Private Field

        /// <summary>
        /// 封装连接类
        /// </summary>
        private DummyEncryptedConnection encryptedConnection;

        /// <summary>
        /// 连接管理类
        /// </summary>
        private DummyConnecter connecter;

        /// <summary>
        /// 是否获得某些片断
        /// </summary>
        private bool getAnything;

        /// <summary>
        /// 下载器
        /// </summary>
        private DummyDownload download;

        /// <summary>
        /// 上传器
        /// </summary>
        private DummyUpload upload;

        /// <summary>
        /// 网络信息类
        /// </summary>
        private DummyMessage message;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问和设置下载器
        /// </summary>
        public DummyDownload Download
        {
            get { return this.download; }
            set { this.download = value; }
        }

        /// <summary>
        /// 访问和设置上传器
        /// </summary>
        public DummyUpload Upload
        {
            get { return this.upload; }
            set { this.upload = value; }
        }

        /// <summary>
        /// 访问是否获得某些片断
        /// </summary>
        public bool GetAnything
        {
            get { return this.getAnything; }
        }

        /// <summary>
        /// 访问节点的IP地址
        /// </summary>
        public string IP
        {
            get { return ""; }//connection.get_ip();}
        }

        /// <summary>
        /// 访问节点的ID号
        /// </summary>
        public byte[] ID
        {
            get { return null; }//connection.get_id();
        }

        /// <summary>
        /// 访问是否被缓冲
        /// </summary>
        public bool IsFlushed
        {
            //if (connecter.rate_capped)
            get { return false; }
            //return connection.is_flushed();
        }

        /// <summary>
        /// 访问是否已经本地初始化
        /// </summary>
        public bool IsLocallyInitiated
        {
            get { return false; }//connection.is_locally_initiated();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connection">封装连接类</param>
        /// <param name="connecter">连接管理类</param>
        public DummyConnection(DummyEncryptedConnection connection, DummyConnecter connecter)
        {
            this.encryptedConnection = connection;
            this.connecter = connecter;
            this.getAnything = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void close()
        {
            //connection.close();
        }

        #region 发送网络信息

        /// <summary>
        /// 发送choke信息
        /// choke: <len=0001><id=0>
        /// </summary>
        public void SendChoke()
        {
            //发送choke信息
            message = DummyMessageFactory.GetChokeMessage();
            encryptedConnection.SendMessage(message.Encode());
        }

        /// <summary>
        /// 发送unchoke信息
        /// unchoke: <len=0001><id=1>
        /// </summary>
        public void SendUnchoke()
        {
            //发送unchoke信息
            message = DummyMessageFactory.GetUnchokeMessage();
            encryptedConnection.SendMessage(message.Encode());
        }

        /// <summary>
        /// 发送interested信息
        /// interested: <len=0001><id=2>
        /// </summary>
        public void SendInterested()
        {
            //发送interested信息
            message = DummyMessageFactory.GetInterestedMessage();
            encryptedConnection.SendMessage(message.Encode());
        }

        /// <summary>
        /// 发送not interested信息
        /// not interested: <len=0001><id=3>
        /// </summary>
        public void SendNotInterested()
        {
            //发送not interested信息
            message = DummyMessageFactory.GetNotInterestedMessage();
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
            message = DummyMessageFactory.GetHaveMessage(index);
            encryptedConnection.SendMessage(message.Encode());
        }

        /// <summary>
        /// 发送bitfield信息
        /// bitfield: <len=0001+X><id=5><bitfield> 
        /// </summary>
        /// <param name="bitField">已经下载的文件片断</param>
        public void SendBitfield(bool[] bitfield)
        {
            //发送bitfield信息
            message = DummyMessageFactory.GetBitfieldMessage(bitfield);
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
            message = DummyMessageFactory.GetRequestMessage(index, begin, length);
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
            //更新上传速率
            connecter.UpdateUploadRate(pieces.Length);

            //发送piece信息
            message = DummyMessageFactory.GetPieceMessage(index, begin, pieces);
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
            message = DummyMessageFactory.GetCancelMessage(index, begin, length);
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
            message = DummyMessageFactory.GetPortMessage(port);
            encryptedConnection.SendMessage(message.Encode());
        }

        #endregion

        #endregion
    }
}