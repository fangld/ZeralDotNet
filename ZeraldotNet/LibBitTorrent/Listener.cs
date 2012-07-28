using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent
{
    public class Listener : IDisposable
    {
        #region Fields

        private Socket _socket;

        #endregion

        #region Event

        public event EventHandler<Peer> NewPeer;

        public event EventHandler<string> ListenFail;

        #endregion

        #region Constructors

        public Listener()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        }

        #endregion

        #region Methods

        public void Listen()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Setting.ListenPort);
            _socket.Bind(localEndPoint);
            _socket.Listen(Setting.ListenBacklog);
            SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
            acceptEventArg.Completed += acceptEventArg_Completed;
            if (!_socket.AcceptAsync(acceptEventArg))
            {
                acceptEventArg_Completed(this, acceptEventArg);
            }
        }

        void acceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Peer peer =  new Peer(e.AcceptSocket);
                NewPeer(this, peer);
            }
            else
            {
                ListenFail(this, e.SocketError.ToString());
            }
        }

        #endregion

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
