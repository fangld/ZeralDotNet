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
        /// Simple
        /// </summary>
        [Test]
        public void TestStorage1()
        {
            List<BitFile> files = new List<BitFile>(1);
            BitFile file;
            file.FileName = @"c:\t\a.tmp";
            file.Length = 5;
            files.Add(file);

            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(5, m.TotalLength);


            m.Write(0, new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            byte[] temp = m.Read(0, 3);
            Assert.AreEqual((byte)'a', temp[0]);
            Assert.AreEqual((byte)'b', temp[1]);
            Assert.AreEqual((byte)'c', temp[2]);

            m.Write(2, new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            temp = m.Read(0, 5);
            Assert.AreEqual((byte)'a', temp[0]);
            Assert.AreEqual((byte)'b', temp[1]);
            Assert.AreEqual((byte)'a', temp[2]);
            Assert.AreEqual((byte)'b', temp[3]);
            Assert.AreEqual((byte)'c', temp[4]);
            temp = m.Read(2, 3);
            Assert.AreEqual((byte)'a', temp[0]);
            Assert.AreEqual((byte)'b', temp[1]);
            Assert.AreEqual((byte)'c', temp[2]);

            m.Write(1, new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            temp = m.Read(0, 5);
            Assert.AreEqual((byte)'a', temp[0]);
            Assert.AreEqual((byte)'a', temp[1]);
            Assert.AreEqual((byte)'b', temp[2]);
            Assert.AreEqual((byte)'c', temp[3]);
            Assert.AreEqual((byte)'c', temp[4]);
            m.Close();
        }

        /// <summary>
        /// Multiple
        /// </summary>
        [Test]
        public void TestStorage2()
        {
            string t;
            List<BitFile> files = new List<BitFile>(3);
            BitFile file1;
            file1.FileName = @"c:\t\a.tmp";
            file1.Length = 5;
            files.Add(file1);

            BitFile file2;
            file2.FileName = @"c:\t\b.tmp";
            file2.Length = 4;
            files.Add(file2);

            BitFile file3;
            file3.FileName = @"c:\t\c.tmp";
            file3.Length = 3;
            files.Add(file3);

            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(12, m.TotalLength);

            m.Write(3, new byte[] { (byte)'a', (byte)'b', (byte)'c' }); // 2 in a.temp + 1 in 2.temp
            byte[] temp = m.Read(3, 3);
            t = Encoding.Default.GetString(temp);
            Assert.AreEqual("abc", t);

            m.Write(5, new byte[] { (byte)'a', (byte)'b' });
            temp = m.Read(4, 3);
            Assert.AreEqual((byte)'a', temp[1]);
            Assert.AreEqual((byte)'b', temp[2]);

            t = Encoding.Default.GetString(temp);
            m.Write(3, new byte[] { (byte)'p', (byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w' });
            temp = m.Read(3, 8);
            t = Encoding.Default.GetString(temp);
            Assert.AreEqual("pqrstuvw", t);

            m.Write(3, new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f' });
            temp = m.Read(3, 7);
            t = Encoding.Default.GetString(temp);
            Assert.AreEqual("abcdefv", t);
            m.Close();
        }

        /// <summary>
        /// Zero
        /// </summary>
        [Test]
        public void TestStorage3()
        {
            List<BitFile> files = new List<BitFile>(1);
            BitFile file;
            file.FileName = @"c:\t\d.tmp";
            file.Length = 0;
            files.Add(file);
            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(0, m.TotalLength);
            m.Close();
        }

        /// <summary>
        /// ResumeZero
        /// </summary>
        [Test]
        public void TestStorage4()
        {
            List<BitFile> files = new List<BitFile>(1);
            BitFile file;
            file.FileName = @"c:\t\d.tmp";
            file.Length = 0;
            files.Add(file);
            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(0, m.TotalLength);
            m.Close();
        }

        /// <summary>
        /// WithZero
        /// </summary>
        [Test]
        public void TestStorage5()
        {
            string t;
            List<BitFile> files = new List<BitFile>(3);
            BitFile fileD;
            fileD.FileName = @"c:\t\e.tmp";
            fileD.Length = 3;
            files.Add(fileD);

            BitFile fileE;
            fileE.FileName = @"c:\t\f.tmp";
            fileE.Length = 0;
            files.Add(fileE);

            BitFile fileF;
            fileF.FileName = @"c:\t\g.tmp";
            fileF.Length = 3;
            files.Add(fileF);

            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(6, m.TotalLength);


            m.Write(2, new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            byte[] temp = m.Read(2, 3);
            t = Encoding.Default.GetString(temp);
            Assert.AreEqual("abc", t);
            m.Close();
        }

        /// <summary>
        /// Resume
        /// </summary>
        [Test]
        public void TestStorage6()
        {
            string t;
            List<BitFile> files = new List<BitFile>(1);
            BitFile file4;
            file4.FileName = @"c:\t\a.tmp";
            file4.Length = 5;
            files.Add(file4);

            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(5, m.TotalLength);

            byte[] temp = m.Read(0, 5);
            t = Encoding.Default.GetString(temp);
            Assert.AreEqual("aabab", t);
            m.Close();
        }

        /// <summary>
        /// MixedResume
        /// </summary>
        [Test]
        public void TestStorage7()
        {
            string t;
            List<BitFile> files = new List<BitFile>(2);

            BitFile file5;
            file5.FileName = @"c:\t\a.tmp";
            file5.Length = 5;
            files.Add(file5);

            BitFile file6;
            file6.FileName = @"c:\t\b.tmp";
            file6.Length = 4;
            files.Add(file6);

            Storage m = new Storage(files, 3, null);
            Assert.AreEqual(9, m.TotalLength);

            byte[] temp = m.Read(3, 3);
            t = Encoding.Default.GetString(temp);
            Assert.AreEqual("abc", t);
            m.Close();
        }
    }
}