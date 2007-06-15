using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// PollItem
    /// </summary>
    public class PollItem
    {
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
        /// Poll的状态
        /// </summary>
        private PollMode mode;

        /// <summary>
        /// 访问和设置Poll的状态
        /// </summary>
        public PollMode Mode
        {
            get { return this.mode; }
            set { this.mode = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="socket">套接字</param>
        /// <param name="mode">Poll的状态</param>
        public PollItem(Socket socket, PollMode mode)
        {
            Socket = socket;
            Mode = mode;
        }
    }
}
