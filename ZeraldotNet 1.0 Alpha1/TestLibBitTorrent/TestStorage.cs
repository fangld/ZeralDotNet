using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent;
using NUnit.Framework;
using System.IO;

namespace ZeraldotNet.TestLibBitTorrent
{
    [TestFixture]
    public class TestStorage
    {
        /// <summary>
        /// 简单操作
        /// </summary>
        [Test]
        public void TestStorage1()
        {
            List<BitFile> files = new List<BitFile>(1);
            BitFile file = new BitFile(@"c:\t\a.txt", 5);
            files.Add(file);

            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(5, m.TotalLength);

            //在位置0上写入数据"abc"
            m.Write(0, new byte[] { (byte)'a', (byte)'b', (byte)'c' });

            //读取总文件位置0上3个字节
            byte[] temp = m.Read(0, 3);
            Assert.AreEqual((byte)'a', temp[0]);
            Assert.AreEqual((byte)'b', temp[1]);
            Assert.AreEqual((byte)'c', temp[2]);

            //在位置2上写入数据"abc"，即总数据为"ababc"
            m.Write(2, new byte[] { (byte)'a', (byte)'b', (byte)'c' });

            //读取总文件位置0上5个字节
            temp = m.Read(0, 5);
            Assert.AreEqual((byte)'a', temp[0]);
            Assert.AreEqual((byte)'b', temp[1]);
            Assert.AreEqual((byte)'a', temp[2]);
            Assert.AreEqual((byte)'b', temp[3]);
            Assert.AreEqual((byte)'c', temp[4]);

            //读取总文件位置2上3个字节
            temp = m.Read(2, 3);
            Assert.AreEqual((byte)'a', temp[0]);
            Assert.AreEqual((byte)'b', temp[1]);
            Assert.AreEqual((byte)'c', temp[2]);

            //在位置1上写入数据"abc"，即总数据为"aabcc"
            m.Write(1, new byte[] { (byte)'a', (byte)'b', (byte)'c' });

            //读取总文件位置0上5个字节
            temp = m.Read(0, 5);
            Assert.AreEqual((byte)'a', temp[0]);
            Assert.AreEqual((byte)'a', temp[1]);
            Assert.AreEqual((byte)'b', temp[2]);
            Assert.AreEqual((byte)'c', temp[3]);
            Assert.AreEqual((byte)'c', temp[4]);
            m.Close();
        }

        /// <summary>
        /// 多文件操作
        /// </summary>
        [Test]
        public void TestStorage2()
        {
            string t;
            List<BitFile> files = new List<BitFile>(3);
            BitFile file1  = new BitFile(@"c:\t\a.txt", 5);
            files.Add(file1);

            BitFile file2 = new BitFile(@"c:\t\b.txt", 4);
            files.Add(file2);

            BitFile file3 = new BitFile(@"c:\t\c.txt", 3);
            files.Add(file3);

            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(12, m.TotalLength);

            //在位置3上写入数据"abc"，即总数据为"   abc      "
            m.Write(3, new byte[] { (byte)'a', (byte)'b', (byte)'c' }); // 2 in a.temp + 1 in 2.temp
            //读取总文件位置3上3个字节
            byte[] temp = m.Read(3, 3);
            t = Encoding.Default.GetString(temp);
            Assert.AreEqual("abc", t);

            //在位置5上写入数据"ab"，即总数据为"   abab     "
            m.Write(5, new byte[] { (byte)'a', (byte)'b' });
            //读取总文件位置4上3个字节
            temp = m.Read(4, 3);
            Assert.AreEqual((byte)'a', temp[1]);
            Assert.AreEqual((byte)'b', temp[2]);

            //在位置3上写入数据"pqrstuvw"，即总数据为"   pqrstuvw"
            t = Encoding.Default.GetString(temp);
            m.Write(3, new byte[] { (byte)'p', (byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w' });
            //读取总文件位置3上8个字节
            temp = m.Read(3, 8);
            t = Encoding.Default.GetString(temp);
            Assert.AreEqual("pqrstuvw", t);

            //在位置3上写入数据"abcdef"，即总数据为"   abcdefvw"
            m.Write(3, new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f' });
            //读取总文件位置3上7个字节
            temp = m.Read(3, 7);
            t = Encoding.Default.GetString(temp);
            Assert.AreEqual("abcdefv", t);
            m.Close();
        }

        /// <summary>
        /// 文件长度为0
        /// </summary>
        [Test]
        public void TestStorage3()
        {
            List<BitFile> files = new List<BitFile>(1);
            BitFile file = new BitFile(@"c:\t\d.txt", 0);
            files.Add(file);
            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(0, m.TotalLength);
            m.Close();
        }

        /// <summary>
        /// 继续读写长度为0的文件
        /// </summary>
        [Test]
        public void TestStorage4()
        {
            List<BitFile> files = new List<BitFile>(1);
            BitFile file = new BitFile(@"c:\t\d.txt", 0);
            files.Add(file);
            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(0, m.TotalLength);
            m.Close();
        }

        /// <summary>
        /// 多文件操作，其中含有长度为0的文件
        /// </summary>
        [Test]
        public void TestStorage5()
        {
            string t;
            List<BitFile> files = new List<BitFile>(3);
            BitFile file1 = new BitFile(@"c:\t\e.txt", 3);
            files.Add(file1);

            BitFile file2 = new BitFile(@"c:\t\f.txt", 0);
            files.Add(file2);

            BitFile file3 = new BitFile(@"c:\t\g.txt", 3);
            files.Add(file3);

            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(6, m.TotalLength);

            //在位置2上写入数据"abc"，即总数据为"  abc "
            m.Write(2, new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            byte[] temp = m.Read(2, 3);
            t = Encoding.Default.GetString(temp);
            Assert.AreEqual("abc", t);
            m.Close();
        }

        /// <summary>
        /// 继续简单操作
        /// </summary>
        [Test]
        public void TestStorage6()
        {
            string t;
            List<BitFile> files = new List<BitFile>(1);
            BitFile file = new BitFile(@"c:\t\a.txt", 5);
            files.Add(file);

            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(5, m.TotalLength);

            //读取总文件位置0上5个字节
            byte[] temp = m.Read(0, 5);
            t = Encoding.Default.GetString(temp);
            Assert.AreEqual("aabab", t);
            m.Close();
        }

        /// <summary>
        /// 混合文件操作的继续读写
        /// </summary>
        [Test]
        public void TestStorage7()
        {
            string t;
            List<BitFile> files = new List<BitFile>(2);

            BitFile file1 = new BitFile(@"c:\t\a.txt",5);
            files.Add(file1);

            BitFile file2 = new BitFile(@"c:\t\b.txt", 4);
            files.Add(file2);

            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(9, m.TotalLength);

            //读取总文件位置3上3个字节
            byte[] temp = m.Read(3, 3);
            t = Encoding.Default.GetString(temp);
            Assert.AreEqual("abc", t);
            m.Close();
        }
    }
}