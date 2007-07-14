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
        /// 自己的下载工具ID号
        /// </summary>
        private byte[] myID;

        /// <summary>
        /// 
        /// </summary>
        private int maxLength;

        /// <summary>
        /// 发送keep alive信息的时间间隔
        /// </summary>
        private double keepAliveDelay;

        /// <summary>
        /// 对方节点的下载工具ID号
        /// </summary>
        private byte[] downloadID;

        /// <summary>
        /// 最大
        /// </summary>
        private int maxInitiate;

        private SchedulerDelegate scheduleFunction;

        #endregion

        #region Public Properties

        public IConnecter Connecter
        {
            get { return this.connecter; }
            set { this.connecter = value; }
        }

        public byte[] MyID
        {
            get { return this.myID; }
            set { this.myID = value; }
        }

        public int MaxLength
        {
            get { return this.maxLength; }
            set { this.maxLength = value; }
        }

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
        /// <param name="connecter"></param>
        /// <param name="rawServer"></param>
        /// <param name="myID"></param>
        /// <param name="maxLength"></param>
        /// <param name="scheduleFunction"></param>
        /// <param name="keepAliveDelay"></param>
        /// <param name="downloadID"></param>
        /// <param name="maxInitiate"></param>
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

        public void Remove(ISingleSocket keySocket)
        {
            this.connections.Remove(keySocket);
        }

        public void SendKeepAlives()
        {
            scheduleFunction(new TaskDelegate(SendKeepAlives), keepAliveDelay, "Send keep alives");
            foreach (IEncryptedConnection item in connections.Values)
            {
                if (item.Completed)
                    item.SendMessage(0);
            }
        }

        public void StartConnect(IPEndPoint dns, byte[] id)
        {
            if (connections.Count >= maxInitiate)
            {
                return;
            }

            if (Globals.IsSHA1Equal(id, myID))
            {
                return;
            }

            foreach(IEncryptedConnection item in connections.Values)
            {
                if (Globals.IsSHA1Equal(id, item.ID))
                {
                    return;
                }
            }

            try
            {
                ISingleSocket singleSocket = rawServer.StartConnect(dns, null);
                connections[singleSocket] = new EncryptedConnection(this, singleSocket, id);
            }

            catch
            {
            }
        }

        public void MakeExternalConnection(ISingleSocket singleSocket)
        {
            connections[singleSocket] = new EncryptedConnection(this, singleSocket, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="singleSocket"></param>
        public void FlushConnection(ISingleSocket singleSocket)
        {
            IEncryptedConnection eConn = connections[singleSocket];
            if (eConn.Completed)
            {
                connecter.FlushConnection(eConn);
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
