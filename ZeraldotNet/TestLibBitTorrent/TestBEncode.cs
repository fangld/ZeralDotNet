﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.BEncoding;


namespace ZeraldotNet.TestLibBitTorrent
{
    [TestFixture]
    public class TestBEncode
    {
        #region 解码测试
        #region Handler解码测试
        [Test]
        public void TestDecodeHandler1()
        {
            byte[] source = File.ReadAllBytes("test_dummy.zip.torrent");
            DictionaryHandler dh = (DictionaryHandler)BEncode.Decode(source);
            Assert.AreEqual("http://tracker.bittorrent.com:6969/announce", (dh["announce"] as BytesHandler).StringValue);
            Assert.AreEqual("http://tracker.bittorrent.com:6969/announce", Encoding.Default.GetString((dh["announce"] as BytesHandler).ByteArrayValue));
        }

        /// <summary>
        /// Handler解码测试函数2,测试用例为""
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeHandler2()
        {
            //Test
            Handler bh = BEncode.Decode("");
        }

        /// <summary>
        /// Handler解码测试函数3,测试用例为"35208734823ljdahflajhdf"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeHandler3()
        {
            //Test
            Handler bh = BEncode.Decode("35208734823ljdahflajhdf");
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
            IntHandler ih1 = (IntHandler)BEncode.Decode("i10e");
            Assert.AreEqual(ih1.Value, 10);

            //Test2零
            IntHandler ih2 = (IntHandler)BEncode.Decode("i0e");
            Assert.AreEqual(ih2.Value, 0);

            //Test3负整数
            IntHandler ih3 = (IntHandler)BEncode.Decode("i-55e");
            Assert.AreEqual(ih3.Value, -55);

            //Test4所有的数字
            IntHandler ih4 = (IntHandler)BEncode.Decode("i1234567890e");
            Assert.AreEqual(ih4.Value, 1234567890);
        }

        /// <summary>
        /// 整数解码测试函数2,测试用例为"ie"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeInteger2()
        {
            //Test
            IntHandler ih = (IntHandler)BEncode.Decode("ie");
        }

        /// <summary>
        /// 整数解码测试函数3,测试用例为"i341foo382e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeInteger3()
        {
            //Test
            IntHandler ih = (IntHandler)BEncode.Decode("i341foo382e");
        }

        /// <summary>
        /// 整数解码测试函数4,测试用例为"index-0e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeInteger4()
        {
            //Test
            IntHandler ih = (IntHandler)BEncode.Decode("i-0e");
        }

        /// <summary>
        /// 整数解码测试函数5,测试用例为"i123"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeInteger5()
        {
            //Test
            IntHandler ih = (IntHandler)BEncode.Decode("i123");
        }

        /// <summary>
        /// 整数解码测试函数6,测试用例为"i0345e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeInteger6()
        {
            //Test
            IntHandler ih = (IntHandler)BEncode.Decode("i0345e");
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
            BytesHandler bah1 = (BytesHandler)BEncode.Decode("10:0123456789");
            Assert.AreEqual(bah1.ByteArrayValue, Encoding.Default.GetBytes("0123456789"));
            Assert.AreEqual(bah1.StringValue, "0123456789");

            //Test2
            BytesHandler bah2 = (BytesHandler)BEncode.Decode("26:abcdefghijklmnopqrstuvwxyz");
            Assert.AreEqual(bah2.ByteArrayValue, Encoding.Default.GetBytes("abcdefghijklmnopqrstuvwxyz"));
            Assert.AreEqual(bah2.StringValue, "abcdefghijklmnopqrstuvwxyz");

            //Test3
            BytesHandler bah3 = (BytesHandler)BEncode.Decode("124:ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９");
            Assert.AreEqual(bah3.ByteArrayValue, Encoding.Default.GetBytes("ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９"));
            Assert.AreEqual(bah3.StringValue, "ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９");

            //Test4
            BytesHandler bah4 = (BytesHandler)BEncode.Decode("0:");
            Assert.AreEqual(bah4.ByteArrayValue, Encoding.Default.GetBytes(string.Empty));
            Assert.AreEqual(bah4.StringValue, string.Empty);
        }

        /// <summary>
        /// 字节数组解码测试函数2,测试用例为"2:abcedefg"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeByteArray2()
        {
            //Test
            BytesHandler bah = (BytesHandler)BEncode.Decode("2:abcedefg");

        }

