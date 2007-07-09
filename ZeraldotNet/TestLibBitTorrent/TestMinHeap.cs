using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent.DataStructures;

namespace ZeraldotNet.TestLibBitTorrent
{
    [TestFixture]
    public class TestMinHeap
    {
        [Test]
        public void Test1()
        {
            MinHeap<int> test = new MinHeap<int>();
            test.Add(10);
            Assert.AreEqual(1,test.Count);
            test.Peek();
            Assert.AreEqual(1, test.Count);
            test.ExtractFirst();
            Assert.AreEqual(0, test.Count);
        }

        [Test]
        public void Test2()
        {
            MinHeap<int> test = new MinHeap<int>();
            int i;
            for (i = 0; i < 10; i++)
            {
                test.Add(i);
            }

            Assert.AreEqual(10, test.Count);

            for (i = 0; i < 10; i++)
            {
                Assert.AreEqual(i, test.ExtractFirst());
                Assert.AreEqual(10 - i - 1, test.Count);
            }
        }

        [Test]
        public void Test3()
        {
            MinHeap<int> test = new MinHeap<int>();
            int i;
            for (i = 9; i >= 0; i--)
            {
                test.Add(i);
            }

            Assert.AreEqual(10, test.Count);

            for (i = 0; i < 10; i++)
            {
                Assert.AreEqual(i, test.ExtractFirst());
            }
        }
    }
}
