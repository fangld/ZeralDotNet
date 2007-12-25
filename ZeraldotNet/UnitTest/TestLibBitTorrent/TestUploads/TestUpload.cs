using System.Collections;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent.Uploads;

namespace ZeraldotNet.UnitTest.TestLibBitTorrent.TestUploads
{
    [TestFixture]
    public class TestUpload
    {
        /// <summary>
        /// Skip over choke
        /// </summary>
        [Test]
        public void Test1()
        {
            ArrayList events = new ArrayList();
            DummyConnection connection = new DummyConnection(events);
            DummyChoker choker = new DummyChoker(events);
            DummyStorageWrapper storageWrapper = new DummyStorageWrapper(events);
            Upload upload = new Upload(connection, choker, storageWrapper, 100, 20, 5);
            Assert.AreEqual(true, upload.Choked);
            Assert.AreEqual(false, upload.Interested);

            upload.GetInterested();
            Assert.AreEqual(true, upload.Interested);

            upload.GetRequest(0, 0, 3);
            connection.Flushed = true;
            upload.Flush();
        }

        /// <summary>
        /// Still rejected after unchoke
        /// </summary>
        [Test]
        public void Test2()
        {
            ArrayList events = new ArrayList();
            DummyConnection connection = new DummyConnection(events);
            DummyChoker choker = new DummyChoker(events);
            DummyStorageWrapper storageWrapper = new DummyStorageWrapper(events);
            Upload upload = new Upload(connection, choker, storageWrapper, 100, 20, 5);
            Assert.AreEqual(true, upload.Choked);
            Assert.AreEqual(false, upload.Interested);

            upload.GetInterested();
            Assert.AreEqual(true, upload.Interested);

            upload.Unchoke();
            Assert.AreEqual(false, upload.Choked);

            upload.GetRequest(0, 0, 3);
            upload.Choke();
            upload.Unchoke();
            connection.Flushed = true;
            upload.Flush();
        }

        /// <summary>
        /// Sends when flushed
        /// </summary>
        [Test]
        public void Test3()
        {
            ArrayList events = new ArrayList();
            DummyConnection connection = new DummyConnection(events);
            DummyChoker choker = new DummyChoker(events);
            DummyStorageWrapper storageWrapper = new DummyStorageWrapper(events);
            Upload upload = new Upload(connection, choker, storageWrapper, 100, 20, 5);

            upload.Unchoke();
            upload.GetInterested();
            upload.GetRequest(0, 1, 3);
            connection.Flushed = true;
            upload.Flush();
            upload.Flush();
        }

        /// <summary>
        /// Sends immediately
        /// </summary>
        [Test]
        public void Test4()
        {
            ArrayList events = new ArrayList();
            DummyConnection connection = new DummyConnection(events);
            DummyChoker choker = new DummyChoker(events);
            DummyStorageWrapper storageWrapper = new DummyStorageWrapper(events);
            Upload upload = new Upload(connection, choker, storageWrapper, 100, 20, 5);

            upload.Unchoke();
            upload.GetInterested();
            connection.Flushed = true;
            upload.GetRequest(0, 1, 3);
        }

        /// <summary>
        /// Cancel
        /// </summary>
        [Test]
        public void Test5()
        {
            ArrayList events = new ArrayList();
            DummyConnection connection = new DummyConnection(events);
            DummyChoker choker = new DummyChoker(events);
            DummyStorageWrapper storageWrapper = new DummyStorageWrapper(events);
            Upload upload = new Upload(connection, choker, storageWrapper, 100, 20, 5);

            upload.Unchoke();
            upload.GetInterested();
            upload.GetRequest(0, 1, 3);
            upload.GetCancel(0, 1, 3);
            upload.GetCancel(0, 1, 2);
            upload.Flush();
            connection.Flushed = true;
        }

        /// <summary>
        /// Clears on not interested
        /// </summary>
        [Test]
        public void Test6()
        {
            ArrayList events = new ArrayList();
            DummyConnection connection = new DummyConnection(events);
            DummyChoker choker = new DummyChoker(events);
            DummyStorageWrapper storageWrapper = new DummyStorageWrapper(events);
            Upload upload = new Upload(connection, choker, storageWrapper, 100, 20, 5);

            upload.Unchoke();
            upload.GetInterested();
            upload.GetRequest(0, 1, 3);
            upload.GetNotInterested();
            connection.Flushed = true;
            upload.Flush();
        }

        /// <summary>
        /// Close when sends on not interested
        /// </summary>
        [Test]
        public void Test7()
        {
            ArrayList events = new ArrayList();
            DummyConnection connection = new DummyConnection(events);
            DummyChoker choker = new DummyChoker(events);
            DummyStorageWrapper storageWrapper = new DummyStorageWrapper(events);
            Upload upload = new Upload(connection, choker, storageWrapper, 100, 20, 5);

            upload.GetRequest(0, 1, 3);
        }

        /// <summary>
        /// Close over max length
        /// </summary>
        [Test]
        public void Test8()
        {
            ArrayList events = new ArrayList();
            DummyConnection connection = new DummyConnection(events);
            DummyChoker choker = new DummyChoker(events);
            DummyStorageWrapper storageWrapper = new DummyStorageWrapper(events);
            Upload upload = new Upload(connection, choker, storageWrapper, 100, 20, 5);

            upload.GetInterested();
            upload.GetRequest(0, 1, 101);
        }

        /// <summary>
        /// no bitfield on start empty
        /// </summary>
        [Test]
        public void Test9()
        {
            ArrayList events = new ArrayList();
            DummyConnection connection = new DummyConnection(events);
            DummyChoker choker = new DummyChoker(events);
            DummyStorageWrapper storageWrapper = new DummyStorageWrapper(events);
            new Upload(connection, choker, storageWrapper, 100, 20, 5);
        }
    }
}
