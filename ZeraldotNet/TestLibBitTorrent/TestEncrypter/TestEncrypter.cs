using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Text;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.TestLibBitTorrent.TestEncrypter
{
    [TestFixture]
    public class TestEncrypter
    {
        private const string protocolName = "BitTorrent protocol";

        private readonly static byte[] handshakeMessage;

        static TestEncrypter()
        {
            handshakeMessage = new byte[68];
            handshakeMessage[0] = 0x13;
            handshakeMessage[1] = 0x42;
            handshakeMessage[2] = 0x69;
            handshakeMessage[3] = 0x74;
            handshakeMessage[4] = 0x54;
            handshakeMessage[5] = 0x6F;
            handshakeMessage[6] = 0x72;
            handshakeMessage[7] = 0x72;
            handshakeMessage[8] = 0x65;
            handshakeMessage[9] = 0x6E;
            handshakeMessage[10] = 0x74;
            handshakeMessage[11] = 0x20;
            handshakeMessage[12] = 0x70;
            handshakeMessage[13] = 0x72;
            handshakeMessage[14] = 0x6F;
            handshakeMessage[15] = 0x74;
            handshakeMessage[16] = 0x6F;
            handshakeMessage[17] = 0x63;
            handshakeMessage[18] = 0x6F;
            handshakeMessage[19] = 0x6C;

            int i;
            for (i = 20; i < 28; i++)
            {
                handshakeMessage[i] = 0;
            }

            for (i = 28; i < 48; i++)
            {
                handshakeMessage[i] = 1;
            }

            for (i = 48; i < 68; i++)
            {
                handshakeMessage[i] = 0;
            }

        }

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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            byte[] temp = c1.Pop();

            int i;
            for (i = 0; i < 68; i++)
            {
                Assert.AreEqual(handshakeMessage[i], temp[i]);
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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            c1.Pop();

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
            c1.Pop();

            IEncryptedConnection ch = (EncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Encoding.Default.GetBytes(ch.IP);

            ch.SendMessage(new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            c1.Pop();

            bytes = new byte[] { 0, 0, 0, 3, (byte)'d', (byte)'e', (byte)'f' };
            e.DataCameIn(c1, bytes);
            c1.Pop();
        }

        /// <summary>
        /// Flush
        /// </summary>
        [Test]
        public void Test3()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            Encrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);

            e.FlushConnection(c1);

            c1.Pop();

            byte[] bytes = new byte[48];
            bytes[0] = 19;
            Buffer.BlockCopy(Encoding.Default.GetBytes(protocolName), 0, bytes, 1, 19);

            int i;
            for (i = 0; i < 8; i++)
                bytes[20 + i] = 0;
            for (i = 0; i < 20; i++)
                bytes[28 + i] = 1;

            e.DataCameIn(c1, bytes);
            c1.Pop();

            bytes = new byte[20];
            for (i = 0; i < 20; i++)
                bytes[i] = (byte)'b';

            e.DataCameIn(c1, bytes);
            c1.Pop();

            EncryptedConnection ch = (EncryptedConnection)((object[])c.log[0])[1];
            c.log.Clear();
            Assert.AreEqual(0, rs.connects.Count);
            Assert.AreEqual(false, c1.Closed);
            Assert.AreEqual(true, ch.IsFlushed);

            e.FlushConnection(c1);
            c1.Pop();

            c1.SetFlushed(false);
            Assert.AreEqual(false, ch.IsFlushed);
        }

        /// <summary>
        /// Wrong header length
        /// </summary>
        [Test]
        public void Test4()
        {
            DummyConnecter c = new DummyConnecter();
            DummyRawServer rs = new DummyRawServer();
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            c1.Pop();
            byte[] bytes = new byte[68];
            Globals.CopyBytes(handshakeMessage, 0, bytes);
            bytes[0] = 5;
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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            c1.Pop();
            byte[] bytes = new byte[68];
            Globals.CopyBytes(handshakeMessage, 0, bytes);            
            int i;
            for (i = 1; i < 20; i++)
                bytes[i] = (byte)'a';

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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            c1.Pop();
            byte[] bytes = new byte[68];
            Globals.CopyBytes(handshakeMessage, 0, bytes);
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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            e.StartConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969), new byte[] { 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9 });
            Debug.Assert(c.log.Count == 0);
            Debug.Assert(rs.connects.Count == 1);
            DummySingleSocket c1 = (DummySingleSocket)((object[])rs.connects[0])[1];
            rs.connects.Clear();
            c1.Pop();
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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, ScheduleLog, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            Assert.AreEqual(1, log.Count);
            Assert.AreEqual(30, (double)((object[])log[0])[1]);
            TaskDelegate kfunc = (TaskDelegate)((object[])log[0])[0];
            log.Clear();
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            c1.Pop();
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
            c1.Pop();
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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();
            e.MakeExternalConnection(c1);
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(0, rs.connects.Count);
            Assert.AreEqual(false, c1.Closed);

            byte[] bytes = new byte[68];
            Globals.CopyBytes(handshakeMessage, 0, bytes);
            int i;
            for (i = 48; i < 68; i++)
                bytes[i] = 2;

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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
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
            IEncryptedConnection ch = (EncryptedConnection)((object[])c.log[0])[1];
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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
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

            c.CloseNext = true;
            e.DataCameIn(c1, new byte[] { 0, 0, 0, 4, 1, 2, 3, 4 });
            Console.WriteLine(c1.Closed);
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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
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

            e.CloseConnection(c1);
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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
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
            IEncrypter e = new Encrypter(c, rs, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 500, DummySchedule, 30, new byte[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }, 40);
            DummySingleSocket c1 = new DummySingleSocket();

            e.StartConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969), new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            Assert.AreEqual(0, c.log.Count);
            Assert.AreEqual(0, rs.connects.Count);
            Assert.AreEqual(false, c1.Closed);
        }
    }
}