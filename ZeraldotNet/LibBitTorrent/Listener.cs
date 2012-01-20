using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent
{
    public class Listener
    {
        #region Fields

        private Socket _socket;

        #endregion

        #region Event

        #endregion

        #region Constructors

        public  Listener()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        }

        #endregion


        #region Methods

        public void Bind()
        {
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, Setting.Port);
            _socket.Bind(endPoint);
        }

        public void Accept()
        {
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(e_Completed);
            _socket.AcceptAsync(e);
        }

        void e_Completed(object sender, SocketAsyncEventArgs e)
        {
            //e.AcceptSocket;
        }

        #endregion
    }
}
