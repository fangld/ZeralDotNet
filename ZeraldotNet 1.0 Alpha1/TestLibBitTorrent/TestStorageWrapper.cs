using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent;
using NUnit.Framework;
using System.IO;

namespace ZeraldotNet.TestLibBitTorrent
{
    [TestFixture]
    public class TestStorageWrapper
    {
        /// <summary>
        /// Basic
        /// </summary>
        [Test]
        public void TestStorageWrapper1()
        {
            if (File.Exists(@"c:\t\Basic.tmp"))
                File.Delete(@"c:\t\Basic.tmp");

            SHA1Managed shaM = new SHA1Managed();
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            List<byte[]> hashArray = new List<byte[]>(1);
            hashArray.Add(hash);

            List<BitFile> files = new List<BitFile>(1);
            BitFile file = new BitFile(@"c:\t\Basic.tmp", 3);
            files.Add(file);
            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 2, hashArray, 4, null, null, null, new Flag(), true, null);
            Assert.AreEqual(3, sw.AmountLeft);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(1, sw.GetHaveList().Length);
            Assert.AreEqual(false, sw.GetHaveList()[0]);
            Assert.AreEqual(true, sw.DoIHaveRequests(0));

            List<InactiveRequest> requestsList = new List<InactiveRequest>();
            requestsList.Add(sw.NewRequest(0));
            Assert.AreEqual(true, sw.DoIHaveRequests(0));

            requestsList.Add(sw.NewRequest(0));
            Assert.AreEqual(false, sw.DoIHaveRequests(0));

            //Check requestList = 0, 2 and 2, 1
            InactiveRequest ir1 = new InactiveRequest(2, 1);
            sw.RequestLost(0, ir1);
            requestsList.RemoveAt(requestsList.Count - 1);
            Assert.AreEqual(true, sw.DoIHaveRequests(0));
            requestsList.Add(sw.NewRequest(0));

            //Check requestList = 0, 2 and 2, 1
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            sw.PieceCameIn(0, 0, new byte[] { (byte)'a', (byte)'b' });
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(3, sw.AmountLeft);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(1, sw.GetHaveList().Length);
            Assert.AreEqual(false, sw.GetHaveList()[0]);

            sw.PieceCameIn(0, 2, new byte[] { (byte)'c' });
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(0, sw.AmountLeft);
            Assert.AreEqual(true, sw.DoIHaveAnything());
            Assert.AreEqual(1, sw.GetHaveList().Length);
            Assert.AreEqual(true, sw.GetHaveList()[0]);
            Assert.AreEqual("abc", Encoding.ASCII.GetString(sw.GetPiece(0, 0, 3)));
            Assert.AreEqual("bc", Encoding.ASCII.GetString(sw.GetPiece(0, 1, 2)));
            Assert.AreEqual("ab", Encoding.ASCII.GetString(sw.GetPiece(0, 0, 2)));
            Assert.AreEqual("b", Encoding.ASCII.GetString(sw.GetPiece(0, 1, 1)));

            storage1.Close();
        }

         /// <summary>
        /// Two pieces
        /// </summary>
        [Test]
        public void TestStorageWrapper2()
        {
            if (File.Exists(@"c:\t\TwoPieces.tmp"))
                File.Delete(@"c:\t\TwoPieces.tmp");

            SHA1Managed shaM = new SHA1Managed();
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            List<byte[]> hashArray = new List<byte[]>(2);
            hashArray.Add(hash);
            hash = shaM.ComputeHash(new byte[] { (byte)'d' });
            hashArray.Add(hash);


            List<BitFile> files = new List<BitFile>(1);
            BitFile file = new BitFile(@"c:\t\TwoPieces.tmp", 4);
            files.Add(file);
            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 3, hashArray, 3, null, null, null, new Flag(), true, null);
            Assert.AreEqual(4, sw.AmountLeft);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(2, sw.GetHaveList().Length);
            Assert.AreEqual(false, sw.GetHaveList()[0]);
            Assert.AreEqual(false, sw.GetHaveList()[1]);
            Assert.AreEqual(true, sw.DoIHaveRequests(0));
            Assert.AreEqual(true, sw.DoIHaveRequests(1));

