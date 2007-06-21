using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent;

namespace ZeraldotNet.TestLibBitTorrent
{
    [TestFixture]
    public class TestEncrypter
    {
        private const string ProtocolName = "BitTorrent protocol";

        private static ArrayList log;

        public static void ScheduleFunction(TaskDelegate taskFunction, double value)
        {
            log.Add(new object[] { taskFunction, value });
        }

        [Test]
        public void TestMessagesInAndOut()
        {
            //Connecter connector = new Connecter(
        }
    }
}
