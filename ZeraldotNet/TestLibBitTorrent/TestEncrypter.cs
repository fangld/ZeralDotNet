using System;
using System.Collections.Specialized;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;
using System.Diagnostics;
using ZeraldotNet.LibBitTorrent;

namespace ZeraldotNet.TestLibBitTorrent.TestEncrypter
{
    public class DummyEncryptedConnection
    {
        private const string protocolName = "BitTorrent protocol";

        private const byte protocolNameLength = 19;

        DummyEncrypter encrypter;
        DummySingleSocket connection;

        private byte[] id;

        public byte[] ID
        {
            get { return this.id; }
            set { this.id = value; }
        }

        private bool isLocallyInitiated;

        private bool complete;

        public bool Complete
        {
            get { return this.complete; }
            set { this.complete = value; }
        }

        bool closed;
        MemoryStream buffer;
        int nextLength;
        FuncDelegate nextFunction;

        public DummyEncryptedConnection(DummyEncrypter encrypter, DummySingleSocket connection, byte[] id)
        {
            this.encrypter = encrypter;
            this.connection = connection;
            this.ID = id;
            this.isLocallyInitiated = (id != null);
            this.Complete = false;
            this.closed = false;
            this.buffer = new MemoryStream();
            this.nextLength = 1;
            this.nextFunction = new FuncDelegate(this.ReadHeaderLength);
            connection.Write(new byte[] { protocolNameLength });
            connection.Write(Encoding.Default.GetBytes(protocolName));
            connection.Write(new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
            connection.Write(encrypter.DownloadID);
            connection.Write(encrypter.MyID);
        }

        public string IP
        {
            get { return connection.IP; }
        }

        public bool IsLocallyInitiated
        {
            get { return isLocallyInitiated; }
        }

        public bool Flushed
        {
            get { return connection.Flushed; }
        }

        public NextFunction ReadHeaderLength(byte[] bytes)
        {
            if (bytes[0] != protocolNameLength)
            {
                return null;
            }
            return new NextFunction(protocolNameLength, new FuncDelegate(ReadHeader));
        }

        public NextFunction ReadHeader(byte[] bytes)
        {
            string pName = Encoding.Default.GetString(bytes, 0, protocolName.Length);
            if (pName != protocolName)
            {
                return null;
            }
            return new NextFunction(8, new FuncDelegate(ReadReserved));
        }

        public NextFunction ReadReserved(byte[] bytes)
        {
            return new NextFunction(20, new FuncDelegate(ReadDownloadID));
        }

        public NextFunction ReadDownloadID(byte[] bytes)
        {
            int i;
            for (i = 0; i < 20; i++)
            {
                if (bytes[i] != encrypter.DownloadID[i])
                {
                    return null;
                }
            }

            return new NextFunction(20, new FuncDelegate(ReadPeerID));
        }

        public NextFunction ReadPeerID(byte[] bytes)
        {
            if (this.id == null)
            {
                id = bytes;
            }
            else
            {
                int i;
                for (i = 0; i < 20; i++)
                {
                    if (bytes[i] != id[i])
                    {
                        return null;
                    }
                }
            }
            complete = true;
            encrypter.Connecter.MakeConnection(this);
            return new NextFunction(4, new FuncDelegate(ReadLength));
        }

        public NextFunction ReadLength(byte[] bytes)
        {
            int length = Globals.BytesToInt32(bytes, 0);
            if (length > encrypter.MaxLength)
            {
                return null;
            }
            return new NextFunction(length, new FuncDelegate(ReadMessage));
        }

        public NextFunction ReadMessage(byte[] bytes)
        {
            try
            {
                if (bytes.Length > 0)
                    encrypter.Connecter.GetMessage(this, bytes);
            }
            //TODO: catch (KeyboardInterrupt) {}
            catch //(Exception ex)
            {
                //TODO: Write error log
            }
            return new NextFunction(4, new FuncDelegate(ReadLength));
        }

        public void Close()
        {
            if (!closed)
            {
                connection.Close();
                this.Sever();
            }
        }

        public void Sever()
        {
            closed = true;
            encrypter.Remove(connection);
            if (complete)
                encrypter.Connecter.LoseConnection(this);
        }

        public void SendMessage(byte message)
        {
            SendMessage(new byte[] { message });
        }

        public void SendMessage(byte[] message)
        {
            byte[] lengthBytes = new byte[4];
            Globals.Int32ToBytes(message.Length, lengthBytes, 0);
            connection.Write(lengthBytes);
            connection.Write(message);
        }

        public void DataCameIn(byte[] bytes)
        {
            int i;
            byte[] t, m;
            do
            {
                if (this.closed)
                {
                    return;
                }
                i = this.nextLength - (int)(this.buffer.Position);
                if (i > bytes.Length)
                {
                    this.buffer.Write(bytes, 0, bytes.Length);
                    return;
                }
                this.buffer.Write(bytes, 0, i);
                t = new byte[bytes.Length - i];
                Buffer.BlockCopy(bytes, i, t, 0, bytes.Length - i);
                bytes = t;
                m = this.buffer.ToArray();
                this.buffer.Close();
                this.buffer = new MemoryStream();
                NextFunction x = this.nextFunction(m);
                if (x == null)
                {
                    this.Close();
                    return;
                }
                this.nextLength = x.Length;
                this.nextFunction = x.NextFunc;
            } while (true);
        }
    }

