using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using ZeraldotNet.LibBitTorrent;


namespace ZeraldotNet.TestLibBitTorrent.TestRawServer
{
    public class DummyRawServer
    {
        private double timeoutCheckInterval;
        private double timeout;
        private Flag doneFlag;
        private bool noisy;
        private List<Task> tasks;
        private List<ExternalTask> externalTasks;
        public List<DummySingleSocket> deadFromWrite;
        public Dictionary<IntPtr, DummySingleSocket> singleSockets;

        private Poll poll;

        public Poll Poll
        {
            get { return poll; }
            set { poll = value; }
        }
        Socket server;
        private DummyEncrypter handler;

        public DummyEncrypter Handler
        {
            get { return handler; }
            set { handler = value; }
        }

        public DummyRawServer(Flag doneFlag, double timeoutCheckInterval, double timeout, bool noisy)
        {
            this.timeoutCheckInterval = timeoutCheckInterval;
            this.timeout = timeout;
            this.poll = new Poll();
            singleSockets = new Dictionary<IntPtr, DummySingleSocket>();
            deadFromWrite = new List<DummySingleSocket>();
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

        public void AddToDeadFromWrite(DummySingleSocket item)
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
            List<DummySingleSocket> toKill = new List<DummySingleSocket>();
            foreach (DummySingleSocket item in singleSockets.Values)
            {
                if (item.LastHit < t)
                    toKill.Add(item);
            }
            foreach (DummySingleSocket killSocket in toKill)
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

        public DummySingleSocket StartConnection(IPEndPoint dns, DummyEncrypter handler)
        {
            if (handler == null)
                handler = this.handler;
            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Blocking = false;
            try
            {
                sock.Connect(dns);
            }
            catch { }
            poll.Register(sock, PollMode.PollIn);
            DummySingleSocket s = new DummySingleSocket(this, sock, handler);
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
                        DummySingleSocket nss = new DummySingleSocket(this, newsock, handler);
                        singleSockets[newsock.Handle] = nss;
                        poll.Register(newsock, PollMode.PollIn);
                        handler.MakeExternalConnection(nss);
                    }
                }
                else
                {
                    DummySingleSocket s = (DummySingleSocket)singleSockets[pi.Socket.Handle];
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
                    if (((pi.Mode & PollMode.PollOut) != 0) && (s.Socket != null) && (!s.IsFlushed))
                    {
                        s.TryWrite();
                        if (s.IsFlushed)
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

        public void ListenForever(DummyEncrypter handler)
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
                            catch
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
                    catch
                    {
                        //TODO: print_exc()
                    }
                }
            }
            finally
            {
                foreach (DummySingleSocket ss in singleSockets.Values)
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
                List<DummySingleSocket> old = deadFromWrite;
                deadFromWrite = new List<DummySingleSocket>();
                foreach (DummySingleSocket s in old)
                    if (s.Socket != null)
                        CloseSocket(s);
            }
        }

        private void CloseSocket(DummySingleSocket s)
        {
            poll.Unregister(s.Socket);
            singleSockets.Remove(s.Socket.Handle);
            s.Socket.Close();
            s.Socket = null;
            s.Handler.LoseConnection(s);
        }
    }
}
