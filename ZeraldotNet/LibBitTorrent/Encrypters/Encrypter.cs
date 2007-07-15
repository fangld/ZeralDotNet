using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using ZeraldotNet.LibBitTorrent.RawServers;
using ZeraldotNet.LibBitTorrent.Connecters;

namespace ZeraldotNet.LibBitTorrent.Encrypters
{
    /// <summary>
    /// 封装连接管理类
    /// </summary>
    public class Encrypter : IEncrypter
    {
        #region Private Field

        /// <summary>
        /// 连接管理类
        /// </summary>
        private IConnecter connecter;

        /// <summary>
        /// 服务器类
        /// </summary>
        private IRawServer rawServer;

        /// <summary>
        /// 一个字典，保存单套接字和封装连接类的字典
        /// </summary>
        private Dictionary<ISingleSocket, IEncryptedConnection> connections;

        /// <summary>
        /// 本地ID号
        /// </summary>
        private byte[] myID;

        /// <summary>
        /// 最大请求子片断长度
        /// </summary>
        private int maxLength;

        /// <summary>
        /// 发送keep alive信息的时间间隔
        /// </summary>
        private double keepAliveDelay;

        /// <summary>
        /// 对方ID号
        /// </summary>
        private byte[] downloadID;

        /// <summary>
        /// 最大连接数
        /// </summary>
        private int maxInitiate;

        /// <summary>
        /// 计划函数
        /// </summary>
        private SchedulerDelegate scheduleFunction;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问和设置连接管理类
        /// </summary>
        public IConnecter Connecter
        {
            get { return this.connecter; }
            set { this.connecter = value; }
        }

        /// <summary>
        /// 访问和设置本地ID号
        /// </summary>
        public byte[] MyID
        {
            get { return this.myID; }
            set { this.myID = value; }
        }

        /// <summary>
        /// 访问和设置最大请求子片断长度
        /// </summary>
        public int MaxLength
        {
            get { return this.maxLength; }
            set { this.maxLength = value; }
        }

        /// <summary>
        /// 访问和设置对方ID号
        /// </summary>
        public byte[] DownloadID
        {
            get { return this.downloadID; }
            set { this.downloadID = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connecter">连接管理类</param>
        /// <param name="rawServer">服务器类</param>
        /// <param name="myID">本地下载工具ID号</param>
        /// <param name="maxLength">最大请求子片断长度</param>
        /// <param name="scheduleFunction">计划函数</param>
        /// <param name="keepAliveDelay">发送keep alive信息的时间间隔</param>
        /// <param name="downloadID">对方下载工具ID号</param>
        /// <param name="maxInitiate">最大连接数</param>
        public Encrypter(IConnecter connecter, IRawServer rawServer, byte[] myID, int maxLength,
            SchedulerDelegate scheduleFunction, double keepAliveDelay, byte[] downloadID, int maxInitiate)
        {
            this.rawServer = rawServer;
            this.Connecter = connecter;
            this.MyID = myID;
            this.maxLength = maxLength;
            this.scheduleFunction = scheduleFunction;
            this.keepAliveDelay = keepAliveDelay;
            this.DownloadID = downloadID;
            this.maxInitiate = maxInitiate;
            this.connections = new Dictionary<ISingleSocket, IEncryptedConnection>();
            scheduleFunction(new TaskDelegate(SendKeepAlives), keepAliveDelay, "Send keep alives");
        }

        #endregion

        #region Methods

        /// <summary>
        /// 移除连接
        /// </summary>
        /// <param name="keySocket">待移除的单套接字</param>
        public void Remove(ISingleSocket keySocket)
        {
            this.connections.Remove(keySocket);
        }

        /// <summary>
        /// 发送keepalives网络信息
        /// </summary>
        public void SendKeepAlives()
        {
            scheduleFunction(new TaskDelegate(SendKeepAlives), keepAliveDelay, "Send keep alives");
            foreach (IEncryptedConnection item in connections.Values)
            {
                if (item.Completed)
                    item.SendMessage(0);
            }
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="dns">对方的IP End Point</param>
        /// <param name="id">对方节点的ID号</param>
        public void StartConnect(IPEndPoint dns, byte[] id)
        {
            //如果本地连接数大于最大连接数，则返回
            if (connections.Count >= maxInitiate)
            {
                return;
            }

            //如果ID号相同，则表明是本地发送的网络信息，则返回
            if (Globals.IsSHA1Equal(id, myID))
            {
                return;
            }

            //如果该ID号已经存在，则返回
            foreach(IEncryptedConnection item in connections.Values)
            {
                if (Globals.IsSHA1Equal(id, item.ID))
                {
                    return;
                }
            }

            //开始建立连接
            try
            {
                ISingleSocket singleSocket = rawServer.StartConnect(dns, null);
                connections[singleSocket] = new EncryptedConnection(this, singleSocket, id);
            }

            catch
            {
            }
        }

        /// <summary>
        /// 增加连接
        /// </summary>
        /// <param name="singleSocket">待建立的单套接字</param>
        public void MakeExternalConnection(ISingleSocket singleSocket)
        {
            connections[singleSocket] = new EncryptedConnection(this, singleSocket, null);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="singleSocket">待写入的单套接字</param>
        public void FlushConnection(ISingleSocket singleSocket)
        {
            IEncryptedConnection encryptedConnection = connections[singleSocket];
            if (encryptedConnection.Completed)
            {
                connecter.FlushConnection(encryptedConnection);
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="singleSocket">待关闭的单套接字</param>
        public void CloseConnection(ISingleSocket singleSocket)
        {
            connections[singleSocket].Server();
        }

        /// <summary>
        /// 接受数据
        /// </summary>
        /// <param name="singleSocket">接受的单套接字</param>
        /// <param name="data">接受的数据</param>
        public void DataCameIn(ISingleSocket singleSocket, byte[] data)
        {
            connections[singleSocket].DataCameIn(data);
        }

        #endregion
    }
}
