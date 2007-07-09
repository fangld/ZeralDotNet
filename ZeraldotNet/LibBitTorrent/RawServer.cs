using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using ZeraldotNet.LibBitTorrent.DataStructures;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate void TaskDelegate();
    public delegate void SchedulerDelegate(TaskDelegate func, double delay, string TaskName);

    /// <summary>
    /// 服务器类
    /// </summary>
    public class RawServer
    {
        #region Private Field

        /// <summary>
        /// 检查超时的间隔时间
        /// </summary>
        private double timeoutCheckInterval;

        /// <summary>
        /// 超时的时间
        /// </summary>
        private double timeout;

        /// <summary>
        /// 完成标志
        /// </summary>
        private Flag doneFlag;

        private bool noisy;

        /// <summary>
        /// 任务最小堆
        /// </summary>
        private MinHeap<Task> tasks;

        /// <summary>
        /// 外部任务列表
        /// </summary>
        private List<ExternalTask> externalTasks;

        /// <summary>
        /// 死连接的单套接字列表
        /// </summary>
        private List<SingleSocket> deadFromWrite;

        /// <summary>
        /// 保存SingleSocket中的Socket与IntPtr对应的字典
        /// </summary>
        private Dictionary<IntPtr, SingleSocket> singleSocketDictionary;

        /// <summary>
        /// 选择器
        /// </summary>
        private Poll poll;

        /// <summary>
        /// 服务器套接字
        /// </summary>
        private Socket server;

        /// <summary>
        /// 封装连接管理类
        /// </summary>
        private Encrypter handler;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问和设置选择器
        /// </summary>
        public Poll Poll
        {
            get { return this.poll; }
            set { this.poll = value; }
        }

        /// <summary>
        /// 访问和设置封装连接管理类
        /// </summary>
        public Encrypter Handler
        {
            get { return this.handler; }
            set { this.handler = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="doneFlag">完成标志</param>
        /// <param name="timeoutCheckInterval">检查超时的间隔时间</param>
        /// <param name="timeout">超时的时间</param>
        /// <param name="noisy"></param>
        public RawServer(Flag doneFlag, double timeoutCheckInterval, double timeout, bool noisy)
        {
            this.timeoutCheckInterval = timeoutCheckInterval;
            this.timeout = timeout;
            this.poll = new Poll();
            this.singleSocketDictionary = new Dictionary<IntPtr, SingleSocket>();
            this.deadFromWrite = new List<SingleSocket>();
            this.doneFlag = doneFlag;
            this.noisy = noisy;
            this.tasks = new MinHeap<Task>();
            this.externalTasks = new List<ExternalTask>();
            this.AddTask(new TaskDelegate(ScanForTimeouts), timeoutCheckInterval, "Scan for timeouts");
        }

        #endregion

        #region Methods

        /// <summary>
        /// 要延时执行的任务计算出它的实际执行时间，并把它添加到一个排好序的最小堆Task
        /// </summary>
        /// <param name="taskFunction">任务函数</param>
        /// <param name="delay">延迟执行事件</param>
        /// <param name="taskName">任务名称</param>
        public void AddTask(TaskDelegate taskFunction, double delay, string taskName)
        {
            lock (this)
            {
                tasks.Add(new Task(taskFunction, DateTime.Now.AddSeconds(delay)));
            }
        }

        /// <summary>
        /// 增加外部任务
        /// </summary>
        /// <param name="taskFunction">任务函数</param>
        /// <param name="delay">延迟执行事</param>
        /// <param name="taskName">任务名称</param>
        public void AddExternalTask(TaskDelegate taskFunction, double delay, string taskName)
        {
            lock (this)
            {
                externalTasks.Add(new ExternalTask(taskFunction, delay, taskName));
            }
        }

        /// <summary>
        /// 增加死连接
        /// </summary>
        /// <param name="item"></param>
        public void AddToDeadFromWrite(SingleSocket item)
        {
            deadFromWrite.Add(item);
        }

        /// <summary>
        /// 删去句柄为key的单套接字
        /// </summary>
        /// <param name="key"></param>
        public void RemoveSingleSockets(IntPtr key)
        {
            this.singleSocketDictionary.Remove(key);
        }

        /// <summary>
        /// 检查超时的网络连接，并关闭它们
        /// </summary>
        public void ScanForTimeouts()
        {
            this.AddTask(new TaskDelegate(ScanForTimeouts), timeoutCheckInterval, "Scan for timeouts");
            DateTime timeoutTime = DateTime.Now.AddSeconds(-timeout);
            foreach (SingleSocket item in singleSocketDictionary.Values)
            {
                if (item.LastHit < timeoutTime && item.LastHit != DateTime.MinValue && item != null)
                {
                    this.CloseSocket(item);
                }
            }
        }

        /// <summary>
        /// 绑定函数
        /// </summary>
        /// <remarks>
        /// 把得到的网络插口和它对应的处理对象添加到一个字典中，
        /// 该字典以网络插口的描述符(FD)为主键。
        /// </remarks>
        /// <param name="port">绑定的端口</param>
        /// <param name="bind">绑定的地址</param>
        /// <param name="reuse"></param>
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

        /// <summary>
        /// 开始连接
        /// </summary>
        /// <param name="dns">一个代表远程节点的IPEndPoint</param>
        /// <param name="handler">封装连接管理类</param>
        /// <returns>返回一个单套接字</returns>
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

            catch
            {
            }

            poll.Register(sock, PollMode.PollIn);
            SingleSocket singleSocket = new SingleSocket(this, sock, handler);
            singleSocketDictionary[sock.Handle] = singleSocket;
            return singleSocket;
        }

        /// <summary>
        /// 处理事件
        /// </summary>
        /// <param name="events">待处理的事件</param>
        public void HandleEvents(List<PollItem> events)
        {
            foreach (PollItem item in events)
            {
                if (item.Socket.Handle == server.Handle)
                {
                    HandleServerSocket(item);
                }

                else
                {
                    HandleClientSocket(item);
                }
            }
        }

        /// <summary>
        /// 处理服务器套接字
        /// </summary>
        /// <param name="item">待处理的PollItem</param>
        private void HandleServerSocket(PollItem item)
        {
            //如果出错或者挂起，则关闭连接
            if ((item.Mode & (PollMode.PollError | PollMode.PollHangUp)) != 0)
            {
                poll.Unregister(server);
                server.Close();
            }

            //否则建立一个新的socket，然后让侦听中的插口去accept它，以后数据的读写应该在新的socket中进行。
            else
            {
                Socket newSocket = server.Accept();
                newSocket.Blocking = false;
                SingleSocket newSingleSocket = new SingleSocket(this, newSocket, handler);
                singleSocketDictionary[newSocket.Handle] = newSingleSocket;
                poll.Register(newSocket, PollMode.PollIn);
                handler.MakeExternalConnection(newSingleSocket);
            }
        }

        /// <summary>
        /// 处理客户端套接字
        /// </summary>
        /// <param name="item">待处理的PollItem</param>
        private void HandleClientSocket(PollItem item)
        {

            if (!singleSocketDictionary.ContainsKey(item.Socket.Handle))
            {
                return;
            }

            SingleSocket singleSocket = singleSocketDictionary[item.Socket.Handle];

            singleSocket.IsConnected = true;

            //如果出错或者挂起，则关闭连接
            if ((item.Mode & (PollMode.PollError | PollMode.PollHangUp)) != 0)
            {
                this.CloseSocket(singleSocket);
                return;
            }

            //如果有数据可读，则读数据
            if ((item.Mode & PollMode.PollIn) != 0)
            {
                try
                {
                    singleSocket.LastHit = DateTime.Now;
                    int available = singleSocket.Socket.Available;

                    if (available == 0)
                    {
                        this.CloseSocket(singleSocket);
                    }

                    else
                    {
                        //把数据读到一个缓冲区
                        byte[] data = new byte[available];
                        singleSocket.Socket.Receive(data, 0, SocketFlags.None);
                        
                        //调用封装连接管理类的DataCameIn进行处理
                        singleSocket.Handler.DataCameIn(singleSocket, data);
                    }
                }

                catch (SocketException socketEx)
                {
                    if (socketEx.ErrorCode != 10035) //WSAE WOULD BLOCK
                    {
                        this.CloseSocket(singleSocket);
                        return;
                    }
                }
            }

            //如果有数据可写，则写数据
            if (((item.Mode & PollMode.PollOut) != 0) && (singleSocket.Socket != null) && (!singleSocket.IsFlushed))
            {
                singleSocket.TryWrite();
                //最后检查是否数据都已经真的发出去了(flushed)，如果是，
                //则调用封装连接管理类中提供的FlushConnection函数进行处理。
                if (singleSocket.IsFlushed)
                {
                    singleSocket.Handler.FlushConnection(singleSocket);
                }
            }
        }

        /// <summary>
        /// 将所有外部任务转换到内部任务
        /// </summary>
        public void PopExternal()
        {
            foreach (ExternalTask item in externalTasks)
            {
                this.AddTask(item.TaskFunction, item.Delay, item.TaskName);
            }
            externalTasks.Clear();
        }

        /// <summary>
        /// 重复监听
        /// </summary>
        /// <param name="handler">待监听的封装连接管理类</param>
        public void ListenForever(Encrypter handler)
        {
            this.handler = handler;
            bool isContinue = true;
            try
            {
                //是否终止监听
                while (!doneFlag.IsSet && isContinue )
                {
                    isContinue = this.ListenOnce();
                }
            }

            finally
            {
                foreach (SingleSocket item in this.singleSocketDictionary.Values)
                {
                    item.Close(true);
                }
                server.Close();
            }
        }

        /// <summary>
        /// 一次监听
        /// </summary>
        /// <returns>返回是否结束监听</returns>
        private bool ListenOnce()
        {
            double period;
            Task firstTask;
            List<PollItem> events;
            try
            {
                this.PopExternal();

                //从添加的任务funcs寻找最近要执行的任务的时间，并与当前时间相减，计算出period
                if (this.tasks.Count == 0)
                {
                    period = 1000000000.0;
                }
                else
                {
                    firstTask = tasks.Peek();
                    period = (firstTask.When - DateTime.Now).TotalSeconds;
                }


                if (period < 0)
                {
                    period = 0;
                }

                //poll轮询这么长的时间，这样做就可以保证轮询结束后不会耽误外部任务过久。
                events = this.poll.Select((int)period * 1000);

                //是否终止监听
                if (doneFlag.IsSet)
                {
                    return false;
                }

                while (tasks.Count > 0)
                {
                    if (tasks.Peek().When <= DateTime.Now)
                    {
                        firstTask = tasks.ExtractFirst();
                        try
                        {
                            firstTask.TaskFunction();
                        }

                        catch
                        {
                            if (this.noisy)
                            {

                            }                       
                        }
                    }

                    else
                    {
                        break;
                    }

                }

                this.CloseDead();
                this.HandleEvents(events);

                //是否终止监听
                if (doneFlag.IsSet)
                {
                    return false;
                }
                this.CloseDead();
            }

            catch
            {

            }

            return true;
        }

        /// <summary>
        /// 关闭死连接
        /// </summary>
        private void CloseDead()
        {
            if (deadFromWrite.Count > 0)
            {
                foreach (SingleSocket item in deadFromWrite)
                {
                    if (item.Socket != null)
                    {
                        CloseSocket(item);
                    }
                }
                deadFromWrite.Clear();
            }
        }

        /// <summary>
        /// 关闭单套接字
        /// </summary>
        /// <param name="singleSocket">待关闭的单套接字</param>
        private void CloseSocket(SingleSocket singleSocket)
        {
            //在poll注销Socket
            poll.Unregister(singleSocket.Socket);

            //在字典中移除singleSocket
            singleSocketDictionary.Remove(singleSocket.Socket.Handle);

            //关闭套接字
            singleSocket.Socket.Close();
            singleSocket.Socket = null;

            //关闭singleSocket的连接
            singleSocket.Handler.CloseConnection(singleSocket);
        }

        #endregion
    }
}
