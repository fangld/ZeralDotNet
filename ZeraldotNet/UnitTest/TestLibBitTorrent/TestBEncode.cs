using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.BEncoding;

namespace ZeraldotNet.UnitTest.TestLibBitTorrent
{
    [TestFixture]
    public class TestBEncode
    {
        #region Parse test

        private static Encoding _encoding = Encoding.UTF8;

        #region Handler解码测试
        [Test]
        public void TestDecodeHandler1()
        {
            byte[] source = File.ReadAllBytes(@"D:\Bittorrent\winedt60.exe.torrent");
            DictNode dh = (DictNode)BEncoder.Decode(source);
            Assert.AreEqual("http://192.168.1.155:8080/announce", (dh["announce"] as BytesNode).StringText);
            Assert.AreEqual("http://192.168.1.155:8080/announce", _encoding.GetString((dh["announce"] as BytesNode).ByteArray));
        }

        /// <summary>
        /// Handler解码测试函数2,测试用例为""
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeHandler2()
        {
            BEncoder.Decode("");
        }

        /// <summary>
        /// Handler解码测试函数3,测试用例为"35208734823ljdahflajhdf"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeHandler3()
        {
            BEncoder.Decode("35208734823ljdahflajhdf");
        }
        #endregion

        #region 整数解码测试
        /// <summary>
        /// 整数解码测试函数1,测试用例为正确用例
        /// </summary>
        [Test]
        public void TestDecodeInteger1()
        {
            //Test1正整数
            IntNode ih1 = (IntNode)BEncoder.Decode("i10e");
            Assert.AreEqual(ih1.Value, 10);

            //Test2零
            IntNode ih2 = (IntNode)BEncoder.Decode("i0e");
            Assert.AreEqual(ih2.Value, 0);

            //Test3负整数
            IntNode ih3 = (IntNode)BEncoder.Decode("i-55e");
            Assert.AreEqual(ih3.Value, -55);

            //Test4所有的数字
            IntNode ih4 = (IntNode)BEncoder.Decode("i1234567890e");
            Assert.AreEqual(ih4.Value, 1234567890);
        }

        /// <summary>
        /// 整数解码测试函数2,测试用例为"ie"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeInteger2()
        {
            BEncoder.Decode("ie");
        }

        /// <summary>
        /// 整数解码测试函数3,测试用例为"i341foo382e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeInteger3()
        {
            BEncoder.Decode("i341foo382e");
        }

        /// <summary>
        /// 整数解码测试函数4,测试用例为"index-0e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeInteger4()
        {
            BEncoder.Decode("i-0e");
        }

        /// <summary>
        /// 整数解码测试函数5,测试用例为"i123"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeInteger5()
        {
            BEncoder.Decode("i123");
        }

        /// <summary>
        /// 整数解码测试函数6,测试用例为"i0345e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeInteger6()
        {
            BEncoder.Decode("i0345e");
        }
        #endregion

        #region 字节数组解码测试
        /// <summary>
        /// 字节数组解码测试函数1,测试用例为正确用例
        /// </summary>
        [Test]
        public void TestDecodeByteArray1()
        {
            //Test1
            BytesNode bah1 = (BytesNode)BEncoder.Decode("10:0123456789");
            Assert.AreEqual(bah1.ByteArray, _encoding.GetBytes("0123456789"));
            Assert.AreEqual(bah1.StringText, "0123456789");

            //Test2
            BytesNode bah2 = (BytesNode)BEncoder.Decode("26:abcdefghijklmnopqrstuvwxyz");
            Assert.AreEqual(bah2.ByteArray, _encoding.GetBytes("abcdefghijklmnopqrstuvwxyz"));
            Assert.AreEqual(bah2.StringText, "abcdefghijklmnopqrstuvwxyz");

            //Test3
            BytesNode bah3 = (BytesNode)BEncoder.Decode("186:ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９");
            Assert.AreEqual(bah3.ByteArray, _encoding.GetBytes("ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９"));
            Assert.AreEqual(bah3.StringText, "ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９");

            //Test4
            BytesNode bah4 = (BytesNode)BEncoder.Decode("0:");
            Assert.AreEqual(bah4.ByteArray, _encoding.GetBytes(string.Empty));
            Assert.AreEqual(bah4.StringText, string.Empty);
        }