        /// <summary>
        /// 字节数组解码测试函数3,测试用例为"02:ab"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeByteArray3()
        {
            //Test
            BytesHandler bah = (BytesHandler)BEncode.Decode("02:ab");
        }

        /// <summary>
        /// 字节数组解码测试函数4,测试用例为"0:0:"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeByteArray4()
        {
            //Test
            BytesHandler bah = (BytesHandler)BEncode.Decode("0:0:");
        }

        /// <summary>
        /// 字节数组解码测试函数5,测试用例为"9:abc"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeByteArray5()
        {
            //Test
            BytesHandler bah = (BytesHandler)BEncode.Decode("9:abc");
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
            ListHandler lh1 = (ListHandler)BEncode.Decode("li0ei1ei2ee");
            Assert.AreEqual(((IntHandler)lh1[0]).Value, 0);
            Assert.AreEqual(((IntHandler)lh1[1]).Value, 1);
            Assert.AreEqual(((IntHandler)lh1[2]).Value, 2);

            //Test2字节数组
            ListHandler lh2 = (ListHandler)BEncode.Decode("l3:abc2:xye");
            Assert.AreEqual((lh2[0] as BytesHandler).ByteArrayValue, Encoding.Default.GetBytes("abc"));
            Assert.AreEqual((lh2[0] as BytesHandler).StringValue, "abc");

            Assert.AreEqual((lh2[1] as BytesHandler).ByteArrayValue, Encoding.Default.GetBytes("xy"));
            Assert.AreEqual((lh2[1] as BytesHandler).StringValue, "xy");

            //Test3空字节数组
            ListHandler lh3 = (ListHandler)BEncode.Decode("l0:0:0:e");
            Assert.AreEqual((lh3[0] as BytesHandler).ByteArrayValue, Encoding.Default.GetBytes(string.Empty));
            Assert.AreEqual((lh3[0] as BytesHandler).StringValue, string.Empty);

            Assert.AreEqual((lh3[1] as BytesHandler).ByteArrayValue, Encoding.Default.GetBytes(string.Empty));
            Assert.AreEqual((lh3[1] as BytesHandler).StringValue, string.Empty);

            Assert.AreEqual((lh3[2] as BytesHandler).ByteArrayValue, Encoding.Default.GetBytes(string.Empty));
            Assert.AreEqual((lh3[2] as BytesHandler).StringValue, string.Empty);

            //Test4字节数组与整数
            ListHandler lh4 = (ListHandler)BEncode.Decode("ll5:Alice3:Bobeli2ei3eee");
            ListHandler lHandler40 = (ListHandler)lh4[0];
            ListHandler lHandler41 = (ListHandler)lh4[1];

            Assert.AreEqual((lHandler40[0] as BytesHandler).ByteArrayValue, Encoding.Default.GetBytes("Alice"));
            Assert.AreEqual((lHandler40[0] as BytesHandler).StringValue, "Alice");

            Assert.AreEqual((lHandler40[1] as BytesHandler).ByteArrayValue, Encoding.Default.GetBytes("Bob"));
            Assert.AreEqual((lHandler40[1] as BytesHandler).StringValue, "Bob");

            Assert.AreEqual(((IntHandler)lHandler41[0]).Value, 2);

            Assert.AreEqual(((IntHandler)lHandler41[1]).Value, 3);

            //Test5空列表
            ListHandler lh5 = (ListHandler)BEncode.Decode("le");
            Assert.AreEqual(lh5.Count, 0);
        }

        /// <summary>
        /// 列表解码测试函数2,测试用例为"lezeral"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeList2()
        {
            //Test
            ListHandler lh = (ListHandler)BEncode.Decode("lezeral");
        }

        /// <summary>
        /// 列表解码测试函数3,测试用例为"l"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeList3()
        {
            //Test
            ListHandler lh = (ListHandler)BEncode.Decode("l");
        }

        /// <summary>
        /// 列表解码测试函数4,测试用例为"l0:"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeList4()
        {
            //Test
            ListHandler lh = (ListHandler)BEncode.Decode("l0:");
        }

