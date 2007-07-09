using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace ZeraldotNet.TestLibBitTorrent.TestRawServer
{
    public class TestHelper
    {
        private DummyEncrypter handler;

        public DummyEncrypter Handler
        {
            get { return handler; }
            set { handler = value; }
        }
        private DummyRawServer rawServer;

        public DummyRawServer RawServer
        {
            get { return rawServer; }
            set { rawServer = value; }
        }
        private List<TaskDelegate> taskDelegates;

        public List<TaskDelegate> TaskDelegates
        {
            get { return taskDelegates; }
            set { taskDelegates = value; }
        }

        private void Raw()
        {
            rawServer.AddTask(taskDelegates[0], 0.1);
        }

        public void Loop(DummyRawServer rawServer)
        {
            this.rawServer = rawServer;
            this.taskDelegates = new List<TaskDelegate>();
            taskDelegates.Add(new TaskDelegate(Raw));
            rawServer.AddTask(new TaskDelegate(Raw), 0.1);
        }

        private void Go()
        {
            rawServer.ListenForever(handler);
            Debug.Write("listen forever completed");
        }

        public void sl(DummyRawServer rawServer, DummyEncrypter handler, int port)
        {
            rawServer.Bind(port, "127.0.0.1", false);
            this.handler = handler;
            Thread t = new Thread(new ThreadStart(Go));
            t.Start();
        }
    }

    [TestFixture]
    public class TestRawServer
    {
        /// <summary>
        /// Starting side close
        /// </summary>
        [Test]
        public void Test1()
        {
            Flag fa = new Flag();
            Flag fb = new Flag();
            try
            {
                DummyEncrypter da = new DummyEncrypter();
                DummyRawServer sa = new DummyRawServer(fa, 100, 100, false);
                TestHelper ta = new TestHelper();
                ta.Loop(sa);
                ta.sl(sa, da, 6800);
                DummyEncrypter db = new DummyEncrypter();
                DummyRawServer sb = new DummyRawServer(fb, 100, 100, false);
                TestHelper tb = new TestHelper();
                tb.Loop(sb);
                tb.sl(sb, db, 6801);

                Thread.Sleep(1000);
                DummySingleSocket ca = sa.StartConnection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6801), null);
                Thread.Sleep(000);

                Assert.AreEqual(0, da.externalDummySockets.Count);
                Assert.AreEqual(0, da.dataIn.Count);
                Assert.AreEqual(0, da.lostDummySockets.Count);
                Assert.AreEqual(1, db.externalDummySockets.Count);
                DummySingleSocket cb = (DummySingleSocket)db.externalDummySockets[0];
                db.externalDummySockets.Clear();
                Assert.AreEqual(0, db.dataIn.Count);
                Assert.AreEqual(0, db.lostDummySockets.Count);

                ca.Write(new byte[] { (byte)'a', (byte)'a', (byte)'a' });
                cb.Write(new byte[] { (byte)'b', (byte)'b', (byte)'b' });
                Thread.Sleep(1000);

                Assert.AreEqual(0, da.externalDummySockets.Count);
                //da.data_in = ca, 'bbb'
                da.dataIn.Clear();
                Assert.AreEqual(0, da.lostDummySockets.Count);
                Assert.AreEqual(0, db.externalDummySockets.Count);
                //db.data_in = cb, 'aaa'
                db.dataIn.Clear();
                Assert.AreEqual(0, db.lostDummySockets.Count);

                ca.Write(new byte[] { (byte)'c', (byte)'c', (byte)'c' });
                cb.Write(new byte[] { (byte)'d', (byte)'d', (byte)'d' });
                Thread.Sleep(1000);

                Assert.AreEqual(0, da.externalDummySockets.Count);
                //da.data_in = ca, 'ddd'
                da.dataIn.Clear();
                Assert.AreEqual(0, da.lostDummySockets.Count);
                Assert.AreEqual(0, db.externalDummySockets.Count);
                //db.data_in = cb, 'ccc'
                db.dataIn.Clear();
                Assert.AreEqual(0, db.lostDummySockets.Count);

                ca.Close();
                Thread.Sleep(1000);

                Assert.AreEqual(0, da.externalDummySockets.Count);
                Assert.AreEqual(0, da.dataIn.Count);
                Assert.AreEqual(0, da.lostDummySockets.Count);
                Assert.AreEqual(0, db.externalDummySockets.Count);
                Assert.AreEqual(0, db.dataIn.Count);
                Assert.AreEqual(1, db.lostDummySockets.Count);
                db.lostDummySockets.Clear();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            finally
            {
                fa.Set();
                fb.Set();
            }

        }
    }
}
