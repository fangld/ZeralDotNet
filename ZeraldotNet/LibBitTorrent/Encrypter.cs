using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 封装连接管理类
    /// </summary>
    public class Encrypter
    {
        #region Private Field

        private Connecter connecter;

        private RawServer rawServer;

        private Dictionary<SingleSocket, EncryptedConnection> connections;

        private byte[] myID;

        private int maxLength;

        private double keepAliveDelay;

        private byte[] downloadID;

        private int maxInitiate;

        private SchedulerDelegate scheduleFunction;

        #endregion

        #region Public Properties

        public Connecter Connecter
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

        public Encrypter(Connecter connecter, RawServer rawServer, byte[] myID, int maxLength,
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
            this.connections = new Dictionary<SingleSocket, EncryptedConnection>();
            scheduleFunction(new TaskDelegate(SendKeepAlives), keepAliveDelay, "Send keep alives");
        }

        #endregion

        #region Methods

        public void Remove(SingleSocket keySocket)
        {
            this.connections.Remove(keySocket);
        }

        public void SendKeepAlives()
        {
            scheduleFunction(new TaskDelegate(SendKeepAlives), keepAliveDelay, "Send keep alives");
            foreach (EncryptedConnection item in connections.Values)
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

            foreach(EncryptedConnection item in connections.Values)
            {
                if (Globals.IsSHA1Equal(id, item.ID))
                {
                    return;
                }
            }

            try
            {
                SingleSocket singleSocket = rawServer.StartConnect(dns, null);
                connections[singleSocket] = new EncryptedConnection(this, singleSocket, id);
            }

            catch
            {
            }
        }

        public void MakeExternalConnection(SingleSocket singleSocket)
        {
            connections[singleSocket] = new EncryptedConnection(this, singleSocket, null);
        }

        public void FlushConnection(SingleSocket singleSocket)
        {
            EncryptedConnection eConn = connections[singleSocket];
            if (eConn.Complete)
            {
                connecter.FlushConnection(eConn);
            }
        }

        public void LoseConnection(SingleSocket singleSocket)
        {
            connections[singleSocket].Server();
        }

        public void DataCameIn(SingleSocket singleSocket, byte[] data)
        {
            connections[singleSocket].DataCameIn(data);
        }

        #endregion
    }
}
