using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate void TaskDelegate();
    public delegate void SchedulerDelegate(TaskDelegate func, double delay, string TaskName);

    public class RawServer
    {
        private double timeoutCheckInterval;

        private double timeout;

        private Flag doneFlag;

        private bool noisy;

        private List<Task> tasks;

        private List<ExternalTask> externalTasks;

        private List<SingleSocket> deadFromWrite;

        private Dictionary<IntPtr, SingleSocket> singleSockets;

        private Poll poll;

        public Poll Poll
        {
            get { return this.poll; }
            set { this.poll = value; }
        }

        private Socket server;

        private Encrypter handler;

        public Encrypter Handler
        {
            get { return this.handler; }
            set { this.handler = value; }
        }

        public RawServer(Flag doneFlag, double timeoutCheckInterval, double timeout, bool noisy)
        {
            this.timeoutCheckInterval = timeoutCheckInterval;
            this.timeout = timeout;
            this.poll = new Poll();
            singleSockets = new Dictionary<IntPtr, SingleSocket>();
            deadFromWrite = new List<SingleSocket>();
            this.doneFlag = doneFlag;
            this.noisy = noisy;
            tasks = new List<Task>();
            externalTasks = new List<ExternalTask>();
            this.AddTask(new TaskDelegate(ScanForTimeouts), timeoutCheckInterval, "Scan for timeouts");
        }

        public void AddTask(TaskDelegate taskFunction, double delay, string taskName)
        {
            lock (this)
            {
                Debug.WriteLine(string.Format("Task : {0} : {1}", taskName, delay.ToString()));


                tasks.Add(new Task(taskFunction, DateTime.Now.AddSeconds(delay)));
                tasks.Sort();
            }
        }

        public void AddExternalTask(TaskDelegate taskFunction, double delay, string taskName)
        {
            lock (this)
            {
                Debug.WriteLine(string.Format("External Task : {0} : {1}", taskName, delay.ToString()));
                externalTasks.Add(new ExternalTask(taskFunction, delay, taskName));
            }
        }

        public void AddToDeadFromWrite(SingleSocket item)
        {
            deadFromWrite.Add(item);
        }

        public void RemoveSingleSockets(IntPtr key)
        {
            this.singleSockets.Remove(key);
        }

        public void ScanForTimeouts()
        {
            AddTask(new TaskDelegate(ScanForTimeouts), timeoutCheckInterval, "Scan for timeouts");
            DateTime timeoutTime = DateTime.Now.AddSeconds(-timeout);
            List<SingleSocket> toKill = new List<SingleSocket>();
            foreach (SingleSocket item in singleSockets.Values)
            {
                if (item.LastHit < timeoutTime && item.LastHit != DateTime.MinValue)
                {
                    toKill.Add(item);
                }
            }

            foreach (SingleSocket killSocket in toKill)
            {
                if (killSocket != null)
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

        public SingleSocket StartConnect(IPEndPoint dns, Encrypter handler)
        {
            if (handler == null)
            {
                handler = this.handler;
            }

            Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Blocking = false;
            try
            {
                sock.Connect(dns);
            }

            catch (Exception ex)
            {
            }

            poll.Register(sock, PollMode.PollIn);
            SingleSocket singleSocket = new SingleSocket(this, sock, handler);
            singleSockets[sock.Handle] = singleSocket;
            return singleSocket;
        }

        public void HandleEvents(List<PollItem> events)
        {
            foreach (PollItem item in events)
            {
                if (item.Socket.Handle == server.Handle)
                {
                    if ((item.Mode & (PollMode.PollError | PollMode.PollHangUp)) != 0)
                    {
                        poll.Unregister(server);
                        server.Close();
                    }
                    else
                    {
                        Socket newSocket = server.Accept();
                        newSocket.Blocking = false;
                        SingleSocket newSingleSocket = new SingleSocket(this, newSocket, handler);
                        singleSockets[newSocket.Handle] = newSingleSocket;
                        poll.Register(newSocket, PollMode.PollIn);
                        //handler


                    }
                }
            }
        }

        private void CloseDead()
        {
            while (deadFromWrite.Count > 0)
            {
                List<SingleSocket> old = deadFromWrite;
                deadFromWrite = new List<SingleSocket>();
                foreach (SingleSocket item in old)
                {
                    if (item.Socket != null)
                    {
                        CloseSocket(item);
                    }
                }
            }
        }

        private void CloseSocket(SingleSocket singleSocket)
        {
            poll.Unregister(singleSocket.Socket);
            singleSockets.Remove(singleSocket.Socket.Handle);
            singleSocket.Socket.Close();
            singleSocket.Socket = null;
            throw new NotImplementedException();
        }
    }
}