        /// <summary>
        /// 列表解码测试函数5,测试用例为"l01:xe"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeList5()
        {
            //Test
            ListHandler lh = (ListHandler)BEncode.Decode("l01:xe");
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
            DictionaryHandler dh1 = (DictionaryHandler)BEncode.Decode("d3:agei25ee");
            Assert.AreEqual(((IntHandler)dh1["age"]).Value, 25);

            //Test2字节数组
            DictionaryHandler dh2 = (DictionaryHandler)BEncode.Decode("d3:agei25e5:color4:bluee");
            Assert.AreEqual(((IntHandler)dh2["age"]).Value, 25);

            Assert.AreEqual((dh2["color"] as BytesHandler).ByteArrayValue, Encoding.Default.GetBytes("blue"));
            Assert.AreEqual((dh2["color"] as BytesHandler).StringValue, "blue");

            //Test3字节数组与整数
            DictionaryHandler dh3 = (DictionaryHandler)BEncode.Decode("d8:spam.mp3d6:author5:Alice6:lengthi1048576eee");
            DictionaryHandler dHandler31 = (DictionaryHandler)dh3["spam.mp3"];
            Assert.AreEqual((dHandler31["author"] as BytesHandler).ByteArrayValue, Encoding.Default.GetBytes("Alice"));
            Assert.AreEqual((dHandler31["author"] as BytesHandler).StringValue, "Alice");
            Assert.AreEqual(((IntHandler)dHandler31["length"]).Value, 1048576);

            //Test4空字典
            DictionaryHandler dh4 = (DictionaryHandler)BEncode.Decode("de");
            Assert.AreEqual(dh4.Count, 0);
        }

        /// <summary>
        /// 字典解码测试函数2,测试用例为"d3:agei25e3:agei50ee"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary2()
        {
            //Test
            DictionaryHandler dh = (DictionaryHandler)BEncode.Decode("d3:agei25e3:agei50ee");
        }

        /// <summary>
        /// 字典解码测试函数3,测试用例为"d"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary3()
        {
            //Test
            DictionaryHandler dh = (DictionaryHandler)BEncode.Decode("d");
        }

        /// <summary>
        /// 字典解码测试函数4,测试用例为"de0564adf"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary4()
        {
            //Test
            DictionaryHandler dh = (DictionaryHandler)BEncode.Decode("de0564adf");
        }

        /// <summary>
        /// 字典解码测试函数5,测试用例为"d3:fooe"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary5()
        {
            //Test
            DictionaryHandler dh = (DictionaryHandler)BEncode.Decode("d3:fooe");
        }

        /// <summary>
        /// 字典解码测试函数6,测试用例为"di1e0:e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary6()
        {
            //Test
            DictionaryHandler dh = (DictionaryHandler)BEncode.Decode("di1e0:e");
        }

        /// <summary>
        /// 字典解码测试函数7,测试用例为"d1:b0:1:a0:e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary7()
        {
            //Test
            DictionaryHandler dh = (DictionaryHandler)BEncode.Decode("d0:1:ae");
        }

        /// <summary>
        /// 字典解码测试函数8,测试用例为"d0:"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary8()
        {
            //Test
            DictionaryHandler dh = (DictionaryHandler)BEncode.Decode("d0:");
        }

        /// <summary>
        /// 字典解码测试函数9,测试用例为"d01:x0:e"
        /// </summary>
        [Test]
        [ExpectedException(typeof(BitTorrentException))]
        public void TestDecodeDictionary9()
        {
            //Test
            DictionaryHandler dh = (DictionaryHandler)BEncode.Decode("d01:x0:e");
        }
        #endregion

        #endregion

        #region 编码测试

        [Test]
        public void TestEncodeHandler1()
        {
            FileStream sourceFile = File.OpenRead(@"test_dummy.zip.torrent");
            byte[] source = new byte[sourceFile.Length];
            sourceFile.Read(source, 0, (int)sourceFile.Length);
            sourceFile.Close();
            DictionaryHandler dh = (DictionaryHandler)BEncode.Decode(source);
            byte[] destion = BEncode.ByteArrayEncode(dh);
            FileStream targetFile = File.OpenWrite("i:\\test.torrent");
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
            IntHandler ih1 = new IntHandler(4);
            string source1 = BEncode.StringEncode(ih1);
            Assert.AreEqual(source1, "i4e");
            //Assert.AreEqual(ih1.OutputBufferSize, 3);

            //Test2测试用例为1234567890
            IntHandler ih2 = new IntHandler(1234567890);
            string source2 = BEncode.StringEncode(ih2);
            Assert.AreEqual(source2, "i1234567890e");

            //Test3测试用例为0
            IntHandler ih3 = new IntHandler(0);
            string source3 = BEncode.StringEncode(ih3);
            Assert.AreEqual(source3, "i0e");

            //Test4测试用例为-10
            IntHandler ih4 = new IntHandler(-10);
            string source4 = BEncode.StringEncode(ih4);
            Assert.AreEqual(source4, "i-10e");
        }

