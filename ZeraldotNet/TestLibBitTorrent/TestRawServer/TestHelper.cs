using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.RawServers;
using ZeraldotNet.LibBitTorrent;
using System.Diagnostics;
using System.Threading;

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
            rawServer.AddTask(taskDelegates[0], 0.1);
        }

        public void Loop(IRawServer rawServer)
        {
            this.rawServer = rawServer;
            this.taskDelegates = new List<TaskDelegate>();
            taskDelegates.Add(new TaskDelegate(Raw));
            rawServer.AddTask(new TaskDelegate(Raw), 0.1);
        }

        private void Go()
        {
            rawServer.ListenForever(handler);
            Debug.Write("listen forever completed");
        }

        public void sl(IRawServer rawServer, DummyEncrypter handler, int port)
        {
            rawServer.Bind(port, "127.0.0.1", false);
            this.handler = handler;
            Thread t = new Thread(new ThreadStart(Go));
            t.Start();
        }
    }
}
