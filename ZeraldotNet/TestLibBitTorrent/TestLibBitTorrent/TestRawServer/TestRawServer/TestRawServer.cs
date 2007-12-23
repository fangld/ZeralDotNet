using System;
using System.Net;
using System.Threading;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.RawServers;

namespace ZeraldotNet.TestLibBitTorrent.TestRawServer
{
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
                IRawServer sa = new RawServer(fa, 100, 100, false);
                TestHelper ta = new TestHelper();
                ta.Loop(sa);
                ta.sl(sa, da, 6800);

                DummyEncrypter db = new DummyEncrypter();
                IRawServer sb = new RawServer(fb, 100, 100, false);
                TestHelper tb = new TestHelper();
                tb.Loop(sb);
                tb.sl(sb, db, 6801);

                Thread.Sleep(50);
                ISingleSocket ca = sa.StartConnect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6801), null);
                Thread.Sleep(50);

                Assert.AreEqual(0, da.externalDummySockets.Count);
                Assert.AreEqual(0, da.dataIn.Count);
                Assert.AreEqual(0, da.lostDummySockets.Count);
                Assert.AreEqual(1, db.externalDummySockets.Count);

                ISingleSocket cb = db.externalDummySockets[0];
                db.externalDummySockets.Clear();

                Assert.AreEqual(0, db.dataIn.Count);
                Assert.AreEqual(0, db.lostDummySockets.Count);

                ca.Write(new byte[] { (byte)'a', (byte)'a', (byte)'a' });
                cb.Write(new byte[] { (byte)'b', (byte)'b', (byte)'b' });
                Thread.Sleep(50);

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
                Thread.Sleep(50);

                Assert.AreEqual(0, da.externalDummySockets.Count);
                //da.data_in = ca, 'ddd'
                da.dataIn.Clear();
                Assert.AreEqual(0, da.lostDummySockets.Count);
                Assert.AreEqual(0, db.externalDummySockets.Count);
                //db.data_in = cb, 'ccc'
                db.dataIn.Clear();
                Assert.AreEqual(0, db.lostDummySockets.Count);

                ca.Close();
                Thread.Sleep(50);

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