        /// <summary>
        /// 字节数组解码测试函数2,测试用例为"2:abcedefg"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeByteArray2()
        {
            BEncoder.Decode("2:abcedefg");
        }

        /// <summary>
        /// 字节数组解码测试函数3,测试用例为"02:ab"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeByteArray3()
        {
            BEncoder.Decode("02:ab");
        }

        /// <summary>
        /// 字节数组解码测试函数4,测试用例为"0:0:"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeByteArray4()
        {
            BEncoder.Decode("0:0:");
        }

        /// <summary>
        /// 字节数组解码测试函数5,测试用例为"9:abc"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeByteArray5()
        {
            BEncoder.Decode("9:abc");
        }
        #endregion

        #region 列表解码测试
        /// <summary>
        /// 列表解码测试函数1,测试用例为正确用例
        /// </summary>
        [Test]
        public void TestDecodeList1()
        {
            //Test1整数
            ListNode lh1 = (ListNode)BEncoder.Decode("li0ei1ei2ee");
            Assert.AreEqual(((IntNode)lh1[0]).Value, 0);
            Assert.AreEqual(((IntNode)lh1[1]).Value, 1);
            Assert.AreEqual(((IntNode)lh1[2]).Value, 2);

            //Test2字节数组
            ListNode lh2 = (ListNode)BEncoder.Decode("l3:abc2:xye");
            Assert.AreEqual((lh2[0] as BytesNode).ByteArray, _encoding.GetBytes("abc"));
            Assert.AreEqual((lh2[0] as BytesNode).StringText, "abc");

            Assert.AreEqual((lh2[1] as BytesNode).ByteArray, _encoding.GetBytes("xy"));
            Assert.AreEqual((lh2[1] as BytesNode).StringText, "xy");

            //Test3空字节数组
            ListNode lh3 = (ListNode)BEncoder.Decode("l0:0:0:e");
            Assert.AreEqual((lh3[0] as BytesNode).ByteArray, _encoding.GetBytes(string.Empty));
            Assert.AreEqual((lh3[0] as BytesNode).StringText, string.Empty);

            Assert.AreEqual((lh3[1] as BytesNode).ByteArray, _encoding.GetBytes(string.Empty));
            Assert.AreEqual((lh3[1] as BytesNode).StringText, string.Empty);

            Assert.AreEqual((lh3[2] as BytesNode).ByteArray, _encoding.GetBytes(string.Empty));
            Assert.AreEqual((lh3[2] as BytesNode).StringText, string.Empty);

            //Test4字节数组与整数
            ListNode lh4 = (ListNode)BEncoder.Decode("ll5:Alice3:Bobeli2ei3eee");
            ListNode lHandler40 = (ListNode)lh4[0];
            ListNode lHandler41 = (ListNode)lh4[1];

            Assert.AreEqual((lHandler40[0] as BytesNode).ByteArray, _encoding.GetBytes("Alice"));
            Assert.AreEqual((lHandler40[0] as BytesNode).StringText, "Alice");

            Assert.AreEqual((lHandler40[1] as BytesNode).ByteArray, _encoding.GetBytes("Bob"));
            Assert.AreEqual((lHandler40[1] as BytesNode).StringText, "Bob");

            Assert.AreEqual(((IntNode)lHandler41[0]).Value, 2);

            Assert.AreEqual(((IntNode)lHandler41[1]).Value, 3);

            //Test5空列表
            ListNode lh5 = (ListNode)BEncoder.Decode("le");
            Assert.AreEqual(lh5.Count, 0);
        }

        /// <summary>
        /// 列表解码测试函数2,测试用例为"lezeral"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeList2()
        {
            BEncoder.Decode("lezeral");
        }

        /// <summary>
        /// 列表解码测试函数3,测试用例为"l"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeList3()
        {
            BEncoder.Decode("l");
        }

        /// <summary>
        /// 列表解码测试函数4,测试用例为"l0:"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeList4()
        {
            BEncoder.Decode("l0:");
        }