            InactiveRequest request1 = sw.NewRequest(0);
            Assert.AreEqual(0, request1.Begin);
            Assert.AreEqual(3, request1.Length);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(2, sw.GetHaveList().Length);
            Assert.AreEqual(false, sw.GetHaveList()[0]);
            Assert.AreEqual(false, sw.GetHaveList()[1]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(true, sw.DoIHaveRequests(1));

            InactiveRequest request2 = sw.NewRequest(1);
            Assert.AreEqual(0, request2.Begin);
            Assert.AreEqual(1, request2.Length);
            Assert.AreEqual(4, sw.AmountLeft);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(2, sw.GetHaveList().Length);
            Assert.AreEqual(false, sw.GetHaveList()[0]);
            Assert.AreEqual(false, sw.GetHaveList()[1]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(false, sw.DoIHaveRequests(1));

            sw.PieceCameIn(0, 0, new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            Assert.AreEqual(1, sw.AmountLeft);
            Assert.AreEqual(true, sw.DoIHaveAnything());
            Assert.AreEqual(2, sw.GetHaveList().Length);
            Assert.AreEqual(true, sw.GetHaveList()[0]);
            Assert.AreEqual(false, sw.GetHaveList()[1]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(false, sw.DoIHaveRequests(1));
            Assert.AreEqual("abc", Encoding.ASCII.GetString(sw.GetPiece(0, 0, 3)));

            sw.PieceCameIn(1, 0, new byte[] { (byte)'d' });
            Assert.AreEqual(0, sw.AmountLeft);
            Assert.AreEqual(true, sw.DoIHaveAnything());
            Assert.AreEqual(2, sw.GetHaveList().Length);
            Assert.AreEqual(true, sw.GetHaveList()[0]);
            Assert.AreEqual(true, sw.GetHaveList()[1]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(false, sw.DoIHaveRequests(1));
            Assert.AreEqual("d", Encoding.ASCII.GetString(sw.GetPiece(1, 0, 1)));

            storage1.Close();
        }

        /// <summary>
        /// Hash fail
        /// </summary>
        [Test]
        public void TestStorageWrapper3()
        {
            if (File.Exists(@"c:\t\HashFail.tmp"))
                File.Delete(@"c:\t\HashFail.tmp");

            SHA1Managed shaM = new SHA1Managed();
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d' });
            List<byte[]> hashArray = new List<byte[]>(1);
            hashArray.Add(hash);


            List<BitFile> files = new List<BitFile>(1);
            BitFile file = new BitFile(@"c:\t\HashFail.tmp", 4);
            files.Add(file);
            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 4, hashArray, 4, null, null, null, new Flag(), true, null);
            Assert.AreEqual(4, sw.AmountLeft);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(false, sw.GetHaveList()[0]);
            Assert.AreEqual(true, sw.DoIHaveRequests(0));

            InactiveRequest request1 = sw.NewRequest(0);
            Assert.AreEqual(0, request1.Begin);
            Assert.AreEqual(4, request1.Length);

            sw.PieceCameIn(0, 0, new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'x' });
            Assert.AreEqual(4, sw.AmountLeft);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(false, sw.GetHaveList()[0]);
            Assert.AreEqual(true, sw.DoIHaveRequests(0));

            InactiveRequest request2 = sw.NewRequest(0);
            Assert.AreEqual(0, request2.Begin);
            Assert.AreEqual(4, request2.Length);

            sw.PieceCameIn(0, 0, new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d' });
            Assert.AreEqual(0, sw.AmountLeft);
            Assert.AreEqual(true, sw.DoIHaveAnything());
            Assert.AreEqual(true, sw.GetHaveList()[0]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));

            storage1.Close();
        }

        /// <summary>
        /// preExist
        /// </summary>
        [Test]
        public void TestStorageWrapper4()
        {
            if (File.Exists(@"c:\t\PreExist.txt"))
                File.Delete(@"c:\t\PreExist.txt");
            File.Create(@"c:\t\PreExist.txt");

            FileStream fs = new FileStream(@"c:\t\PreExist.txt");
            fs.Write(new byte[] { (byte)'q', (byte)'q', 1, 1 }, 0, 4);
            fs.Close();

            SHA1Managed shaM = new SHA1Managed();
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'q', (byte)'q' });
            List<byte[]> hashArray = new List<byte[]>(2);
            hashArray.Add(hash);
            hash = shaM.ComputeHash(new byte[] { (byte)'a', (byte)'b' });
            hashArray.Add(hash);

            BitFile file1 = new BitFile(@"c:\t\PreExist.tmp", 4);
            List<BitFile> files = new List<BitFile>();
            files.Add(file1);

            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 2, hashArray, 2, null, null, null, new Flag(), true, null);
            Assert.AreEqual(2, sw.AmountLeft);
            Assert.AreEqual(true, sw.DoIHaveAnything());
            Assert.AreEqual(true, sw.GetHaveList()[0]);
            Assert.AreEqual(false, sw.GetHaveList()[1]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(true, sw.DoIHaveRequests(1));

            InactiveRequest request = sw.NewRequest(1);
            Assert.AreEqual(0, request.Begin);
            Assert.AreEqual(2, request.Length);

            sw.PieceCameIn(1, 0, new byte[] { (byte)'a', (byte)'b' });
            Assert.AreEqual(0, sw.AmountLeft);
            Assert.AreEqual(true, sw.DoIHaveAnything());
            Assert.AreEqual(true, sw.GetHaveList()[0]);
            Assert.AreEqual(true, sw.GetHaveList()[1]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(false, sw.DoIHaveRequests(1));

            storage1.Close();
        }
    }
}
