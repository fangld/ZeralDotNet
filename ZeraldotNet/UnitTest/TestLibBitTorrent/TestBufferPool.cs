using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeraldotNet.LibBitTorrent;
using NUnit.Framework;

namespace ZeraldotNet.UnitTest.TestLibBitTorrent
{
    [TestFixture]
    public class TestBufferPool
    {
        private const int _capacity = 1024;

        [Test]
        public void Test1()
        {
            BufferPool bufferPool = new BufferPool(_capacity);
            Assert.AreEqual(0, bufferPool.Length);
            Assert.AreEqual(_capacity, bufferPool.Capacity);
        }

        [Test]
        public void Test2()
        {
            BufferPool bufferPool = new BufferPool();
            byte[] writeBytes = new byte[_capacity];
            for (int i=0; i < writeBytes.Length; i++)
            {
                writeBytes[i] = (byte)i;
            }
            bufferPool.Write(writeBytes, 0, writeBytes.Length);
            byte[] readBytes = new byte[(_capacity >> 1)];
            bufferPool.Read(readBytes, 0, readBytes.Length);
            for (int i = 0; i < readBytes.Length; i++)
            {
                Assert.AreEqual(writeBytes[i], readBytes[i]);
            }
        }

        [Test]
        public void Test3()
        {
            BufferPool bufferPool = new BufferPool();
            byte[] writeBytes = new byte[_capacity];
            for (int i = 0; i < writeBytes.Length; i++)
            {
                writeBytes[i] = (byte)i;
            }
            bufferPool.Write(writeBytes, 0, writeBytes.Length);
            byte[] readBytes = new byte[(_capacity >> 1)];
            bufferPool.Read(readBytes, 0, readBytes.Length);
            bufferPool.Seek(-4);
            bufferPool.Read(readBytes, 0, readBytes.Length);
            for (int i = 0; i < readBytes.Length; i++)
            {
                Assert.AreEqual(writeBytes[i + (_capacity >> 1) - 4], readBytes[i]);
            }
        }

        [Test]
        public void Test4()
        {
            BufferPool bufferPool = new BufferPool();
            byte[] writeBytes = new byte[_capacity];
            for (int i = 0; i < writeBytes.Length; i++)
            {
                writeBytes[i] = (byte)i;
            }
            bufferPool.Write(writeBytes, 0, writeBytes.Length);
            byte[] readBytes = new byte[(_capacity >> 1)];
            bufferPool.Seek(4);
            bufferPool.Read(readBytes, 0, readBytes.Length);
            for (int i = 0; i < readBytes.Length; i++)
            {
                Assert.AreEqual(writeBytes[i + 4], readBytes[i]);
            }
        }

        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void Test5()
        {
            BufferPool bufferPool = new BufferPool();
            byte[] bytes = new byte[_capacity];
            bufferPool.Read(bytes, 0, _capacity);
        }

        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void Test6()
        {
            BufferPool bufferPool = new BufferPool();
            byte[] writeBytes = new byte[_capacity];
            Array.Clear(writeBytes, 0, writeBytes.Length);
            bufferPool.Write(writeBytes, 0, writeBytes.Length);
            bufferPool.Seek(-1);
        }

        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void Test7()
        {
            BufferPool bufferPool = new BufferPool();
            byte[] writeBytes = new byte[_capacity];
            byte[] readBytes = new byte[_capacity];

            Array.Clear(writeBytes, 0, writeBytes.Length);
            bufferPool.Write(writeBytes, 0, writeBytes.Length);
            bufferPool.Read(readBytes, 0, readBytes.Length);

            bufferPool.Seek(1);
        }
    }
}
