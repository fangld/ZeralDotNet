using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.UnitTest.TestLibBitTorrent.TestStorages
{
    [TestFixture]
    public class TestStorageWrapper
    {
        private readonly static SHA1Managed shaM;

        static TestStorageWrapper()
        {
            shaM = new SHA1Managed();
        }

        [SetUp]
        public void Initial()
        {
            if (!Directory.Exists("t"))
            {
                Directory.CreateDirectory("t");
            }
        }

        /// <summary>
        /// 基本操作
        /// </summary>
        [Test]
        public void TestStorageWrapper1()
        {
            //如果文件存在，则删除
            if (File.Exists(@"t\Basic.txt"))
                File.Delete(@"t\Basic.txt");

            //计算校验和
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            List<byte[]> hashArray = new List<byte[]>(1);
            hashArray.Add(hash);

            List<BitFile> files = new List<BitFile>(1);
            BitFile file = new BitFile(@"t\Basic.txt", 3);
            files.Add(file);

            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 2, hashArray, 4, null, null, null, new Flag(), true, null);

            Assert.AreEqual(3, sw.LeftLength);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(1, sw.GetHaveList().Length);
            Assert.AreEqual(false, sw.GetHaveList()[0]);
            Assert.AreEqual(true, sw.DoIHaveRequests(0));

            List<InactiveRequest> requestsList = new List<InactiveRequest>();
            //获取第0个子片断
            requestsList.Add(sw.NewRequest(0));
            Assert.AreEqual(true, sw.DoIHaveRequests(0));

            //获取第1个子片断
            requestsList.Add(sw.NewRequest(0));
            Assert.AreEqual(false, sw.DoIHaveRequests(0));


            //检验请求列表（0，2）和（2，1）
            InactiveRequest ir1 = new InactiveRequest(2, 1);

            //丢失第1个子片断
            sw.RequestLost(0, ir1);
            requestsList.RemoveAt(requestsList.Count - 1);
            Assert.AreEqual(true, sw.DoIHaveRequests(0));
            requestsList.Add(sw.NewRequest(0));

            //检验请求列表（0，2）和（2，1）            
            Assert.AreEqual(false, sw.DoIHaveRequests(0));

            //第0个子片断传递了
            sw.PieceCameIn(0, 0, new byte[] { (byte)'a', (byte)'b' });
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(3, sw.LeftLength);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(1, sw.GetHaveList().Length);
            Assert.AreEqual(false, sw.GetHaveList()[0]);

            //第1个子片断传递了
            sw.PieceCameIn(0, 2, new byte[] { (byte)'c' });
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(0, sw.LeftLength);
            Assert.AreEqual(true, sw.DoIHaveAnything());
            Assert.AreEqual(1, sw.GetHaveList().Length);
            Assert.AreEqual(true, sw.GetHaveList()[0]);
            Assert.AreEqual("abc", Encoding.ASCII.GetString(sw.GetPiece(0, 0, 3)));
            Assert.AreEqual("bc", Encoding.ASCII.GetString(sw.GetPiece(0, 1, 2)));
            Assert.AreEqual("ab", Encoding.ASCII.GetString(sw.GetPiece(0, 0, 2)));
            Assert.AreEqual("b", Encoding.ASCII.GetString(sw.GetPiece(0, 1, 1)));

            //关闭文件
            storage1.Close();
        }

         /// <summary>
        /// 两个片断
        /// </summary>
        [Test]
        public void TestStorageWrapper2()
        {
            //如果文件存在，则删除
            if (File.Exists(@"t\TwoPieces.txt"))
                File.Delete(@"t\TwoPieces.txt");

            //计算校验和
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            List<byte[]> hashArray = new List<byte[]>(2);
            hashArray.Add(hash);
            hash = shaM.ComputeHash(new byte[] { (byte)'d' });
            hashArray.Add(hash);

            List<BitFile> files = new List<BitFile>(1);
            BitFile file = new BitFile(@"t\TwoPieces.txt", 4);
            files.Add(file);
            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 3, hashArray, 3, null, null, null, new Flag(), true, null);
            Assert.AreEqual(4, sw.LeftLength);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(2, sw.GetHaveList().Length);
            Assert.AreEqual(false, sw.GetHaveList()[0]);
            Assert.AreEqual(false, sw.GetHaveList()[1]);
            Assert.AreEqual(true, sw.DoIHaveRequests(0));
            Assert.AreEqual(true, sw.DoIHaveRequests(1));

            //请求第0个片断的子片断
            InactiveRequest request1 = sw.NewRequest(0);
            Assert.AreEqual(0, request1.Begin);
            Assert.AreEqual(3, request1.Length);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(2, sw.GetHaveList().Length);
            Assert.AreEqual(false, sw.GetHaveList()[0]);
            Assert.AreEqual(false, sw.GetHaveList()[1]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(true, sw.DoIHaveRequests(1));

            //请求第1个片断的子片断
            InactiveRequest request2 = sw.NewRequest(1);
            Assert.AreEqual(0, request2.Begin);
            Assert.AreEqual(1, request2.Length);
            Assert.AreEqual(4, sw.LeftLength);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(2, sw.GetHaveList().Length);
            Assert.AreEqual(false, sw.GetHaveList()[0]);
            Assert.AreEqual(false, sw.GetHaveList()[1]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(false, sw.DoIHaveRequests(1));

            //第0个片断的子片断传递了
            sw.PieceCameIn(0, 0, new byte[] { (byte)'a', (byte)'b', (byte)'c' });
            Assert.AreEqual(1, sw.LeftLength);
            Assert.AreEqual(true, sw.DoIHaveAnything());
            Assert.AreEqual(2, sw.GetHaveList().Length);
            Assert.AreEqual(true, sw.GetHaveList()[0]);
            Assert.AreEqual(false, sw.GetHaveList()[1]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(false, sw.DoIHaveRequests(1));
            Assert.AreEqual("abc", Encoding.ASCII.GetString(sw.GetPiece(0, 0, 3)));

            //第1个片断的子片断传递了
            sw.PieceCameIn(1, 0, new byte[] { (byte)'d' });
            Assert.AreEqual(0, sw.LeftLength);
            Assert.AreEqual(true, sw.DoIHaveAnything());
            Assert.AreEqual(2, sw.GetHaveList().Length);
            Assert.AreEqual(true, sw.GetHaveList()[0]);
            Assert.AreEqual(true, sw.GetHaveList()[1]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(false, sw.DoIHaveRequests(1));
            Assert.AreEqual("d", Encoding.ASCII.GetString(sw.GetPiece(1, 0, 1)));

            //关闭文件
            storage1.Close();
        }

        /// <summary>
        /// 校验错误
        /// </summary>
        [Test]
        public void TestStorageWrapper3()
        {
            //如果文件存在，则删除
            if (File.Exists(@"t\HashFail.txt"))
                File.Delete(@"t\HashFail.txt");

            //计算校验和
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d' });
            List<byte[]> hashArray = new List<byte[]>(1);
            hashArray.Add(hash);

            List<BitFile> files = new List<BitFile>(1);
            BitFile file = new BitFile(@"t\HashFail.txt", 4);
            files.Add(file);
            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 4, hashArray, 4, null, null, null, new Flag(), true, null);
            Assert.AreEqual(4, sw.LeftLength);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(false, sw.GetHaveList()[0]);
            Assert.AreEqual(true, sw.DoIHaveRequests(0));

            InactiveRequest request1 = sw.NewRequest(0);
            Assert.AreEqual(0, request1.Begin);
            Assert.AreEqual(4, request1.Length);

            //第0个片断的子片断传递了，但是错误
            sw.PieceCameIn(0, 0, new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'x' });
            Assert.AreEqual(4, sw.LeftLength);
            Assert.AreEqual(false, sw.DoIHaveAnything());
            Assert.AreEqual(false, sw.GetHaveList()[0]);
            Assert.AreEqual(true, sw.DoIHaveRequests(0));

            //请求第0个片断的子片断
            InactiveRequest request2 = sw.NewRequest(0);
            Assert.AreEqual(0, request2.Begin);
            Assert.AreEqual(4, request2.Length);

            //第0个片断的子片断传递了，这次是正确的
            sw.PieceCameIn(0, 0, new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d' });
            Assert.AreEqual(0, sw.LeftLength);
            Assert.AreEqual(true, sw.DoIHaveAnything());
            Assert.AreEqual(true, sw.GetHaveList()[0]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));

            //关闭文件
            storage1.Close();
        }

        /// <summary>
        /// 已经存在
        /// </summary>
        [Test]
        public void TestStorageWrapper4()
        {
            //如果文件存在，则删除
            if (File.Exists(@"t\PreExist.txt"))
                File.Delete(@"t\PreExist.txt");

            //将"qq  "写入文件
            StreamWriter streamW = new StreamWriter(@"t\PreExist.txt");
            streamW.Write("qq  ");
            streamW.Close();

            //计算校验和
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'q', (byte)'q' });
            List<byte[]> hashArray = new List<byte[]>(2);
            hashArray.Add(hash);
            hash = shaM.ComputeHash(new byte[] { (byte)'a', (byte)'b' });
            hashArray.Add(hash);

            BitFile file1 = new BitFile(@"t\PreExist.txt", 4);
            List<BitFile> files = new List<BitFile>();
            files.Add(file1);

            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 2, hashArray, 2, null, null, null, new Flag(), true, null);

            //检验第0个片断"qq"是否已经存在
            Assert.AreEqual(2, sw.LeftLength);
            Assert.AreEqual(true, sw.DoIHaveAnything());
            Assert.AreEqual(true, sw.GetHaveList()[0]);
            Assert.AreEqual(false, sw.GetHaveList()[1]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(true, sw.DoIHaveRequests(1));

            //请求第1个片断的子片断
            InactiveRequest request = sw.NewRequest(1);
            Assert.AreEqual(0, request.Begin);
            Assert.AreEqual(2, request.Length);

            //第1个片断的子片断传递了
            sw.PieceCameIn(1, 0, new byte[] { (byte)'a', (byte)'b' });
            Assert.AreEqual(0, sw.LeftLength);
            Assert.AreEqual(true, sw.DoIHaveAnything());
            Assert.AreEqual(true, sw.GetHaveList()[0]);
            Assert.AreEqual(true, sw.GetHaveList()[1]);
            Assert.AreEqual(false, sw.DoIHaveRequests(0));
            Assert.AreEqual(false, sw.DoIHaveRequests(1));

            //关闭文件
            storage1.Close();
        }

        /// <summary>
        /// 总文件太短
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestStorageWrapper5()
        {
            //如果文件存在，则删除
            if (File.Exists(@"t\TotalTooShort.txt"))
                File.Delete(@"t\TotalTooShort.txt");

            //计算校验和
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'q', (byte)'q', (byte)'q', (byte)'q' });
            List<byte[]> hashArray = new List<byte[]>();
            hashArray.Add(hash);
            hash = shaM.ComputeHash(new byte[] { (byte)'q', (byte)'q', (byte)'q', (byte)'q' });
            hashArray.Add(hash);

            BitFile file = new BitFile(@"t\TotalTooShort.txt", 4);
            List<BitFile> files = new List<BitFile>();
            files.Add(file);

            Storage storage1 = new Storage(files, 3, null);
            new StorageWrapper(storage1, 4, hashArray, 4, null, null, null, new Flag(), true, null);

            //关闭文件
            storage1.Close();
        }

        /// <summary>
        /// 总文件太长
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestStorageWrapper6()
        {
            //如果文件存在，则删除
            if (File.Exists(@"t\TotalTooLong.txt"))
                File.Delete(@"t\TotalTooLong.txt");

            //计算校验和
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'q', (byte)'q', (byte)'q', (byte)'q' });
            List<byte[]> hashArray = new List<byte[]>();
            hashArray.Add(hash);
            hash = shaM.ComputeHash(new byte[] { (byte)'q', (byte)'q', (byte)'q', (byte)'q' });
            hashArray.Add(hash);

            BitFile file = new BitFile(@"t\TotalTooLong.txt", 9);
            List<BitFile> files = new List<BitFile>();
            files.Add(file);

            Storage storage1 = new Storage(files, 3, null);
            new StorageWrapper(storage1, 4, hashArray, 4, null, null, null, new Flag(), true, null);

            //关闭文件
            storage1.Close();
        }

        /// <summary>
        /// 请求的数据段超出总文件的长度
        /// </summary>
        [Test]
        public void TestStorageWrapper7()
        {
            //如果文件存在，则删除
            if (File.Exists(@"t\EndAboveTotalLength.txt"))
                File.Delete(@"t\EndAboveTotalLength.txt");

            //计算校验和
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'q', (byte)'q', (byte)'q', (byte)'q' });
            List<byte[]> hashArray = new List<byte[]>();
            hashArray.Add(hash);

            BitFile file = new BitFile(@"t\EndAboveTotalLength.txt", 3);
            List<BitFile> files = new List<BitFile>();
            files.Add(file);

            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 4, hashArray, 4, null, null, null, new Flag(), true, null);
            Assert.AreEqual(null, sw.GetPiece(0, 0, 4));

            //关闭文件
            storage1.Close();
        }

        /// <summary>
        /// 读取的数据段超过片断的长度
        /// </summary>
        [Test]
        public void TestStorageWrapper8()
        {
            //如果文件存在，则删除
            if (File.Exists(@"t\EndPastPieceEnd.txt"))
                File.Delete(@"t\EndPastPieceEnd.txt");

            //计算校验和
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'q', (byte)'q' });
            List<byte[]> hashArray = new List<byte[]>();
            hashArray.Add(hash);
            hash = shaM.ComputeHash(new byte[] { (byte)'q', (byte)'q' });
            hashArray.Add(hash);

            BitFile file = new BitFile(@"t\EndPastPieceEnd.txt", 4);
            List<BitFile> files = new List<BitFile>();
            files.Add(file);

            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 4, hashArray, 2, null, null, null, new Flag(), true, null);
            Assert.AreEqual(null, sw.GetPiece(0, 0, 3));

            //关闭文件
            storage1.Close();
        }

        /// <summary>
        /// 延迟检验
        /// </summary>
        [Test]
        public void TestStorageWrapper9()
        {
            //如果文件存在，则删除
            if (File.Exists(@"t\LazyHashing.txt"))
                File.Delete(@"t\LazyHashing.txt");

            //计算校验和
            List<byte[]> hashArray = new List<byte[]>();
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d' });
            hashArray.Add(hash);

            BitFile file = new BitFile(@"t\LazyHashing.txt", 4);
            List<BitFile> files = new List<BitFile>();
            files.Add(file);

            Storage storage1 = new Storage(files, 3, null);
            Flag flag = new Flag();
            flag.Set();
            StorageWrapper sw = new StorageWrapper(storage1, 4, hashArray, 4, null, null, null, flag, false, null);
            Assert.AreEqual(null, sw.GetPiece(0, 0, 2));
            Assert.AreEqual(true, flag.IsSet);

            //关闭文件
            storage1.Close();
        }

        /// <summary>
        /// 随机分配磁盘空间
        /// </summary>
        [Test]
        public void TestStorageWrapper10()
        {
            //如果文件存在，则删除
            if (File.Exists(@"t\AllocateRandom.txt"))
                File.Delete(@"t\AllocateRandom.txt");

            //计算校验和
            List<byte[]> hashArray = new List<byte[]>();
            byte i;
            for (i = 0; i <= 100; i++)
            {
                hashArray.Add(shaM.ComputeHash(new byte[] { i }));
            }

            BitFile file = new BitFile(@"t\AllocateRandom.txt", 101);
            List<BitFile> files = new List<BitFile>();
            files.Add(file);

            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 1, hashArray, 1, null, null, null, new Flag(), true, null);

            //分别发出子片断请求
            InactiveRequest request;
            for (i = 0; i <= 100; i++)
            {
                request = sw.NewRequest(i);
                Assert.AreEqual(0, request.Begin);
                Assert.AreEqual(1, request.Length);
            }

            //各个子片断分别传递
            for (i = 0; i <= 100; i++)
            {
                sw.PieceCameIn(i, 0, new byte[] { i });
                Assert.AreEqual(100 - i, sw.LeftLength);
            }

            //检验子片断是否获取成功
            for (i = 0; i <= 100; i++)
            {
                Assert.AreEqual(i, sw.GetPiece(i, 0, 1)[0]);
            }

            //关闭文件
            storage1.Close();
        }

        /// <summary>
        /// 继续分配磁盘空间
        /// </summary>
        [Test]
        public void TestStorageWrapper11()
        {
            //如果文件存在，则删除
            if (File.Exists(@"t\AllocateResume.txt"))
                File.Delete(@"t\AllocateResume.txt");

            //计算校验和
            List<byte[]> hashArray = new List<byte[]>();
            byte i;
            for (i = 0; i <= 100; i++)
            {
                hashArray.Add(shaM.ComputeHash(new byte[] { i }));
            }

            BitFile file = new BitFile(@"t\AllocateResume.txt", 101);
            List<BitFile> files = new List<BitFile>();
            files.Add(file);

            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 1, hashArray, 1, null, null, null, new Flag(), true, null);

            //分别发出子片断请求
            InactiveRequest request;
            for (i = 100; i >= 1; i--)
            {
                request = sw.NewRequest(i);
                Assert.AreEqual(0, request.Begin);
                Assert.AreEqual(1, request.Length);
            }
            request = sw.NewRequest(0);
            Assert.AreEqual(0, request.Begin);
            Assert.AreEqual(1, request.Length);

            //首先接收后51个子片断
            for (i = 50; i <= 100; i++)
            {
                sw.PieceCameIn(i, 0, new byte[] { i });
                Assert.AreEqual(true, sw.GetHaveList()[i]);
                Assert.AreEqual(150 - i, sw.LeftLength);
            }

            //检验是否接收后51个子片断成功
            for (i = 50; i <= 100; i++)
            {
                Assert.AreEqual(i, sw.GetPiece(i, 0, 1)[0]);
            }

            //关闭文件
            storage1.Close();

            storage1 = new Storage(files, 3, null);
            sw = new StorageWrapper(storage1, 1, hashArray, 1, null, null, null, new Flag(), true, null);

            //检验是否还有50个子片断没有接收
            Assert.AreEqual(50, sw.LeftLength);

            //检验后面51个子片断的正确性
            for (i = 100; i >= 50; i--)
            {
                Assert.AreEqual(i, sw.GetPiece(i, 0, 1)[0]);
            }

            //分别发出前50个子片断请求
            for (i = 0; i <= 49; i++)
            {
                request = sw.NewRequest(i);
                Assert.AreEqual(0, request.Begin);
                Assert.AreEqual(1, request.Length);
            }

            //接收前50个子片断
            for (i = 0; i <= 49; i++)
            {
                sw.PieceCameIn(i, 0, new byte[] { i });
                Assert.AreEqual(true, sw.GetHaveList()[i]);
                Assert.AreEqual(49 - i, sw.LeftLength);
            }

            //检验是否接收前50个子片断成功
            for (i = 49; i >= 1; i--)
            {
                Assert.AreEqual(i, sw.GetPiece(i, 0, 1)[0]);
            }
            Assert.AreEqual(0, sw.GetPiece(0, 0, 1)[0]);

            //关闭文件
            storage1.Close();
        }

        /// <summary>
        /// 已经存在了某个片断
        /// </summary>
        [Test]
        public void TestStorageWrapper12()
        {
            //如果文件存在，则删除
            if (File.Exists(@"t\LastPieceNotPre.txt"))
                File.Delete(@"t\LastPieceNotPre.txt");

            //写入数据段
            FileStream fs = File.Create(@"t\LastPieceNotPre.txt");
            fs.Write(new byte[] { 255, 1, 255 }, 0, 3);     
            fs.Close();
                
            //计算校验和
            List<byte[]> hashArray = new List<byte[]>();
            byte i;
            for (i = 0; i <= 2; i++)
            {
                hashArray.Add(shaM.ComputeHash(new byte[] { 1 }));
            }

            BitFile file = new BitFile(@"t\LastPieceNotPre.txt", 3);
            List<BitFile> files = new List<BitFile>();
            files.Add(file);

            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 1, hashArray, 1, null, null, null, new Flag(), true, null);

            //检验是否剩余2个片断
            Assert.AreEqual(2, sw.LeftLength);

            //检验第1个片断是否存在
            Assert.AreEqual(false, sw.DoIHaveRequests(1));

            //检验第0,2个片断是否不存在
            Assert.AreEqual(true, sw.DoIHaveRequests(0));
            Assert.AreEqual(true, sw.DoIHaveRequests(2));

            //关闭文件
            storage1.Close();
        }

        /// <summary>
        /// Not rateLast pre
        /// </summary>
        [Test]
        public void TestStorageWrapper13()
        {
            //如果文件存在，则删除
            if (File.Exists(@"t\NotLastPre.txt"))
                File.Delete(@"t\NotLastPre.txt");

            //计算校验和
            List<byte[]> hashArray = new List<byte[]>();
            int i;
            for (i = 0; i < 25; i++)
            {
                hashArray.Add(shaM.ComputeHash(new byte[] { (byte)'a', (byte)'a' }));
            }
            byte[] hash = shaM.ComputeHash(new byte[] { (byte)'b' });
            hashArray.Add(hash);

            BitFile file = new BitFile(@"t\NotLastPre.txt", 51);
            List<BitFile> files = new List<BitFile>();
            files.Add(file);

            Storage storage1 = new Storage(files, 3, null);
            StorageWrapper sw = new StorageWrapper(storage1, 4, hashArray, 2, null, null, null, new Flag(), true, null);

            //检验发出的子片断请求是否正确
            InactiveRequest request;
            for (i = 0; i < 25; i++)
            {
                request = sw.NewRequest(i);
                Assert.AreEqual(0, request.Begin);
                Assert.AreEqual(2, request.Length);
            }
            request = sw.NewRequest(25);
            Assert.AreEqual(0, request.Begin);
            Assert.AreEqual(1, request.Length);

            //第25个子片断传递
            sw.PieceCameIn(25, 0, new byte[] { (byte)'b' });

            //前25个子片断传递
            for (i = 0; i < 25; i++)
            {
                sw.PieceCameIn(i, 0, new byte[] { (byte)'a', (byte)'a' });
            }

            //检验所有片断是否正确
            for (i = 0; i < 25; i++)
            {
                Assert.AreEqual("aa", Encoding.Default.GetString(sw.GetPiece(i, 0, 2)));
            }
            Assert.AreEqual("b", Encoding.Default.GetString(sw.GetPiece(25, 0, 1)));

            //关闭文件
            storage1.Close();
        }
    }
}
