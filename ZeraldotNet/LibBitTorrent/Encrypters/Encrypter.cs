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

        private IConnecter connecter;

        private IRawServer rawServer;

        private Dictionary<ISingleSocket, IEncryptedConnection> connections;

        private byte[] myID;

        private int maxLength;

        private double keepAliveDelay;

        private byte[] downloadID;

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

        public Encrypter(IConnecter connecter, IRawServer rawServer, byte[] myID, int maxLength,
            SchedulerDelegate scheduleFunction, double keepAliveDelay, byte[] downloadID, int maxInitiate)
        {
            this.rawServer = rawServer;
            this.Connecter = connecter;
            this.MyID = myID;
            this.MaxLength = maxLength;
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
                if (item.Complete)
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

        public void FlushConnection(ISingleSocket singleSocket)
        {
            IEncryptedConnection eConn = connections[singleSocket];
            if (eConn.Complete)
            {
                connecter.FlushConnection(eConn);
            }
        }

        public void CloseConnection(ISingleSocket singleSocket)
        {
            connections[singleSocket].Server();
        }

        public void DataCameIn(ISingleSocket singleSocket, byte[] data)
        {
            connections[singleSocket].DataCameIn(data);
        }

        #endregion
    }
}
