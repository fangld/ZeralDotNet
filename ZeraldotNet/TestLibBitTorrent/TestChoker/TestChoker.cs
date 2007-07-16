using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Chokers;
using NUnit.Framework;

namespace ZeraldotNet.TestLibBitTorrent.TestChoker
{
    [TestFixture]
    public class TestChoker
    {
        /// <summary>
        /// Round robin with no downloads
        /// </summary>
        [Test]
        public void Test1()
        {
            DummyScheduler scheduler = new DummyScheduler();
            new Choker(2, new SchedulerDelegate(scheduler.Call), new Flag());
            Assert.AreEqual(1, scheduler.FunctionCount);
            Assert.AreEqual(10.0, scheduler.GetDelay(0));
            TaskDelegate function = scheduler.GetFunction(0);
            function();

            scheduler.RemoveAt(0);
            Assert.AreEqual(1, scheduler.FunctionCount);
            Assert.AreEqual(10.0, scheduler.GetDelay(0));
            function = scheduler.GetFunction(0);
            function();

            scheduler.RemoveAt(0);
            function = scheduler.GetFunction(0);
            function();

            scheduler.RemoveAt(0);
            function = scheduler.GetFunction(0);
            function();

            scheduler.RemoveAt(0);
            function = scheduler.GetFunction(0);
        }

        /// <summary>
        /// Resort
        /// </summary>
        [Test]
        public void Test2()
        {
            DummyScheduler scheduler = new DummyScheduler();
            Choker choker = new Choker(1, new SchedulerDelegate(scheduler.Call), new Flag());
            DummyConnection connection1 = new DummyConnection(0);
            DummyConnection connection2 = new DummyConnection(1);
            DummyConnection connection3 = new DummyConnection(2);
            DummyConnection connection4 = new DummyConnection(3);
            connection2.Upload.Interested = true;
            connection3.Upload.Interested = true;

            choker.MakeConnection(connection1);
            Assert.AreEqual(false, connection1.Upload.Choked);

            choker.MakeConnection(connection2, 1);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(false, connection2.Upload.Choked);

            choker.MakeConnection(connection3, 1);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(false, connection3.Upload.Choked);

            connection2.Value = 2;
            connection3.Value = 1;

            choker.MakeConnection(connection4, 1);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(false, connection3.Upload.Choked);
            Assert.AreEqual(false, connection4.Upload.Choked);

            choker.CloseConnection(connection4);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(false, connection3.Upload.Choked);

            scheduler.GetFunction(0)();
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(false, connection3.Upload.Choked);
        }

        /// <summary>
        /// interest
        /// </summary>
        [Test]
        public void Test3()
        {
            DummyScheduler scheduler = new DummyScheduler();
            Choker choker = new Choker(1, new SchedulerDelegate(scheduler.Call), new Flag());
            DummyConnection connection1 = new DummyConnection(0);
            DummyConnection connection2 = new DummyConnection(1);
            DummyConnection connection3 = new DummyConnection(2);

            connection2.Upload.Interested = true;
            connection3.Upload.Interested = true;

            choker.MakeConnection(connection1);
            Assert.AreEqual(false, connection1.Upload.Choked);

            choker.MakeConnection(connection2, 1);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(false, connection2.Upload.Choked);

            choker.MakeConnection(connection3, 1);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(false, connection3.Upload.Choked);

            connection3.Upload.Interested = false;
            choker.NotInterested(connection3);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(false, connection2.Upload.Choked);
            Assert.AreEqual(false, connection3.Upload.Choked);

            connection3.Upload.Interested = true;
            choker.Interested(connection3);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(false, connection3.Upload.Choked);

            choker.CloseConnection(connection3);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(false, connection2.Upload.Choked);
        }

        /// <summary>
        /// Robin interest
        /// </summary>
        [Test]
        public void Test4()
        {
            DummyScheduler scheduler = new DummyScheduler();
            Choker choker = new Choker(1, new SchedulerDelegate(scheduler.Call), new Flag());
            DummyConnection connection1 = new DummyConnection(0);
            DummyConnection connection2 = new DummyConnection(1);
            connection1.Upload.Interested = true;

            choker.MakeConnection(connection2);
            Assert.AreEqual(false, connection2.Upload.Choked);

            choker.MakeConnection(connection1, 0);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);

