using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.RawServers;
using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.UnitTest.TestLibBitTorrent.TestRawServers
{
    public class DummySingleSocket : ISingleSocket
    {
        #region Private Field

        /// <summary>
        /// 服务器
        /// </summary>
        private DummyRawServer rawServer;

        /// <summary>
        /// 套接字
        /// </summary>
        private Socket socket;

        /// <summary>
        /// 上次点击的时间
        /// </summary>
        private DateTime lastHit;

        /// <summary>
        /// 是否已经连接
        /// </summary>
        private bool isConnected;

        /// <summary>
        /// 缓冲区
        /// </summary>
        private LinkedList<byte[]> buffer;

        /// <summary>
        /// 封装连接管理类
        /// </summary>
        private DummyEncrypter handler;

        #endregion

        #region Public Properties


        /// <summary>
        /// 访问和设置套接字
        /// </summary>
        public Socket Socket
        {
            get { return socket; }
            set { socket = value; }
        }

        /// <summary>
        /// 访问和设置上次点击的时间
        /// </summary>
        public DateTime LastHit
        {
            get { return lastHit; }
            set { lastHit = value; }
        }

        /// <summary>
        /// 访问和设置是否已经连接
        /// </summary>
        public bool IsConnected
        {
            get { return isConnected; }
            set { isConnected = value; }
        }

        /// <summary>
        /// 访问和设置封装连接管理类
        /// </summary>
        public DummyEncrypter Handler
        {
            get { return handler; }
            set { handler = value; }
        }

        /// <summary>
        /// 连接的IP地址
        /// </summary>
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

        /// <summary>
        /// 访问缓冲区是否为0
        /// </summary>   
        public bool IsFlushed
        {
            get { return buffer.Count == 0; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rawServer">服务器</param>
        /// <param name="socket">套接字</param>
        /// <param name="handler">封装连接管理类</param>
        public DummySingleSocket(DummyRawServer rawServer, Socket socket, DummyEncrypter handler)
        {
            this.rawServer = rawServer;
            this.socket = socket;
            this.handler = handler;
            buffer = new LinkedList<byte[]>();
            lastHit = DateTime.Now;
            isConnected = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void Close()
        {
            Close(false);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="closing">是否从服务器取消连接</param>
        public void Close(bool closing)
        {
            Socket tempSocket = this.socket;
            this.socket = null;

            //新建缓冲区
            buffer = new LinkedList<byte[]>();
            if (!closing)
            {
                rawServer.RemoveSingleSockets(tempSocket.Handle);
            }
            rawServer.Poll.Unregister(tempSocket);

            //关闭套接字
            tempSocket.Close();
        }

        /// <summary>
        /// 取消接收和发送数据
        /// </summary>
        /// <param name="how">不再允许操作的事件</param>
        public void Shutdown(SocketShutdown how)
        {
            socket.Shutdown(how);
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="bytes">待写入的数据</param>
        public void Write(byte[] bytes)
        {
            //将数据写入缓冲区
            buffer.AddLast(bytes);

            //如果缓冲区的数量为1，则发送数据
            if (buffer.Count == 1)
            {
                TryWrite();
            }
        }

        /// <summary>
        /// 缓冲区写数据
        /// </summary>
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
                        byte[] bytes = buffer.First.Value;
                        bytesLength = bytes.Length;
                        amount = socket.Send(bytes);

                        buffer.RemoveFirst();

                        //如果发送了数据的长度不等于应该发生的长度，则代表发送出错
                        if (amount != bytesLength)
                        {
                            //如果已经发送了一些数据
                            if (amount != 0)
                            {
                                byte[] anotherBuffer = new byte[bytesLength - amount];
                                Globals.CopyBytes(bytes, amount, anotherBuffer);
                                buffer.AddFirst(anotherBuffer);
                            }
                            break;
                        }
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

        #endregion

        #region ISingleSocket Members


        IEncrypter ISingleSocket.Handler
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void ShutDown(SocketShutdown how)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
