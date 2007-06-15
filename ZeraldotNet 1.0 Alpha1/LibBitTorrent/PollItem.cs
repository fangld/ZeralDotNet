using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ZeraldotNet.LibBitTorrent
{
    public class PollItem
    {
        private Socket socket;

        public Socket Socket
        {
            get { return this.socket; }
            set { this.socket = value; }
        }

        private PollMode mode;

        public PollMode Mode
        {
            get { return this.mode; }
            set { this.mode = value; }
        }

        public PollItem(Socket socket, PollMode mode)
        {
            Socket = socket;
            Mode = mode;
        }
    }
}