            connection1.Upload.Interested = false;
            choker.NotInterested(connection1);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(false, connection2.Upload.Choked);

            connection1.Upload.Interested = true;
            choker.NotInterested(connection1);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);

            choker.CloseConnection(connection1);
            Assert.AreEqual(false, connection2.Upload.Choked);
        }

        /// <summary>
        /// Skip not interested
        /// </summary>
        [Test]
        public void Test5()
        {
            DummyScheduler scheduler = new DummyScheduler();
            Choker choker = new Choker(1, new SchedulerDelegate(scheduler.Call), new Flag());
            DummyConnection connection1 = new DummyConnection(0);
            DummyConnection connection2 = new DummyConnection(1);
            DummyConnection connection3 = new DummyConnection(2);

            connection1.Upload.Interested = true;
            connection3.Upload.Interested = true;
            choker.MakeConnection(connection2);
            Assert.AreEqual(false, connection2.Upload.Choked);

            choker.MakeConnection(connection1, 0);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);

            choker.MakeConnection(connection3, 2);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(true, connection3.Upload.Choked);

            TaskDelegate function = scheduler.GetFunction(0);
            function();
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(true, connection3.Upload.Choked);

            function();
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(true, connection3.Upload.Choked);

            function();
            Assert.AreEqual(true, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(false, connection3.Upload.Choked);
        }

        /// <summary>
        /// Close connection without interrupt
        /// </summary>
        [Test]
        public void Test6()
        {
            DummyScheduler scheduler = new DummyScheduler();
            Choker choker = new Choker(1, new SchedulerDelegate(scheduler.Call), new Flag());
            DummyConnection connection1 = new DummyConnection(0);
            DummyConnection connection2 = new DummyConnection(1);
            DummyConnection connection3 = new DummyConnection(2);

            connection1.Upload.Interested = true;
            connection2.Upload.Interested = true;
            connection3.Upload.Interested = true;
            choker.MakeConnection(connection1);
            choker.MakeConnection(connection2, 1);
            choker.MakeConnection(connection3, 2);

            TaskDelegate function = scheduler.GetFunction(0);
            function();
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(true, connection3.Upload.Choked);

            function();
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(true, connection3.Upload.Choked);

            function();
            Assert.AreEqual(true, connection1.Upload.Choked);
            Assert.AreEqual(false, connection2.Upload.Choked);
            Assert.AreEqual(true, connection3.Upload.Choked);

            function();
            Assert.AreEqual(true, connection1.Upload.Choked);
            Assert.AreEqual(false, connection2.Upload.Choked);
            Assert.AreEqual(true, connection3.Upload.Choked);

            function();
            Assert.AreEqual(true, connection1.Upload.Choked);
            Assert.AreEqual(false, connection2.Upload.Choked);
            Assert.AreEqual(true, connection3.Upload.Choked);

            choker.CloseConnection(connection3);
            Assert.AreEqual(true, connection1.Upload.Choked);
            Assert.AreEqual(false, connection2.Upload.Choked);

            function();
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);

            choker.CloseConnection(connection2);
            Assert.AreEqual(false, connection1.Upload.Choked);
        }

        /// <summary>
        /// Make connection without interrupt
        /// </summary>
        [Test]
        public void Test7()
        {
            DummyScheduler scheduler = new DummyScheduler();
            Choker choker = new Choker(1, new SchedulerDelegate(scheduler.Call), new Flag());
            DummyConnection connection1 = new DummyConnection(0);
            DummyConnection connection2 = new DummyConnection(1);
            DummyConnection connection3 = new DummyConnection(2);

            connection1.Upload.Interested = true;
            connection2.Upload.Interested = true;
            connection3.Upload.Interested = true;
            choker.MakeConnection(connection1);
            choker.MakeConnection(connection2, 1);

            TaskDelegate function = scheduler.GetFunction(0);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);

            function();
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);

            function();
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);

            choker.MakeConnection(connection3, 1);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(true, connection3.Upload.Choked);

            function();
            Assert.AreEqual(true, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
            Assert.AreEqual(false, connection3.Upload.Choked);
        }

        /// <summary>
        /// Round robin
        /// </summary>
        [Test]
        public void Test8()
        {
            DummyScheduler scheduler = new DummyScheduler();
            Choker choker = new Choker(1, new SchedulerDelegate(scheduler.Call), new Flag());
            DummyConnection connection1 = new DummyConnection(0);
            DummyConnection connection2 = new DummyConnection(1);

            connection1.Upload.Interested = true;
            connection2.Upload.Interested = true;
            choker.MakeConnection(connection1);
            choker.MakeConnection(connection2, 1);

            TaskDelegate function = scheduler.GetFunction(0);
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);

            function();
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);

            function();
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);

            function();
            Assert.AreEqual(true, connection1.Upload.Choked);
            Assert.AreEqual(false, connection2.Upload.Choked);

            function();
            Assert.AreEqual(true, connection1.Upload.Choked);
            Assert.AreEqual(false, connection2.Upload.Choked);

            function();
            Assert.AreEqual(true, connection1.Upload.Choked);
            Assert.AreEqual(false, connection2.Upload.Choked);

            function();
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(true, connection2.Upload.Choked);
        }

        /// <summary>
        /// Multi
        /// </summary>
        [Test]
        public void Test9()
        {
            DummyScheduler scheduler = new DummyScheduler();
            Choker choker = new Choker(4, new SchedulerDelegate(scheduler.Call), new Flag());
            DummyConnection connection1 = new DummyConnection(0);
            DummyConnection connection2 = new DummyConnection(0);
            DummyConnection connection3 = new DummyConnection(0);
            DummyConnection connection4 = new DummyConnection(8);
            DummyConnection connection5 = new DummyConnection(0);
            DummyConnection connection6 = new DummyConnection(0);
            DummyConnection connection7 = new DummyConnection(6);
            DummyConnection connection8 = new DummyConnection(0);
            DummyConnection connection9 = new DummyConnection(9);
            DummyConnection connection10 = new DummyConnection(7);
            DummyConnection connection11 = new DummyConnection(10);

            choker.MakeConnection(connection1, 0);
            choker.MakeConnection(connection2, 1);
            choker.MakeConnection(connection3, 2);
            choker.MakeConnection(connection4, 3);
            choker.MakeConnection(connection5, 4);
            choker.MakeConnection(connection6, 5);
            choker.MakeConnection(connection7, 6);
            choker.MakeConnection(connection8, 7);
            choker.MakeConnection(connection9, 8);
            choker.MakeConnection(connection10, 9);
            choker.MakeConnection(connection11, 10);

            connection2.Upload.Interested = true;
            connection4.Upload.Interested = true;
            connection6.Upload.Interested = true;
            connection8.Upload.Interested = true;
            connection10.Upload.Interested = true;

            connection2.Download.Snubbed = true;
            connection6.Download.Snubbed = true;
            connection8.Download.Snubbed = true;

            scheduler.GetFunction(0)();
            Assert.AreEqual(false, connection1.Upload.Choked);
            Assert.AreEqual(false, connection2.Upload.Choked);
            Assert.AreEqual(false, connection3.Upload.Choked);
            Assert.AreEqual(false, connection4.Upload.Choked);
            Assert.AreEqual(false, connection5.Upload.Choked);
            Assert.AreEqual(false, connection6.Upload.Choked); 
            Assert.AreEqual(true, connection7.Upload.Choked);
            Assert.AreEqual(true, connection8.Upload.Choked); 
            Assert.AreEqual(true, connection9.Upload.Choked);
            Assert.AreEqual(false, connection10.Upload.Choked);
            Assert.AreEqual(true, connection11.Upload.Choked);
        }

    }
}
