using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class Encrypter
    {
        private Connecter connecter;

        public Connecter Connecter
        {
            get { return this.connecter; }
            set { this.connecter = value; }
        }

        private RawServer rawServer;

        private Dictionary<SingleSocket, EncryptedConnection> connections;

        private byte[] myID;

        public byte[] MyID
        {
            get { return this.myID; }
            set { this.myID = value; }
        }

        private int maxLength;

        public int MaxLength
        {
            get { return this.maxLength; }
            set { this.maxLength = value; }
        }

        private SchedulerDelegate scheduleFunction;

        private double keepAliveDelay;

        private byte[] downloadID;

        public byte[] DownloadID
        {
            get { return this.downloadID; }
            set { this.downloadID = value; }
        }

        private int maxInitiate;

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
            //scheduleFunction(new TaskDelegate(
        }

        public void SendKeepAlives()
        {
            throw new NotImplementedException();

            //scheduleFunction(new TaskDelegate(SendKeepAlives), keepAliveDelay, "Send Keepalives");
            //foreach (EncryptedConnection item in connections.Values)
            //{
            //    if (item.
            //}
        }

    }
}