        /// <summary>
        /// 列表解码测试函数5,测试用例为"l01:xe"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeList5()
        {
            BEncoder.Decode("l01:xe");
        }
        #endregion

        #region 字典解码测试
        /// <summary>
        /// 字典解码测试函数1,测试用例为正确用例
        /// </summary>
        [Test]
        public void TestDecodeDictionary1()
        {
            //Test1整数
            DictNode dh1 = (DictNode)BEncoder.Decode("d3:agei25ee");
            Assert.AreEqual(((IntNode)dh1["age"]).Value, 25);

            //Test2字节数组
            DictNode dh2 = (DictNode)BEncoder.Decode("d3:agei25e5:color4:bluee");
            Assert.AreEqual(((IntNode)dh2["age"]).Value, 25);

            Assert.AreEqual((dh2["color"] as BytesNode).ByteArray, _encoding.GetBytes("blue"));
            Assert.AreEqual((dh2["color"] as BytesNode).StringText, "blue");

            //Test3字节数组与整数
            DictNode dh3 = (DictNode)BEncoder.Decode("d8:spam.mp3d6:author5:Alice6:lengthi1048576eee");
            DictNode dHandler31 = (DictNode)dh3["spam.mp3"];
            Assert.AreEqual((dHandler31["author"] as BytesNode).ByteArray, _encoding.GetBytes("Alice"));
            Assert.AreEqual((dHandler31["author"] as BytesNode).StringText, "Alice");
            Assert.AreEqual(((IntNode)dHandler31["length"]).Value, 1048576);

            //Test4空字典
            DictNode dh4 = (DictNode)BEncoder.Decode("de");
            Assert.AreEqual(dh4.Count, 0);
        }

        /// <summary>
        /// 字典解码测试函数2,测试用例为"d3:agei25e3:agei50ee"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary2()
        {
            BEncoder.Decode("d3:agei25e3:agei50ee");
        }

        /// <summary>
        /// 字典解码测试函数3,测试用例为"d"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary3()
        {
            BEncoder.Decode("d");
        }

        /// <summary>
        /// 字典解码测试函数4,测试用例为"de0564adf"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary4()
        {
            BEncoder.Decode("de0564adf");
        }

        /// <summary>
        /// 字典解码测试函数5,测试用例为"d3:fooe"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary5()
        {
            BEncoder.Decode("d3:fooe");
        }

        /// <summary>
        /// 字典解码测试函数6,测试用例为"di1e0:e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary6()
        {
            BEncoder.Decode("di1e0:e");
        }

        /// <summary>
        /// 字典解码测试函数7,测试用例为"d0:1:ae"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary7()
        {
            BEncoder.Decode("d0:1:ae");
        }

        /// <summary>
        /// 字典解码测试函数8,测试用例为"d0:"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary8()
        {
            BEncoder.Decode("d0:");
        }

        /// <summary>
        /// 字典解码测试函数9,测试用例为"d01:x0:e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary9()
        {
           BEncoder.Decode("d01:x0:e");
        }
        #endregion

        #endregion

        #region Encode test

        [Test]
        public void TestEncodeHandler1()
        {
            FileStream sourceFile = File.OpenRead(@"D:\Bittorrent\winedt60.exe.torrent");
            byte[] source = new byte[sourceFile.Length];
            sourceFile.Read(source, 0, (int)sourceFile.Length);
            sourceFile.Close();
            DictNode dh = (DictNode)BEncoder.Decode(source);
            byte[] destion = BEncoder.ByteArrayEncode(dh);
            FileStream targetFile = File.OpenWrite(@"D:\Bittorrent\test.torrent");
            targetFile.Write(destion, 0, destion.Length);

            int i;
            for (i = 0; i < source.Length; i++)
            {
                Assert.AreEqual(source[i], destion[i]);
            }

            targetFile.Close();
        }

