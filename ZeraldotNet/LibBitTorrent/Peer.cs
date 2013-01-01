using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ZeraldotNet.LibBitTorrent.Messages;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// The peer that handles messages
    /// </summary>
    public class Peer : IEquatable<Peer>, IDisposable
    {
        #region Fields

        /// <summary>
        /// The socket of peer
        /// </summary>
        private Socket _socket;

        private BufferPool _bufferPool;

        /// <summary>
        /// The bitfield of peer
        /// </summary>
        private bool[] _bitfield;

        private List<int> _requestedIndexes;

        private Timer _timer;

        #endregion

        #region Properties

        /// <summary>
        /// The host address of peer
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// The port of peer
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The SHA1 hash of the torrent
        /// </summary>
        public byte[] InfoHash { get; set; }

        /// <summary>
        /// The peer id
        /// </summary>
        public string PeerId { get; set; }

        /// <summary>
        /// The flag that represents whether peer supports extension protocol
        /// </summary>
        public bool SupportExtension { get; set; }

        /// <summary>
        /// The flag that represents whether peer supports DHT.
        /// </summary>
        public bool SupportDht { get; set; }

        /// <summary>
        /// The flag that represents whether peer supports peer exchange.
        /// </summary>
        public bool SupportPeerExchange { get; set; }

        /// <summary>
        /// The flag that represents whether peer supports fast peer.
        /// </summary>
        public bool SupportFastPeer { get; set; }

        /// <summary>
        /// The flag that represents whether peer is connected.
        /// </summary>
        public bool IsConnected { get; private set; }

        /// <summary>
        /// The flag that represents whether peer is handshaked.
        /// </summary>
        public bool IsHandshaked { get; set; }

        /// <summary>
        /// The flag that represents whether peer is choked by me.
        /// </summary>
        public bool AmChoking { get; set; }

        /// <summary>
        /// The flag that represents whether peer is interested by me.
        /// </summary>
        public bool AmInterested { get; set; }

        /// <summary>
        /// The flag that represents whether peer is choking me.
        /// </summary>
        public bool PeerChoking { get; set; }

        /// <summary>
        /// The flag that represents whether peer is interested with me.
        /// </summary>
        public bool PeerInterested { get; set; }

        #endregion

        #region Events

        public event EventHandler OnConnected;
        public event EventHandler<SocketException> ConnectFail;
        public event EventHandler ReceiveFail;
        public event EventHandler SendFail;
        public event EventHandler TimeOut;
        public event EventHandler<Message> MessageSending;
        public event EventHandler<HandshakeMessage> HandshakeMessageReceived;
        public event EventHandler<KeepAliveMessage> KeepAliveMessageReceived;
        public event EventHandler<ChokeMessage> ChokeMessageReceived;
        public event EventHandler<UnchokeMessage> UnchokeMessageReceived;
        public event EventHandler<InterestedMessage> InterestedMessageReceived;
        public event EventHandler<NotInterestedMessage> NotInterestedMessageReceived;
        public event EventHandler<HaveMessage> HaveMessageReceived;
        public event EventHandler<BitfieldMessage> BitfieldMessageReceived;
        public event EventHandler<RequestMessage> RequestMessageReceived;
        public event EventHandler<PieceMessage> PieceMessageReceived;
        public event EventHandler<CancelMessage> CancelMessageReceived;
        public event EventHandler<PortMessage> PortMessageReceived;
        public event EventHandler<SuggestPieceMessage> SuggestPieceMessageReceived;
        public event EventHandler<HaveAllMessage> HaveAllMessageReceived;
        public event EventHandler<HaveNoneMessage> HaveNoneMessageReceived;
        public event EventHandler<RejectRequestMessage> RejectRequestMessageReceived;
        public event EventHandler<AllowedFastMessage> AllowedFastMessageReceived;
        public event EventHandler<ExtendedListMessage> ExtendedListMessageReceived;

        #endregion

        #region Constructors

        /// <summary>
        /// Create new nonconnected peer
        /// </summary>
        public Peer()
        {
            Initial();
            IsConnected = false;
        }

        /// <summary>
        /// Create new connected peer with socket
        /// </summary>
        public Peer(Socket socket) : this()
        {
            Initial();
            _socket = socket;
            IPEndPoint ipEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            Host = ipEndPoint.Address.ToString();
            Port = ipEndPoint.Port;
            IsConnected = true;
        }

        /// <summary>
        /// Set the initial status of the peer
        /// </summary>
        private void Initial()
        {
            _bufferPool = new BufferPool(Setting.BufferPoolCapacity);
            _requestedIndexes = new List<int>();
            AmChoking = true;
            AmInterested = false;
            PeerChoking = true;
            PeerInterested = false;
            IsHandshaked = false;
            _timer = new Timer(Setting.PeerAliveInterval);
            _timer.Elapsed += _timer_Elapsed;
        }

        #endregion

        #region Methods

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            TimeOut(this, e);
            _timer.Start();
        }

        #region Connect Methods

        /// <summary>
        /// ConnectAsync a remote peer
        /// </summary>
        public void ConnectAsync()
        {
            Debug.Assert(!IsConnected);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            SocketAsyncEventArgs connectEventArg = new SocketAsyncEventArgs();
            connectEventArg.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(Host), Port);
            connectEventArg.Completed += connectSocketArg_Completed;
            try
            {
                if (!_socket.ConnectAsync(connectEventArg))
                {
                    Debug.Assert(OnConnected != null);
                    IsConnected = true;
                    OnConnected(this, null);
                }
            }
            catch(SocketException e)
            {
                Debug.Assert(ConnectFail != null);
                IsConnected = false;
                ConnectFail(this, e);
            }

        }

        void connectSocketArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Debug.Assert(OnConnected != null);
                IsConnected = true;
                OnConnected(this, null);
            }
            else
            {
                Debug.Assert(ConnectFail != null);
                IsConnected = false;
                ConnectFail(this, null);
            }
        }

        #endregion

        #region Receive Methods

        /// <summary>
        /// Receive messages from a remote peer
        /// </summary>
        public void ReceiveAsnyc()
        {
            try
            {
                if (IsConnected)
                {
                    byte[] buffer = new byte[Setting.TrackerBufferLength];
                    SocketAsyncEventArgs rcvEventArg = new SocketAsyncEventArgs();
                    rcvEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(rcvEventArg_Completed);
                    rcvEventArg.SetBuffer(buffer, 0, Setting.TrackerBufferLength);
                    if (!_socket.ReceiveAsync(rcvEventArg))
                    {
                        rcvEventArg_Completed(this, rcvEventArg);
                    }
                }
                else
                {
                    ReceiveFail(this, null);
                }
            }
            catch (NullReferenceException)
            {
                ReceiveFail(this, null);
            }
        }

        private void rcvEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            lock (this)
            {
                try
                {
                    if (IsConnected)
                    {
                        _timer.Stop();

                        if (e.SocketError == SocketError.Success)
                        {
                            if (e.BytesTransferred > 0)
                            {
                                _bufferPool.Write(e.Buffer, 0, e.BytesTransferred);

                                //int index = 0;

                                //for (int i = 0; i < e.BytesTransferred - 2; i += 2)
                                //{
                                //   // Console.Write("{0}:{1} {2:D3}:{3:D3}|{4}|{3:X2}   ", Host, Port, index++, readBytes[i], (char)readBytes[i]);
                                //    //Console.WriteLine("{0:D3}:{1:D3}|{2}|{1:X2}", index++, readBytes[i + 1], (char)readBytes[i + 1]);


                                //if (e.BytesTransferred%2 == 1)
                                //{
                                //    //Console.WriteLine("{0}:{1} {2:D3}:{3:D3}|{4}|{3:X2}   ", Host, Port, index++, readBytes[e.BytesTransferred - 1], (char)readBytes[e.BytesTransferred - 1]);
                                //}
                                //else if (e.BytesTransferred != 0)
                                //{
                                //    //Console.Write("{0}:{1} {2:D3}:{3:D3}|{4}|{3:X2}   ", Host, Port, index++, readBytes[e.BytesTransferred - 2], (char)readBytes[e.BytesTransferred - 2]);
                                //    //Console.WriteLine("{0:D3}:{1:D3}|{2}|{1:X2}", index++, readBytes[e.BytesTransferred - 1], (char)readBytes[e.BytesTransferred - 1]);
                                //}

                                //Console.WriteLine("Sum length:{0}", _sumLength);

                                Message message;
                                while ((message = Message.Parse(_bufferPool, _bitfield.Length)) != null)
                                {
                                    message.Handle(this);
                                    switch (message.Type)
                                    {
                                        case MessageType.Handshake:
                                            HandshakeMessageReceived(this, (HandshakeMessage)message);
                                            break;
                                        case MessageType.KeepAlive:
                                            KeepAliveMessageReceived(this, (KeepAliveMessage)message);
                                            break;
                                        case MessageType.Choke:
                                            ChokeMessageReceived(this, (ChokeMessage)message);
                                            break;
                                        case MessageType.Unchoke:
                                            UnchokeMessageReceived(this, (UnchokeMessage)message);
                                            break;
                                        case MessageType.Interested:
                                            InterestedMessageReceived(this, (InterestedMessage)message);
                                            break;
                                        case MessageType.NotInterested:
                                            NotInterestedMessageReceived(this, (NotInterestedMessage)message);
                                            break;
                                        case MessageType.Have:
                                            HaveMessageReceived(this, (HaveMessage)message);
                                            break;
                                        case MessageType.BitField:
                                            BitfieldMessageReceived(this, (BitfieldMessage)message);
                                            break;
                                        case MessageType.Request:
                                            RequestMessageReceived(this, (RequestMessage)message);
                                            break;
                                        case MessageType.Piece:
                                            PieceMessageReceived(this, (PieceMessage)message);
                                            break;
                                        case MessageType.Cancel:
                                            CancelMessageReceived(this, (CancelMessage)message);
                                            break;
                                        case MessageType.Port:
                                            PortMessageReceived(this, (PortMessage)message);
                                            break;
                                        case MessageType.SuggestPiece:
                                            SuggestPieceMessageReceived(this, (SuggestPieceMessage)message);
                                            break;
                                        case MessageType.HaveAll:
                                            HaveAllMessageReceived(this, (HaveAllMessage)message);
                                            break;
                                        case MessageType.HaveNone:
                                            HaveNoneMessageReceived(this, (HaveNoneMessage)message);
                                            break;
                                        case MessageType.RejectRequest:
                                            RejectRequestMessageReceived(this, (RejectRequestMessage)message);
                                            break;
                                        case MessageType.AllowedFast:
                                            AllowedFastMessageReceived(this, (AllowedFastMessage)message);
                                            break;
                                        case MessageType.ExtendedList:
                                            ExtendedListMessageReceived(this, (ExtendedListMessage)message);
                                            break;
                                    }
                                }

                                if (!_socket.ReceiveAsync(e))
                                {
                                    rcvEventArg_Completed(this, e);
                                }
                            }

                            _timer.Start();
                        }
                        else
                        {
                            ReceiveFail(this, null);
                        }
                    }
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    ReceiveFail(this, null);
                }
                catch (ObjectDisposedException ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    ReceiveFail(this, null);
                }
            }
        }

        #endregion

        #region Send Methods

        /// <summary>
        /// Send handshake message asynchronously
        /// </summary>
        /// <param name="infoHash">the info hash of requested file</param>
        /// <param name="peerId">the peer id</param>
        public void SendHandshakeMessageAsync(byte[] infoHash, byte[] peerId)
        {
            HandshakeMessage handshakeMessage = new HandshakeMessage(infoHash, peerId);
            handshakeMessage.SupportExtension = Setting.AllowExtension;
            handshakeMessage.SupportDht = Setting.AllowDht;
            handshakeMessage.SupportFastPeer = Setting.AllowFastPeer;
            handshakeMessage.SupportPeerExchange = Setting.AllowPeerExchange;
            SendMessageAsync(handshakeMessage);
        }

        /// <summary>
        /// Send keep alive message asynchronously
        /// </summary>
        public void SendKeepAliveMessageAsync()
        {
            SendMessageAsync(KeepAliveMessage.Instance);
        }
        
        /// <summary>
        /// Send choke message asynchronously
        /// </summary>
        public void SendChokeMessageAsync()
        {
            SendMessageAsync(ChokeMessage.Instance);
            AmChoking = true;
        }

        /// <summary>
        /// Send unchoke message asynchronously
        /// </summary>
        public void SendUnchokeMessageAsync()
        {
            SendMessageAsync(UnchokeMessage.Instance);
            AmChoking = false;
        }

        /// <summary>
        /// Send interested message asynchronously
        /// </summary>
        public void SendInterestedMessageAsync()
        {
            SendMessageAsync(InterestedMessage.Instance);
            AmInterested = true;
        }

        /// <summary>
        /// Send not interested message asynchronously
        /// </summary>
        public void SendNotInterestedMessageAsync()
        {
            SendMessageAsync(NotInterestedMessage.Instance);
            AmInterested = false;
        }

        /// <summary>
        /// Send have message asynchronously
        /// </summary>
        /// <param name="index">the index of piece</param>
        public void SendHaveMessageAsync(int index)
        {
            HaveMessage message = new HaveMessage(index);
            SendMessageAsync(message);
        }

        /// <summary>
        /// Send bitfield message asynchronously
        /// </summary>
        /// <param name="bitfield">The bitfield of the requested file</param>
        public void SendBitfieldMessageAsync(bool[] bitfield)
        {
            BitfieldMessage message = new BitfieldMessage(bitfield);
            SendMessageAsync(message);
        }

        /// <summary>
        /// Send request message asynchronously
        /// </summary>
        /// <param name="index">the index of piece</param>
        /// <param name="begin">the begin of piece</param>
        /// <param name="length">the length of piece</param>
        public void SendRequestMessageAsync(int index, int begin, int length)
        {
            RequestMessage message = new RequestMessage(index, begin, length);
            SendMessageAsync(message);
        }

        /// <summary>
        /// Send piece message asynchronously
        /// </summary>
        /// <param name="index">the index of piece</param>
        /// <param name="begin">the begin of piece</param>
        /// <param name="block">the block of piece</param>
        public void SendPieceMessageAsync(int index, int begin, byte[] block)
        {
            PieceMessage message = new PieceMessage(index, begin, block);
            SendMessageAsync(message);
        }

        /// <summary>
        /// Send cancel message asynchronously
        /// </summary>
        /// <param name="index">the index of piece</param>
        /// <param name="begin">the begin of piece</param>
        /// <param name="length">the length of piece</param>
        public void SendCancelMessageAsync(int index, int begin, int length)
        {
            CancelMessage message = new CancelMessage(index, begin, length);
            SendMessageAsync(message);
        }

        /// <summary>
        /// Send port message asynchronously
        /// </summary>
        /// <param name="port">the listenning port of dht</param>
        public void SendPortMessageAsync(ushort port)
        {
            PortMessage message = new PortMessage(port);
            SendMessageAsync(message);
        }

        /// <summary>
        /// Send allowed fast message asynchronously
        /// </summary>
        /// <param name="index">the index of piece</param>
        public void SendSuggestPieceMessage(int index)
        {
            SuggestPieceMessage message = new SuggestPieceMessage(index);
            SendMessageAsync(message);
        }

        /// <summary>
        /// Send have all message asynchronously
        /// </summary>
        public void SendHaveAllMessageAsync()
        {
            SendMessageAsync(HaveAllMessage.Instance);
        }

        /// <summary>
        /// Send have none message asynchronously
        /// </summary>
        public void SendHaveNoneMessageAsync()
        {
            SendMessageAsync(HaveNoneMessage.Instance);
        }

        /// <summary>
        /// Send reject request message asynchronously
        /// </summary>
        /// <param name="index">the index of piece</param>
        /// <param name="begin">the begin of piece</param>
        /// <param name="length">the length of piece</param>
        public void SendRejectRequestMessageAsync(int index, int begin, int length)
        {
            RejectRequestMessage message = new RejectRequestMessage(index, begin, length);
            SendMessageAsync(message);
        }

        /// <summary>
        /// Send allowed fast message asynchronously
        /// </summary>
        /// <param name="index">the index of piece</param>
        public void SendAllowedFastMessageAsync(int index)
        {
            AllowedFastMessage message = new AllowedFastMessage(index);
            SendMessageAsync(message);
        }
        
        /// <summary>
        /// Send extended list message asynchronously
        /// </summary>
        public void SendExtendedListMessageAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Send message asynchronously
        /// </summary>
        /// <param name="message">the message carries network information</param>
        private void SendMessageAsync(Message message)
        {
            try
            {
                if (IsConnected)
                {
                    MessageSending(this, message);
                    SocketAsyncEventArgs sndEventArg = new SocketAsyncEventArgs();
                    byte[] buffer = message.GetByteArray();
                    sndEventArg.SetBuffer(buffer, 0, buffer.Length);
                    sndEventArg.Completed += sndEventArg_Completed;
                    if (!_socket.SendAsync(sndEventArg))
                    {
                        sndEventArg_Completed(this, sndEventArg);
                    }
                }
                else
                {
                    SendFail(this, null);
                }
            }
            catch (NullReferenceException)
            {
                SendFail(this, null);
            }
        }

        void sndEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            lock (this)
            {
                try
                {
                    if (IsConnected)
                    {
                        _timer.Stop();
                        //If local socket sends message to remote socket wrong, close and dispose socket.
                        if (e.SocketError != SocketError.Success)
                        {
                            SendFail(this, null);
                        }
                        else
                        {
                            _timer.Start();
                        }
                    }
                    e.Dispose();
                }
                catch (NullReferenceException)
                {
                    SendFail(this, null);
                }
            }
        }

        #endregion

        #region Disconnect Methods

        /// <summary>
        /// Disconnect a remote peer
        /// </summary>
        public void Disconnect()
        {
            lock (this)
            {
                if (IsConnected)
                {
                    IsConnected = false;
                    IsHandshaked = false;
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Disconnect(false);
                }
            }
        }

        #endregion

        #region Handle pieces Methods

        /// <summary>
        /// Initial bitfield
        /// </summary>
        /// <param name="length">The length of bit field</param>
        public void InitialBitfield(int length)
        {
            _bitfield = new bool[length];
            Array.Clear(_bitfield, 0, length);
        }

        /// <summary>
        /// Clear bitfield
        /// </summary>
        public void ClearBitfield()
        {
            Debug.Assert(_bitfield != null);
            Array.Clear(_bitfield, 0, _bitfield.Length);
        }

        /// <summary>
        /// Set bitfield
        /// </summary>
        public void SetBitfield()
        {
            Debug.Assert(_bitfield != null);
            Array.ForEach(_bitfield, b => b = true);
        }

        /// <summary>
        /// Copy from bitfield
        /// </summary>
        /// <param name="bitfield">The bitfield</param>
        public void CopyFromBitfield(bool[] bitfield)
        {
            Debug.Assert(bitfield != null);
            Debug.Assert(_bitfield != null);
            Debug.Assert(_bitfield.Length == bitfield.Length);
            lock (_bitfield)
            {
                Array.Copy(bitfield, _bitfield, _bitfield.Length);
            }
        }

        /// <summary>
        /// Set the index-th bit
        /// </summary>
        /// <param name="index">The index of bit</param>
        public void SetBit(int index)
        {
            Debug.Assert(_bitfield != null);
            lock(_bitfield)
            {
                _bitfield[index] = true;
            }
        }

        /// <summary>
        /// Get the bitfield
        /// </summary>
        /// <returns>Return the bitfield</returns>
        public bool[] GetBitfield()
        {
            bool[] result = new bool[_bitfield.Length];
            Debug.Assert(_bitfield != null);
            lock (_bitfield)
            {
                Array.Copy(_bitfield, result, _bitfield.Length);
            }
            return result;
        }

        public int[] GetRequestedIndexes()
        {
            int[] result;
            lock (_requestedIndexes)
            {
                result = _requestedIndexes.ToArray();
            }
            return result;
        }

        public void AddRequestedIndex(int index)
        {
            lock (_requestedIndexes)
            {
                _requestedIndexes.Add(index);
            }
        }

        public void RemoveRequestedIndex(int index)
        {
            lock (_requestedIndexes)
            {
                _requestedIndexes.Remove(index);
            }
        }

        #endregion

        /// <summary>
        /// Reset the timer
        /// </summary>
        public void ResetTimer()
        {
            _timer.Stop();
            _timer.Start();
        }

        public override string ToString()
        {
            string result = string.Format("{0}:{1}[{2}]", Host, Port, PeerId);
            return result;
        }

        public override bool Equals(object obj)
        {
            Peer other = obj as Peer;
            if (other != null)
            {
                return Host == other.Host;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public void Dispose()
        {
            lock (this)
            {
                Console.WriteLine("Dispose");
                if (_socket != null)
                {
                    _socket.Dispose();
                    _socket = null;
                }
                
                if (_timer != null)
                {
                    _timer.Dispose();
                    _timer = null;
                }
            }
        }

        #endregion

        public bool Equals(Peer other)
        {
            return Host.Equals(other.Host) && Port.Equals(other.Port);
        }
    }
}