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
    public class TestPiecePicker
    {
        private List<int> request;

        //判断request是否已经包含了第index个片断
        private bool Want(int index)
        {
            return !request.Contains(index);
        }

        /// <summary>
        /// 发送请求信息的队列
        /// </summary>
        /// <param name="pp">PiecePicker片断选择类</param>
        /// <returns>返回发送请求信息的队列</returns>
        private List<int> Pull(PiecePicker pp)
        {
            int selectIndex;
            request = new List<int>();
            while (true)
            {
                //调用PiecePicker选择片断
                selectIndex = pp.Next(new WantDelegate(Want));

                //当selectIndex =＝ -1时，表示已经没有排片断可供选择，所以结束选择
                if (selectIndex == -1)
                    break;

                //将所选择的片断索引号加入到request中
                request.Add(selectIndex);
            }
            return request;
        }

        /// <summary>
        /// 发送请求信息
        /// </summary>
        [Test]
        public void TestPiecePicker1()
        {
            PiecePicker piecePicker1 = new PiecePicker(8);

            //收到第0,2,4和6个片断的have信息
            piecePicker1.GotHave(0);
            piecePicker1.GotHave(2);
            piecePicker1.GotHave(4);
            piecePicker1.GotHave(6);

            //发送第0,1,3,6个片断的请求信息
            piecePicker1.Requested(1);
            piecePicker1.Requested(1);
            piecePicker1.Requested(3);
            piecePicker1.Requested(0);
            piecePicker1.Requested(6);
            List<int> pull1 = Pull(piecePicker1);

            //因为第5和7个片断没有发送request信息，并且没有收到have信息，所以请求队列长度为6
            Assert.AreEqual(6, pull1.Count);

            //严格优先选择策略
            Assert.AreEqual(1, pull1[0]);
            Assert.AreEqual(3, pull1[1]);
            Assert.AreEqual(0, pull1[2]);
            Assert.AreEqual(6, pull1[3]);
            Assert.AreEqual(true, (pull1[4] == 2 && pull1[5] == 4) || (pull1[4] == 4 && pull1[5] == 2));
        }

        /// <summary>
        /// 改变选择片断1
        /// </summary>
        [Test]
        public void TestPiecePicker2()
        {
            PiecePicker piecePicker1 = new PiecePicker(8);
            
            //收到第0,2,4和6个片断的have信息
            piecePicker1.GotHave(0);
            piecePicker1.GotHave(2);
            piecePicker1.GotHave(4);
            piecePicker1.GotHave(6);
            
            //丢失第2和6个片断的have信息
            piecePicker1.LostHave(2);
            piecePicker1.LostHave(6);
            List<int> pull1 = Pull(piecePicker1);
            Assert.AreEqual(2, pull1.Count);
            Assert.AreEqual(true, (pull1[0] == 0 && pull1[1] == 4) || (pull1[0] == 4 && pull1[1] == 0));
        }

        /// <summary>
        /// 改变选择片断2
        /// </summary>
        [Test]
        public void TestPiecePicker3()
        {
            PiecePicker piecePicker1 = new PiecePicker(9);

            //完成了第8个片断
            piecePicker1.Complete(8);

            //收到第0,2,4和6个片断的have信息
            piecePicker1.GotHave(0);
            piecePicker1.GotHave(2);
            piecePicker1.GotHave(4);
            piecePicker1.GotHave(6);

            //丢失第2和6个片断的have信息
            piecePicker1.LostHave(2);
            piecePicker1.LostHave(6);
            List<int> pull1 = Pull(piecePicker1);
            Assert.AreEqual(2, pull1.Count);
            Assert.AreEqual(true, (pull1[0] == 0 && pull1[1] == 4) || (pull1[0] == 4 && pull1[1] == 0));
        }

        /// <summary>
        /// 最小优先选择策略
        /// </summary>
        [Test]
        public void TestPiecePicker4()
        {
            PiecePicker piecePicker1 = new PiecePicker(3);
            
            //完成了第2个片断
            piecePicker1.Complete(2);

            //发送第1个片断的请求信息
            piecePicker1.Requested(0);

            //收到第0和1个片断的have信息
            piecePicker1.GotHave(1);
            piecePicker1.GotHave(0);
            piecePicker1.GotHave(0);
            List<int> pull1 = Pull(piecePicker1);
            Assert.AreEqual(2, pull1.Count);
            Assert.AreEqual(1, pull1[0]);
            Assert.AreEqual(0, pull1[1]);
        }

        /// <summary>
        /// 严格优先选择策略
        /// </summary>
        [Test]
        public void TestPiecePicker5()
        {
            PiecePicker piecePicker1 = new PiecePicker(3);

            //完成了第2个片断
            piecePicker1.Complete(2);

            //发送第0和1个片断的请求信息
            piecePicker1.Requested(0);
            piecePicker1.Requested(1);

            //收到第0和1个片断的have信息
            piecePicker1.GotHave(1);
            piecePicker1.GotHave(0);
            piecePicker1.GotHave(0);
            List<int> pull1 = Pull(piecePicker1);
            Assert.AreEqual(2, pull1.Count);
            Assert.AreEqual(1, pull1[0]);
            Assert.AreEqual(0, pull1[1]);
        }

        /// <summary>
        /// 0个片断
        /// </summary>
        [Test]
        public void TestPiecePicker6()
        {
            PiecePicker piecePicker1 = new PiecePicker(0);
            List<int> pull1 = Pull(piecePicker1);
            Assert.AreEqual(0, pull1.Count);
        }
    }
}