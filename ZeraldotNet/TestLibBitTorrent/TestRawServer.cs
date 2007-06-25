using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ZeraldotNet.LibBitTorrent;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace ZeraldotNet.TestLibBitTorrent.TestRawServer
{
    public class DummySocket
    {
        DummyRawServer rawServer;

        private Socket socket;

        public Socket Socket
        {
            get { return socket; }
            set { socket = value; }
        }

        private DateTime lastHit;

        public DateTime LastHit
        {
            get { return lastHit; }
            set { lastHit = value; }
        }

        private bool isConnected;

        public bool IsConnected
        {
            get { return isConnected; }
            set { isConnected = value; }
        }

        private List<byte[]> buffer;

        private DummyHandler handler;

        public DummyHandler Handler
        {
            get { return handler; }
            set { handler = value; }
        }

        public DummySocket(DummyRawServer rawServer, Socket socket, DummyHandler handler)
        {
            this.rawServer = rawServer;
            this.socket = socket;
            this.handler = handler;
            buffer = new List<byte[]>();
            lastHit = DateTime.Now;
            isConnected = false;
        }

        public string IP
        {
            get
            {
                try
                {
                    return ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
                }
                catch (SocketException)
                {
                    return "无连接";
                }
            }
        }

        public void Close()
        {
            Close(false);
        }

        public void Close(bool closing)
        {
            Socket tempSocket = this.socket;
            this.socket = null;

            //新建缓冲区
            buffer = new List<byte[]>();
            if (!closing)
            {
                rawServer.RemoveSingleSockets(tempSocket.Handle);
            }
            rawServer.Poll.Unregister(tempSocket);

            //关闭套接字
            tempSocket.Close();
        }

        public void shutdown(SocketShutdown how)
        {
            socket.Shutdown(how);
        }

        public bool IsFlushed()
        {
            return buffer.Count == 0;
        }

        //TODO: What type should bytes be?
        public void Write(byte[] bytes)
        {
            //将数据写入缓冲区
            buffer.Add(bytes);

            //如果缓冲区的数量为1，则发送数据
            if (buffer.Count == 1)
            {
                TryWrite();
            }
        }

        public void TryWrite()
        {
            //如果已经连上
            if (isConnected)
            {
                try
                {
                    int amount;
                    int bytesLength;
                    //当缓冲区的数量 > 0，则继续发送数据
                    while (buffer.Count > 0)
                    {
                        byte[] bytes = buffer[0];
                        bytesLength = bytes.Length;
                        amount = socket.Send(bytes);
                        if (amount != bytesLength)
                        {
                            //如果已经发送了一些数据
                            if (amount != 0)
                            {
                                byte[] anotherBuffer = new byte[bytesLength - amount];
                                Globals.CopyBytes(bytes, amount, anotherBuffer);
                                buffer[0] = anotherBuffer;
                            }
                            break;
                        }
                        buffer.RemoveAt(0);
                    }
                }

                //捕获Socket异常
                catch (SocketException sockEx)
                {
                    //如果异常代码为10035，则添加为死连接。
                    if (sockEx.ErrorCode != 10035) //WSAE would block
                    {
                        rawServer.AddToDeadFromWrite(this);
                        return;
                    }
                }

                //如果缓冲区的数量为0，则注册服务器为可读
                if (buffer.Count == 0)
                {
                    rawServer.Poll.Register(socket, PollMode.PollIn);
                }

                //否则注册服务器为可写
                else
                {
                    rawServer.Poll.Register(socket, PollMode.PollOut);
                }
            }
        }
    }

    public class DummyRawServer
    {
        double timeoutCheckInterval;
        double timeout;
        Flag doneFlag;
        bool noisy;
        List<Task> tasks;
        List<ExternalTask> externalTasks;
        public List<DummySocket> deadFromWrite;
        public Dictionary<IntPtr,DummySocket> singleSockets;

        private Poll poll;

        public Poll Poll
        {
            get { return poll; }
            set { poll = value; }
        }
        Socket server;
        private DummyHandler handler;

        public DummyHandler Handler
        {
            get { return handler; }
            set { handler = value; }
        }

        public DummyRawServer(Flag doneFlag, double timeoutCheckInterval, double timeout, bool noisy)
        {
            this.timeoutCheckInterval = timeoutCheckInterval;
            this.timeout = timeout;
            this.poll = new Poll();
            singleSockets = new Dictionary<IntPtr, DummySocket>();
            deadFromWrite = new List<DummySocket>();
            this.doneFlag = doneFlag;
            this.noisy = noisy;
            tasks = new List<Task>();
            externalTasks = new List<ExternalTask>();
            this.AddTask(new TaskDelegate(ScanForTimeouts), timeoutCheckInterval);
        }

        public void RemoveSingleSockets(IntPtr key)
        {
            this.singleSockets.Remove(key);
        }

        public void AddToDeadFromWrite(DummySocket item)
        {
            deadFromWrite.Add(item);
        }

        public void AddTask(TaskDelegate taskFunction, double delay)
        {
            Debug.WriteLine(delay);
            lock (this)
            {
                tasks.Add(new Task(taskFunction, DateTime.Now.AddSeconds(delay)));
                tasks.Sort();
            }
        }

        public void AddExternalTask(TaskDelegate taskFunction, double delay)
        {
            lock (this)
            {
                externalTasks.Add(new ExternalTask(taskFunction, delay, "Add task"));
            }
        }

        public void ScanForTimeouts()
        {
            AddTask(new TaskDelegate(ScanForTimeouts), timeoutCheckInterval);
            DateTime t = DateTime.Now.AddSeconds(-timeout);
            List<DummySocket> toKill = new List<DummySocket>();
            foreach (DummySocket item in singleSockets.Values)
            {
                if (item.LastHit < t)
                    toKill.Add(item);
            }
            foreach (DummySocket killSocket in toKill)
            {
                if (killSocket.Socket != null)
                {
                    CloseSocket(killSocket);
                }
            }
        }

        public void Bind(int port, string bind, bool reuse)
        {
            Socket newServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            if (reuse)
            {
                newServer.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
            }
            newServer.Blocking = false;
            newServer.Bind(new IPEndPoint(IPAddress.Parse(bind), port));
            newServer.Listen(5);
            poll.Register(newServer, PollMode.PollIn);
            this.server = newServer;
        }

        public DummySocket StartConnection(IPEndPoint dns, DummyHandler handler)
        {
            if (handler == null)
                handler = this.handler;
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Blocking = false;
            try { sock.Connect(dns); }
            catch (Exception ex)
            {
                int i = 0;
            }
            poll.Register(sock, PollMode.PollIn);
            DummySocket s = new DummySocket(this, sock, handler);
            singleSockets[sock.Handle] = s;
            return s;
        }

        public void HandleEvents(List<PollItem> events)
        {
            foreach (PollItem pi in events)
            {
                if (pi.Socket.Handle == server.Handle)
                {
                    if ((pi.Mode & (PollMode.PollError | PollMode.PollHangUp)) != 0)
                    {
                        poll.Unregister(server);
                        server.Close();
                        //TODO: print "lost server socket"
                    }
                    else
                    {
                        Socket newsock = server.Accept();
                        newsock.Blocking = false;
                        DummySocket nss = new DummySocket(this, newsock, handler);
                        singleSockets[newsock.Handle] = nss;
                        poll.Register(newsock, PollMode.PollIn);
                        handler.MakeExternalConnection(nss);
                    }
                }
                else
                {
                    DummySocket s = (DummySocket)singleSockets[pi.Socket.Handle];
                    if (s == null)
                        continue;
                    s.IsConnected = true;
                    if ((pi.Mode & (PollMode.PollError | PollMode.PollHangUp)) != 0)
                    {
                        CloseSocket(s);
                        continue;
                    }
                    if ((pi.Mode & PollMode.PollIn) != 0)
                    {
                        try
                        {
                            s.LastHit = DateTime.Now;
                            int available = s.Socket.Available;
                            if (available == 0)
                                CloseSocket(s);
                            else
                            {
                                byte[] data = new byte[available];
                                s.Socket.Receive(data, 0, available, SocketFlags.None);
                                s.Handler.DataCameIn(s, data);
                            }
                        }
                        catch (SocketException ex)
                        {
                            if (ex.ErrorCode != 10035) // WSAEWOULDBLOCK
                            {
                                CloseSocket(s);
                                continue;
                            }

                        }
                    }
                    if (((pi.Mode & PollMode.PollOut) != 0) && (s.Socket != null) && (!s.IsFlushed()))
                    {
                        s.TryWrite();
                        if (s.IsFlushed())
                            s.Handler.FlushConnection(s);
                    }
                }
            }
        }

        public void PopExternal()
        {
            foreach (ExternalTask task in externalTasks)
            {
                AddTask(task.TaskFunction, task.Delay);
            }
            externalTasks.Clear();
        }

        public void ListenForever(DummyHandler handler)
        {
            this.handler = handler;
            try
            {
                while (!doneFlag.IsSet)
                {
                    try
                    {
                        double period;
                        PopExternal();
                        if (tasks.Count == 0)
                            period = 2 ^ 30;
                        else
                        {
                            Task t = (Task)tasks[0];
                            period = (t.When - DateTime.Now).TotalSeconds;
                            if (period < 0) period = 0;
                        }
                        List<PollItem> events = poll.Select((int)(period * 1000000));
                        if (doneFlag.IsSet)
                            return;
                        while (tasks.Count > 0 && tasks[0].When <= DateTime.Now)
                        {
                            Task t = tasks[0];
                            tasks.RemoveAt(0);
                            try
                            {
                                t.TaskFunction();
                            }
                            //TODO: except KeyboardInterrupt:
                            // print_exc()
                            // return
                            catch (Exception ex)
                            {
                                if (noisy)
                                { ;} //TODO: print_exc()
                            }
                        }
                        CloseDead();
                        HandleEvents(events);
                        if (doneFlag.IsSet)
                            return;
                        CloseDead();
                    }
                    //TODO:
                    //except error:
                    //    if self.doneflag.isSet():
                    //        return
                    //except KeyboardInterrupt:
                    //    print_exc()
                    //    return
                    catch (Exception ex)
                    {
                        int i = 0;
                        //TODO: print_exc()
                    }
                }
            }
            finally
            {
                foreach (DummySocket ss in singleSockets.Values)
                {
                    ss.Close(true);
                }
                server.Close();
            }
        }

        private void CloseDead()
        {
            while (deadFromWrite.Count > 0)
            {
                List<DummySocket> old = deadFromWrite;
                deadFromWrite = new List<DummySocket>();
                foreach (DummySocket s in old)
                    if (s.Socket != null)
                        CloseSocket(s);
            }
        }

        private void CloseSocket(DummySocket s)
        {
            poll.Unregister(s.Socket);
            singleSockets.Remove(s.Socket.Handle);
            s.Socket.Close();
            s.Socket = null;
            s.Handler.LoseConnection(s);
        }
    }

    public class DummyHandler
    {
        public List<DummySocket> externalDummySockets;
        public ArrayList data_in;
        public List<DummySocket> lostDummySockets;

        public DummyHandler()
        {
            externalDummySockets = new List<DummySocket>();
            data_in = new ArrayList();
            lostDummySockets = new List<DummySocket>();
        }

        public void MakeExternalConnection(DummySocket s)
        {
            externalDummySockets.Add(s);
        }

        public void DataCameIn(DummySocket s, byte[] data)
        {
            data_in.Add(new object[] { s, data });
        }

        public void LoseConnection(DummySocket s)
        {
            lostDummySockets.Add(s);
        }

        public void FlushConnection(DummySocket s)
        {
        }
    }

    public class TestHelper
    {
        private DummyHandler handler;

        public DummyHandler Handler
        {
            get { return handler; }
            set { handler = value; }
        }
        private DummyRawServer rawServer;

        public DummyRawServer RawServer
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

        public void Loop(DummyRawServer rawServer)
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

        public void sl(DummyRawServer rawServer, DummyHandler handler, int port)
        {
            rawServer.Bind(port, "127.0.0.1", false);
            this.handler = handler;
            Thread t = new Thread(new ThreadStart(Go));
            t.Start();
        }
    }

    [TestFixture]
    public class TestRawServer
    {
        /// <summary>
        /// Starting side close
        /// </summary>
        [Test]
        public void Test1()
        {
            Flag fa = new Flag();
            Flag fb = new Flag();
            try
            {
                DummyHandler da = new DummyHandler();
                DummyRawServer sa = new DummyRawServer(fa, 100, 100, false);
                TestHelper ta = new TestHelper();
                ta.Loop(sa);
                ta.sl(sa, da, 6800);
                DummyHandler db = new DummyHandler();
                DummyRawServer sb = new DummyRawServer(fb, 100, 100, false);
                TestHelper tb = new TestHelper();
                tb.Loop(sb);
                tb.sl(sb, db, 6801);

                Thread.Sleep(500);
                DummySocket ca = sa.StartConnection(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6801), null);
                Thread.Sleep(5000);

                Assert.AreEqual(0, da.externalDummySockets.Count);
                Assert.AreEqual(0, da.data_in.Count);
                Assert.AreEqual(0, da.lostDummySockets.Count);
                Assert.AreEqual(1, db.externalDummySockets.Count);
                DummySocket cb = (DummySocket)db.externalDummySockets[0];
                db.externalDummySockets.Clear();
                Assert.AreEqual(0, db.data_in.Count);
                Assert.AreEqual(0, db.lostDummySockets.Count);

                ca.Write(new byte[] { (byte)'a', (byte)'a', (byte)'a' });
                cb.Write(new byte[] { (byte)'b', (byte)'b', (byte)'b' });
                Thread.Sleep(10000);

                Assert.AreEqual(0, da.externalDummySockets.Count);
                //da.data_in = ca, 'bbb'
                da.data_in.Clear();
                Assert.AreEqual(0, da.lostDummySockets.Count);
                Assert.AreEqual(0, db.externalDummySockets.Count);
                //db.data_in = cb, 'aaa'
                db.data_in.Clear();
                Assert.AreEqual(0, db.lostDummySockets.Count);

                ca.Write(new byte[] { (byte)'c', (byte)'c', (byte)'c' });
                cb.Write(new byte[] { (byte)'d', (byte)'d', (byte)'d' });
                Thread.Sleep(1000);

                Assert.AreEqual(0, da.externalDummySockets.Count);
                //da.data_in = ca, 'ddd'
                da.data_in.Clear();
                Assert.AreEqual(0, da.lostDummySockets.Count);
                Assert.AreEqual(0, db.externalDummySockets.Count);
                //db.data_in = cb, 'ccc'
                db.data_in.Clear();
                Assert.AreEqual(0, db.lostDummySockets.Count);

                ca.Close();
                Thread.Sleep(1000);

                Assert.AreEqual(0, da.externalDummySockets.Count);
                Assert.AreEqual(0, da.data_in.Count);
                Assert.AreEqual(0, da.lostDummySockets.Count);
                Assert.AreEqual(0, db.externalDummySockets.Count);
                Assert.AreEqual(0, db.data_in.Count);
                Assert.AreEqual(1, db.lostDummySockets.Count);
                db.lostDummySockets.Clear();
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            finally
            {
                fa.Set();
                fb.Set();
            }

        }
    }
}
