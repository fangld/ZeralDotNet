using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Listener that accept peer
    /// </summary>
    public class Listener : IDisposable
    {
        #region Fields

        private Socket _socket;

        private bool _running;

        #endregion

        #region Events

        public event EventHandler<Peer> NewPeer;

        public event EventHandler<string> ListenFail;

        #endregion

        #region Constructors

        public Listener()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            _running = false;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Listen
        /// </summary>
        public async void Listen()
        {
            lock (this)
            {
                _running = true;
                IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, Setting.PeerListenningPort);
                _socket.Bind(localEndPoint);
                _socket.Listen(Setting.ListenBacklog);
                SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += acceptEventArg_Completed;
                if (!_socket.AcceptAsync(acceptEventArg))
                {
                    acceptEventArg_Completed(this, acceptEventArg);
                }
            }
        }

        public async void Stop()
        {
            lock (this)
            {
                _running = false;
                _socket.Dispose();
            }
        }

        void acceptEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (_running)
            {
                if (e.SocketError == SocketError.Success)
                {
                    Peer peer = new Peer(e.AcceptSocket);
                    NewPeer(this, peer);
                }
                else
                {
                    ListenFail(this, e.SocketError.ToString());
                }

                SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += acceptEventArg_Completed;
                if (!_socket.AcceptAsync(acceptEventArg))
                {
                    acceptEventArg_Completed(this, acceptEventArg);
                }
            }
        }

        #endregion

        public void Dispose()
        {
            _socket.Dispose();
        }
    }
}