        /// <summary>
        /// 测试整数编码
        /// </summary>
        [Test]
        public void TestEncodeInteger1()
        {
            //Test1测试用例为4
            IntNode ih1 = new IntNode(4);
            string source1 = BEncoder.StringEncode(ih1);
            Assert.AreEqual(source1, "i4e");
            //Assert.AreEqual(ih1.OutputBufferSize, 3);

            //Test2测试用例为1234567890
            IntNode ih2 = new IntNode(1234567890);
            string source2 = BEncoder.StringEncode(ih2);
            Assert.AreEqual(source2, "i1234567890e");

            //Test3测试用例为0
            IntNode ih3 = new IntNode(0);
            string source3 = BEncoder.StringEncode(ih3);
            Assert.AreEqual(source3, "i0e");

            //Test4测试用例为-10
            IntNode ih4 = new IntNode(-10);
            string source4 = BEncoder.StringEncode(ih4);
            Assert.AreEqual(source4, "i-10e");
        }

        /// <summary>
        /// 测试字节数组编码
        /// </summary>
        [Test]
        public void TestEncodeByteArray1()
        {
            //Test1标点符号
            BytesNode bah1 = new BytesNode("~!@#$%^&*()_+|`-=\\{}:\"<>?[];',./");
            string source1 = BEncoder.StringEncode(bah1);
            Assert.AreEqual(source1, "32:~!@#$%^&*()_+|`-=\\{}:\"<>?[];',./");

            //Test2空字符
            BytesNode bah2 = new BytesNode("");
            string source2 = BEncoder.StringEncode(bah2);
            Assert.AreEqual(source2, "0:");

            //Test3英文字母与数字
            BytesNode bah3 = new BytesNode("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
            string source3 = BEncoder.StringEncode(bah3);
            Assert.AreEqual(source3, "62:abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");

            //Test4中文字体与全角标点符号
            BytesNode bah4 = new BytesNode("微软公司，广州大学");
            string source4 = BEncoder.StringEncode(bah4);
            Assert.AreEqual(source4, "27:微软公司，广州大学");

            //Test5全角的数字与英文字母
            BytesNode bah5 = new BytesNode("ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９");
            string source5 = BEncoder.StringEncode(bah5);
            Assert.AreEqual(source5, "186:ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９");
        }

        /// <summary>
        /// 测试列表编码
        /// </summary>
        [Test]
        public void TestEncodeList1()
        {
            //Test1
            ListNode list1 = new ListNode();
            string source1 = BEncoder.StringEncode(list1);
            Assert.AreEqual(source1, "le");

            //Test2
            ListNode list2 = new ListNode();
            for (int i = 1; i <= 3; i++)
                list2.Add(i);
            string source2 = BEncoder.StringEncode(list2);
            Assert.AreEqual(source2, "li1ei2ei3ee");

            //Test3
            ListNode lh31 = new ListNode();
            lh31.Add("Alice");
            lh31.Add("Bob");
            ListNode lh32 = new ListNode();
            lh32.Add(2);
            lh32.Add(3);
            ListNode list3 = new ListNode(new List<BEncodedNode>(new BEncodedNode[] { lh31, lh32 }));
            string source3 = BEncoder.StringEncode(list3);
            Assert.AreEqual(source3, "ll5:Alice3:Bobeli2ei3eee");
        }

        /// <summary>
        /// 测试字典编码
        /// </summary>
        [Test]
        public void TestEncodeDictionary1()
        {
            //Test1
            DictNode dict1 = new DictNode();
            string source1 = BEncoder.StringEncode(dict1);
            Assert.AreEqual(source1, "de");

            //Test2
            DictNode dict2 = new DictNode();
            dict2.Add("age", 25);
            dict2.Add("eyes", "blue");
            string source2 = BEncoder.StringEncode(dict2);
            Assert.AreEqual(source2, "d3:agei25e4:eyes4:bluee");
            

            //Test3
            DictNode dh31 = new DictNode();
            dh31.Add("author", "Alice");
            dh31.Add("length", 1048576);
            DictNode dict3 = new DictNode();
            dict3.Add("spam.mp3", dh31);
            string source3 = BEncoder.StringEncode(dict3);
            Assert.AreEqual(source3, "d8:spam.mp3d6:author5:Alice6:lengthi1048576eee");
            Assert.AreEqual(dict3.ToString(), "d8:spam.mp3d6:author5:Alice6:lengthi1048576eee");
        }
        #endregion
    }
}