    public class DummyEncrypter
    {
        public DummyConnecter connecter;

        public DummyConnecter Connecter
        {
            get { return this.connecter; }
            set { this.connecter = value; }
        }

        private DummyRawServer rawServer;
        public Dictionary<DummySingleSocket, DummyEncryptedConnection> connections;

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

        public void Remove(DummySingleSocket keySocket)
        {
            this.connections.Remove(keySocket);
        }

        SchedulerDelegate scheduleFunction;
        private double keepAliveDelay;
        public byte[] DownloadID;
        private int maxInitiate;

        public DummyEncrypter(DummyConnecter connecter, DummyRawServer rawServer, byte[] MyID, int maxLength, SchedulerDelegate scheduleFunction, double keepAliveDelay, byte[] downloadID, int maxInitiate)
        {
            this.rawServer = rawServer;
            this.Connecter = connecter;
            this.MyID = MyID;
            this.MaxLength = maxLength;
            this.scheduleFunction = scheduleFunction;
            this.keepAliveDelay = keepAliveDelay;
            this.DownloadID = downloadID;
            this.maxInitiate = maxInitiate;
            this.connections = new Dictionary<DummySingleSocket, DummyEncryptedConnection>();
            scheduleFunction(new TaskDelegate(SendKeepAlives), keepAliveDelay, "Send keep alives");
        }

        public void SendKeepAlives()
        {
            scheduleFunction(new TaskDelegate(SendKeepAlives), keepAliveDelay, "Send keep alives");
            foreach (DummyEncryptedConnection item in connections.Values)
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

            foreach (DummyEncryptedConnection item in connections.Values)
            {
                if (Globals.IsSHA1Equal(id, item.ID))
                {
                    return;
                }
            }

            try
            {
                DummySingleSocket singleSocket = rawServer.StartConnect(dns, null);
                connections[singleSocket] = new DummyEncryptedConnection(this, singleSocket, id);
            }

            catch
            {
            }
        }

        public void MakeExternalConnection(DummySingleSocket connection)
        {
            connections[connection] = new DummyEncryptedConnection(this, connection, null);
        }

        public void FlushConnection(DummySingleSocket connection)
        {
            DummyEncryptedConnection eConn = connections[connection];
            if (eConn.Complete)
                Connecter.FlushConnection(eConn);
        }

        public void LoseConnection(DummySingleSocket connection)
        {
            connections[connection].Sever();
        }

        public void DataCameIn(DummySingleSocket connection, byte[] data)
        {
            connections[connection].DataCameIn(data);
        }
    }

    public class DummyConnecter
    {
        public ArrayList log;
        public bool closeNext;

        public DummyConnecter()
        {
            this.log = new ArrayList();
            this.closeNext = false;
        }

        public void MakeConnection(DummyEncryptedConnection connection)
        {
            log.Add(new object[] { "make ", connection });
        }

        public void LoseConnection(DummyEncryptedConnection connection)
        {
            log.Add(new object[] { "lose ", connection });
        }

        public void FlushConnection(DummyEncryptedConnection connection)
        {
            log.Add(new object[] { "flush ", connection });
        }

        public void GetMessage(DummyEncryptedConnection connection, byte[] message)
        {
            log.Add(new object[] { "get ", connection, message });
            if (closeNext)
                connection.Close();
        }
    }

