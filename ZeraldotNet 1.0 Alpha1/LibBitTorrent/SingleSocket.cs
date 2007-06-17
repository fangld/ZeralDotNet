using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 单套接字类
    /// </summary>
    public class SingleSocket
    {
        /// <summary>
        /// 服务器
        /// </summary>
        private RawServer rawServer;

        /// <summary>
        /// 设置服务器
        /// </summary>
        public RawServer RawServer
        {
            set { rawServer = value; }
        }

        /// <summary>
        /// 套接字
        /// </summary>
        private Socket socket;

        /// <summary>
        /// 访问和设置套接字
        /// </summary>
        public Socket Socket
        {
            get { return this.socket; }
            set { this.socket = value; }
        }

        /// <summary>
        /// 上次点击的时间
        /// </summary>
        private DateTime lastHit;

        /// <summary>
        /// 访问和设置上次点击的时间
        /// </summary>
        public DateTime LastHit
        {
            get { return this.lastHit; }
            set { this.lastHit = value; }
        }

        /// <summary>
        /// 是否已经连接
        /// </summary>
        private bool isConnect;

        /// <summary>
        /// 访问和设置是否已经连接
        /// </summary>
        public bool IsConnect
        {
            get { return this.isConnect; }
            set { this.isConnect = value; }
        }

        /// <summary>
        /// 缓冲区
        /// </summary>
        private List<byte[]> buffer;

        /// <summary>
        /// 
        /// </summary>
        private Encrpyter handler;

        /// <summary>
        /// 
        /// </summary>
        public Encrpyter Handler
        {
            get { return this.handler; }
            set { this.handler = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rawServer">服务器</param>
        /// <param name="socket">套接字</param>
        /// <param name="handler"></param>
        public SingleSocket(RawServer rawServer, Socket socket, Encrpyter handler)
        {
            this.RawServer = rawServer;
            this.Socket = socket;
            this.Handler = handler;

            this.buffer = new List<byte[]>();
            this.lastHit = DateTime.Now;
            this.isConnect = false;
        }

        /// <summary>
        /// 返回连接的IP地址
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
            buffer = new List<byte[]>();
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
        public void ShutDown(SocketShutdown how)
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
            buffer.Add(bytes);
            
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
            if (isConnect)
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
                                //Buffer.BlockCopy(bytes, amount, anotherBuffer, 0, anotherBuffer.Length);
                                CopyBytes(bytes, amount, anotherBuffer);
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

        /// <summary>
        /// 复制数组
        /// </summary>
        /// <param name="source">被复制的数组</param>
        /// <param name="sourceOffset">被复制数组的偏移位置</param>
        /// <param name="target">写入的数组</param>
        private void CopyBytes(byte[] source, int sourceOffset, byte[] target)
        {
            int position;
            for (position = sourceOffset; position < source.Length; position++)
            {
                target[position - sourceOffset] = source[position];
            }
        }
    }
}
