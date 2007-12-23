using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.RawServers;

namespace ZeraldotNet.TestLibBitTorrent.TestRawServer
{
    public class TestHelper
    {
        private DummyEncrypter handler;

        public DummyEncrypter Handler
        {
            get { return handler; }
            set { handler = value; }
        }
        private IRawServer rawServer;

        public IRawServer RawServer
        {
            get { return rawServer; }
            set { rawServer = value; }
        }
        private List<TaskDelegate> taskDelegates;

        public List<TaskDelegate> TaskDelegates
        {
            get { return taskDelegates; }
            set { taskDelegates = value; }
        }

        private void Raw()
        {
            rawServer.AddTask(taskDelegates[0], 0.1, string.Empty);
        }

        public void Loop(IRawServer rawServer)
        {
            this.rawServer = rawServer;
            this.taskDelegates = new List<TaskDelegate>();
            taskDelegates.Add(Raw);
            rawServer.AddTask(Raw, 0.1, string.Empty);
        }

        private void Go()
        {
            rawServer.ListenForever(handler);
            Debug.Write("listen forever completed");
        }

        public void sl(IRawServer rawServer, DummyEncrypter encrypter, int port)
        {
            rawServer.Bind(port, "127.0.0.1", false);
            this.handler = encrypter;
            Thread t = new Thread(Go);
            t.Start();
        }
    }
}