    public class DummyRawServer
    {
        public ArrayList connects;

        public DummyRawServer()
        {
            connects = new ArrayList();
        }

        public DummySingleSocket StartConnect(IPEndPoint dns, object o)
        {
            DummySingleSocket c = new DummySingleSocket();
            connects.Add(new object[] { dns, c });
            return c;
        }
    }

    public class DummySingleSocket
    {
        private bool closed;

        public bool Closed
        {
            get { return closed; }
            set { closed = value; }
        }

        public List<byte[]> data;

        private bool flushed;

        public bool Flushed
        {
            get { return flushed; }
            set { flushed = value; }
        }


        public DummySingleSocket()
        {
            closed = false;
            data = new List<byte[]>();
            flushed = true;
        }

        public string IP
        {
            get { return "fake.ip"; }
        }

        public void Write(byte[] data)
        {
            Debug.Assert(!closed);
            this.data.Add(data);
        }

        public void Close()
        {
            Debug.Assert(!closed);
            closed = true;
        }

        public byte[] Pop()
        {
            List<byte> result = new List<byte>();
            foreach (byte[] b in data)
            {
                result.AddRange(b);

            }
            data.Clear();
            return result.ToArray();
        }
    }

    [TestFixture]
    public class TestEncrypter
    {
        private const string protocolName = "BitTorrent protocol";

        public static void DummySchedule(TaskDelegate t, double a, string taskName)
        {
        }

        static ArrayList log;

        public static void ScheduleLog(TaskDelegate t, double a, string taskName)
        {
            log.Add(new object[] { t, a });
        }

        /// <summary>
        /// Test handshake message
        /// </summary>
        [Test]
        public void Test1()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            byte[] temp = c1.Pop();
            byte[] handShakeMessage = new byte[68];

            handShakeMessage[0] = 0x13;
            handShakeMessage[1] = 0x42;
            handShakeMessage[2] = 0x69;
            handShakeMessage[3] = 0x74;
            handShakeMessage[4] = 0x54;
            handShakeMessage[5] = 0x6F;
            handShakeMessage[6] = 0x72;
            handShakeMessage[7] = 0x72;
            handShakeMessage[8] = 0x65;
            handShakeMessage[9] = 0x6E;
            handShakeMessage[10] = 0x74;
            handShakeMessage[11] = 0x20;
            handShakeMessage[12] = 0x70;
            handShakeMessage[13] = 0x72;
            handShakeMessage[14] = 0x6F;
            handShakeMessage[15] = 0x74;
            handShakeMessage[16] = 0x6F;
            handShakeMessage[17] = 0x63;
            handShakeMessage[18] = 0x6F;
            handShakeMessage[19] = 0x6C;

            int i;
            for (i = 20; i < 28; i++)
            {
                handShakeMessage[i] = 0;
            }

            for (i = 28; i < 48; i++)
            {
                handShakeMessage[i] = 1;
            }

            for (i = 48; i < 68; i++)
            {
                handShakeMessage[i] = 0;
            }

            for (i = 0; i < 68; i++)
            {
                Assert.AreEqual(handShakeMessage[i], temp[i]);
            }
        }

        /// <summary>
        /// Messages in and out
        /// </summary>
        [Test]
        public void Test2()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            byte[] temp = c1.Pop();

            byte[] bytes = new byte[48];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.Default.GetBytes(protocolName), 0, bytes, 1, 19);

            int i;
            for (i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (i = 0; i < 20; i++)
                bytes[28 + i] = 1;

            e.DataCameIn(c1, bytes);


            bytes = new byte[20];
            for (i = 0; i < 20; i++)
                bytes[i] = (byte)'b';
            e.DataCameIn(c1, bytes);
            temp = c1.Pop();

            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            temp = Encoding.Default.GetBytes(ch.IP);

            ch.SendMessage(new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            temp = c1.Pop();

            bytes = new byte[] { 0, 0, 0, 3, (byte)'d', (byte)'e', (byte)'f' };
            e.DataCameIn(c1, bytes);
            temp = c1.Pop();
        }

        /// <summary>
        /// Flush
        /// </summary>
        [Test]
        public void Test3()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            byte[] temp = c1.Pop();

            byte[] bytes = new byte[48];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.Default.GetBytes(protocolName), 0, bytes, 1, 19);