        /// <summary>
        /// 测试字节数组编码
        /// </summary>
        [Test]
        public void TestEncodeByteArray1()
        {
            //Test1标点符号
            BytesHandler bah1 = new BytesHandler("~!@#$%^&*()_+|`-=\\{}:\"<>?[];',./");
            string source1 = BEncode.StringEncode(bah1);
            Assert.AreEqual(source1, "32:~!@#$%^&*()_+|`-=\\{}:\"<>?[];',./");

            //Test2空字符
            BytesHandler bah2 = new BytesHandler("");
            string source2 = BEncode.StringEncode(bah2);
            Assert.AreEqual(source2, "0:");

            //Test3英文字母与数字
            BytesHandler bah3 = new BytesHandler("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");
            string source3 = BEncode.StringEncode(bah3);
            Assert.AreEqual(source3, "62:abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789");

            //Test4中文字体与全角标点符号
            BytesHandler bah4 = new BytesHandler("微软公司，广州大学");
            string source4 = BEncode.StringEncode(bah4);
            Assert.AreEqual(source4, "18:微软公司，广州大学");

            //Test5全角的数字与英文字母
            BytesHandler bah5 = new BytesHandler("ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９");
            string source5 = BEncode.StringEncode(bah5);
            Assert.AreEqual(source5, "124:ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ０１２３４５６７８９");
        }

        /// <summary>
        /// 测试列表编码
        /// </summary>
        [Test]
        public void TestEncodeList1()
        {
            //Test1
            ListHandler list1 = new ListHandler();
            string source1 = BEncode.StringEncode(list1);
            Assert.AreEqual(source1, "le");

            //Test2
            ListHandler list2 = new ListHandler();
            for (int i = 1; i <= 3; i++)
                list2.Add(i);
            string source2 = BEncode.StringEncode(list2);
            Assert.AreEqual(source2, "li1ei2ei3ee");

            //Test3
            ListHandler lh31 = new ListHandler();
            lh31.Add("Alice");
            lh31.Add("Bob");
            ListHandler lh32 = new ListHandler();
            lh32.Add(2);
            lh32.Add(3);
            ListHandler list3 = new ListHandler(new List<Handler>(new Handler[] { lh31, lh32 }));
            string source3 = BEncode.StringEncode(list3);
            Assert.AreEqual(source3, "ll5:Alice3:Bobeli2ei3eee");
        }

        /// <summary>
        /// 测试字典编码
        /// </summary>
        [Test]
        public void TestEncodeDictionary1()
        {
            //Test1
            DictionaryHandler dict1 = new DictionaryHandler();
            string source1 = BEncode.StringEncode(dict1);
            Assert.AreEqual(source1, "de");

            //Test2
            DictionaryHandler dict2 = new DictionaryHandler();
            dict2.Add("age", 25);
            dict2.Add("eyes", "blue");
            string source2 = BEncode.StringEncode(dict2);
            Assert.AreEqual(source2, "d3:agei25e4:eyes4:bluee");
            

            //Test3
            DictionaryHandler dh31 = new DictionaryHandler();
            dh31.Add(Encoding.Default.GetBytes("author"), "Alice");
            dh31.Add("length", 1048576);
            DictionaryHandler dict3 = new DictionaryHandler();
            dict3.Add("spam.mp3", dh31);
            string source3 = BEncode.StringEncode(dict3);
            Assert.AreEqual(source3, "d8:spam.mp3d6:author5:Alice6:lengthi1048576eee");
            Assert.AreEqual(dict3.ToString(), "d8:spam.mp3d6:author5:Alice6:lengthi1048576eee");
        }
        #endregion
    }
}