            int i;
            for (i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (i = 0; i < 20; i++)
                bytes[28 + i] = 1;
            e.DataCameIn(c1, bytes);
            temp = c1.Pop();

            e.FlushConnection(c1);

            temp = c1.Pop();

            bytes = new byte[20];
            for (i = 0; i < 20; i++)
                bytes[i] = (byte)'b';
            e.DataCameIn(c1, bytes);
            temp = c1.Pop();

            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Assert.AreEqual(0, rs.connects.Count);
            Assert.AreEqual(false, c1.Closed);
            Assert.AreEqual(true, ch.Flushed);

            e.FlushConnection(c1);
            temp = c1.Pop();

            c1.Flushed = false;
            Assert.AreEqual(false, ch.Flushed);
        }

        /// <summary>
        /// Wrong header length
        /// </summary>
        [Test]
        public void Test4()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            byte[] temp = c1.Pop();
            byte[] bytes = new byte[30];
            for (int i = 0; i < 30; i++)
                bytes[i] = 5;
            e.DataCameIn(c1, bytes);
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(true, c1.Closed);
        }

        /// <summary>
        /// Wrong header
        /// </summary>
        [Test]
        public void Test5()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            byte[] temp = c1.Pop();
            byte[] bytes = new byte[20];
            bytes[0] = 19;
            for (int i = 0; i < 19; i++)
                bytes[i + 1] = (byte)'a';
            e.DataCameIn(c1, bytes);
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(true, c1.Closed);
        }

        /// <summary>
        /// Wrong download id
        /// </summary>
        [Test]
        public void Test6()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            byte[] temp = c1.Pop();
            byte[] bytes = new byte[48];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.Default.GetBytes(protocolName), 0, bytes, 1, 19);
            for (int i = 0; i < 8; i++)
                bytes[20 + i] = 1;
            for (int i = 0; i < 20; i++)
                bytes[28 + i] = 2;
            e.DataCameIn(c1, bytes);
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(true, c1.Closed);
        }

        /// <summary>
        /// Wrong other id
        /// </summary>
        [Test]
        public void Test7()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            e.StartConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969), new byte[] { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 });
            Debug.Assert(c.log.Count == 0);
            Debug.Assert(rs.connects.Count == 1);
            DummySingleSocket c1 = (DummySingleSocket)((object[])rs.connects[0])[1];
            rs.connects.Clear();
            byte[] temp = c1.Pop();
            Assert.AreEqual(false, c1.Closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.Default.GetBytes(protocolName), 0, bytes, 1, 19);

            int i;
            for (i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.DataCameIn(c1, bytes);
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(true, c1.Closed);
        }

        /// <summary>
        /// Over max length
        /// </summary>
        [Test]
        public void Test8()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(0, rs.connects.Count);
            Assert.AreEqual(false, c1.Closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.Default.GetBytes(protocolName), 0, bytes, 1, 19);

            int i;
            for (i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.DataCameIn(c1, bytes);
            Assert.AreEqual(1, c.log.Count);

            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Assert.AreEqual(false, c1.Closed);

            e.DataCameIn(c1, new byte[] { 1, 0, 0, 0 });
            Assert.AreEqual(1, c.log.Count);
            Assert.AreEqual(true, c1.Closed);
        }

        /// <summary>
        /// Keep alive
        /// </summary>
        [Test]
        public void Test9()
        {
            log = new ArrayList();

            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(ScheduleLog), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            Assert.AreEqual(1, log.Count);
            Assert.AreEqual(30, (double)((object[])log[0])[1]);
            TaskDelegate kfunc = (TaskDelegate)((object[])log[0])[0];
            log.Clear();
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            byte[] temp = c1.Pop();
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(false, c1.Closed);
            kfunc();
            Assert.AreEqual(new byte[0], c1.Pop());
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(false, c1.Closed);
            log.Clear();

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.Default.GetBytes(protocolName), 0, bytes, 1, 19);

            int i;
            for (i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.DataCameIn(c1, bytes);
            Assert.AreEqual(1, c.log.Count);
            c.log.Clear();
            byte[] result = c1.Pop();
            Assert.AreEqual(new byte[0], c1.Pop());
            Assert.AreEqual(false, c1.Closed);

            kfunc();
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(false, c1.Closed);
        }

        /// <summary>
        /// Swallow keep alive
        /// </summary>
        [Test]
        public void Test10()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(0, rs.connects.Count);
            Assert.AreEqual(false, c1.Closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.Default.GetBytes(protocolName), 0, bytes, 1, 19);

            int i;
            for (i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.DataCameIn(c1, bytes);
            Assert.AreEqual(1, c.log.Count);
            c.log.Clear();
            Assert.AreEqual(false, c1.Closed);

            e.DataCameIn(c1, new byte[] { 0, 0, 0, 0 });
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(false, c1.Closed);

        }

        /// <summary>
        /// Local close
        /// </summary>
        [Test]
        public void Test11()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(0, rs.connects.Count);
            Assert.AreEqual(false, c1.Closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.Default.GetBytes(protocolName), 0, bytes, 1, 19);

            int i;
            for (i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.DataCameIn(c1, bytes);
            Assert.AreEqual(1, c.log.Count);
            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Assert.AreEqual(false, c1.Closed);

            ch.Close();
            Assert.AreEqual(true, c1.Closed);
        }

        /// <summary>
        /// Local close in message receive
        /// </summary>
        [Test]
        public void Test12()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(0, rs.connects.Count);
            Assert.AreEqual(false, c1.Closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.Default.GetBytes(protocolName), 0, bytes, 1, 19);

            int i;
            for (i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.DataCameIn(c1, bytes);
            Assert.AreEqual(1, c.log.Count);
            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Assert.AreEqual(false, c1.Closed);

            c.closeNext = true;
            e.DataCameIn(c1, new byte[] { 0, 0, 0, 4, 1, 2, 3, 4 });
            Assert.AreEqual(true, c1.Closed);
        }

        /// <summary>
        /// Remote close
        /// </summary>
        [Test]
        public void Test13()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(0, rs.connects.Count);
            Assert.AreEqual(false, c1.Closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.Default.GetBytes(protocolName), 0, bytes, 1, 19);

            int i;
            for (i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 2;
            }
            e.DataCameIn(c1, bytes);
            Assert.AreEqual(1, c.log.Count);
            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Assert.AreEqual(false, c1.Closed);

            e.LoseConnection(c1);
            Assert.AreEqual(false, c1.Closed);
        }

        /// <summary>
        /// Partial data in
        /// </summary>
        [Test]
        public void Test14()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(0, rs.connects.Count);
            Assert.AreEqual(false, c1.Closed);

            byte[] bytes = new byte[24];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.Default.GetBytes(protocolName), 0, bytes, 1, 19);

            int i;
            for (i = 0; i < 4; i++)
                bytes[20 + i] = 0;
            e.DataCameIn(c1, bytes);
            bytes = new byte[34];
            for (i = 0; i < 4; i++)
                bytes[i] = 0;
            for (i = 0; i < 20; i++)
            {
                bytes[4 + i] = 1;
            }
            for (i = 0; i < 10; i++)
            {
                bytes[24 + i] = 2;
            }
            e.DataCameIn(c1, bytes);
            bytes = new byte[10];
            for (i = 0; i < 10; i++)
            {
                bytes[i] = 2;
            }
            e.DataCameIn(c1, bytes);
            Assert.AreEqual(1, c.log.Count);
            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Assert.AreEqual(false, c1.Closed);

        }

        /// <summary>
        /// Ignore connect of extant
        /// </summary>
        [Test]
        public void Test15()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            Debug.Assert(c.log.Count == 0);
            Debug.Assert(rs.connects.Count == 0);
            Debug.Assert(!c1.Closed);

            byte[] bytes = new byte[68];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.Default.GetBytes(protocolName), 0, bytes, 1, 19);

            int i;
            for (i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (i = 0; i < 20; i++)
            {
                bytes[28 + i] = 1;
                bytes[48 + i] = 9;
            }
            e.DataCameIn(c1, bytes);
            Assert.AreEqual(1, c.log.Count);
            DummyEncryptedConnection ch = (DummyEncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Assert.AreEqual(false, c1.Closed);

            e.StartConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969), new byte[] { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 });
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(0, rs.connects.Count);
            Assert.AreEqual(false, c1.Closed);
        }

        /// <summary>
        /// Ignore connect to self
        /// </summary>
        [Test]
        public void Test16()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            DummyEncrypter e = new DummyEncrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, new SchedulerDelegate(DummySchedule), 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();

            e.StartConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969), new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(0, rs.connects.Count);
            Assert.AreEqual(false, c1.Closed);
        }
    }
}