﻿using System;
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
    public class Peer : IDisposable
    {
        #region Fields

        private Socket _socket;

        private BufferPool _bufferPool;

        private bool[] _bitfield;

        private List<int> _requestedIndexes;

        private Timer _timer;

        #endregion

        #region Properties

        public string Host { get; set; }

        public int Port { get; set; }

        /// <summary>
        /// The SHA1 hash of the torrent
        /// </summary>
        public byte[] InfoHash { get; set; }

        /// <summary>
        /// The peer id
        /// </summary>
        public byte[] PeerId { get; set; }

        /// <summary>
        /// The flag that represents whether peer support DHT.
        /// </summary>
        public bool IsDht { get; set; }

        /// <summary>
        /// The flag that represents whether peer support peer exchange.
        /// </summary>
        public bool IsPeerExchange { get; set; }

        /// <summary>
        /// The flag that represents whether peer support fast extension.
        /// </summary>
        public bool IsFastExtension { get; set; }

        public bool AmChoking { get; set; }

        public bool AmInterested { get; set; }

        public bool PeerChoking { get; set; }

        public bool PeerInterested { get; set; }

        public bool IsConnected { get; private set; }

        #endregion

        #region Events

        public event EventHandler OnConnected;
        public event EventHandler ConnectFail;
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

        public Peer()
        {
            Initial();
            IsConnected = false;
        }
        
        public Peer(Socket socket) : this()
        {
            Initial();
            _socket = socket;
            IPEndPoint ipEndPoint = (IPEndPoint)socket.RemoteEndPoint;
            Host = ipEndPoint.Address.ToString();
            Port = ipEndPoint.Port;
            IsConnected = true;
        }

        private void Initial()
        {
            _bufferPool = new BufferPool(Setting.BufferPoolCapacity);
            _requestedIndexes = new List<int>();
            AmChoking = true;
            AmInterested = false;
            PeerChoking = true;
            PeerInterested = false;
            _timer = new Timer(Setting.PeerAliveInterval);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
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
            catch(SocketException)
            {
                Debug.Assert(ConnectFail != null);
                IsConnected = false;
                ConnectFail(this, null);
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
        /// Receive the messages from a remote peer
        /// </summary>
        public void ReceiveAsnyc()
        {
            byte[] buffer = new byte[Setting.BufferSize];
            SocketAsyncEventArgs rcvEventArg = new SocketAsyncEventArgs();
            rcvEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(rcvEventArg_Completed);
            rcvEventArg.SetBuffer(buffer, 0, Setting.BufferSize);
            if (!_socket.ReceiveAsync(rcvEventArg))
            {
                rcvEventArg_Completed(this, rcvEventArg);
            }
        }

        private void rcvEventArg_Completed(object sender, SocketAsyncEventArgs e)
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
                        switch (message.Type)
                        {
                            case MessageType.Handshake:
                                HandshakeMessageReceived(this, (HandshakeMessage) message);
                                break;
                            case MessageType.KeepAlive:
                                KeepAliveMessageReceived(this, (KeepAliveMessage) message);
                                break;
                            case MessageType.Choke:
                                ChokeMessageReceived(this, (ChokeMessage) message);
                                break;
                            case MessageType.Unchoke:
                                UnchokeMessageReceived(this, (UnchokeMessage) message);
                                break;
                            case MessageType.Interested:
                                InterestedMessageReceived(this, (InterestedMessage) message);
                                break;
                            case MessageType.NotInterested:
                                NotInterestedMessageReceived(this, (NotInterestedMessage) message);
                                break;
                            case MessageType.Have:
                                HaveMessageReceived(this, (HaveMessage) message);
                                break;
                            case MessageType.BitField:
                                BitfieldMessageReceived(this, (BitfieldMessage) message);
                                break;
                            case MessageType.Request:
                                RequestMessageReceived(this, (RequestMessage) message);
                                break;
                            case MessageType.Piece:
                                PieceMessageReceived(this, (PieceMessage) message);
                                break;
                            case MessageType.Cancel:
                                CancelMessageReceived(this, (CancelMessage) message);
                                break;
                            case MessageType.Port:
                                PortMessageReceived(this, (PortMessage) message);
                                break;
                            case MessageType.SuggestPiece:
                                SuggestPieceMessageReceived(this, (SuggestPieceMessage) message);
                                break;
                            case MessageType.HaveAll:
                                HaveAllMessageReceived(this, (HaveAllMessage) message);
                                break;
                            case MessageType.HaveNone:
                                HaveNoneMessageReceived(this, (HaveNoneMessage) message);
                                break;
                            case MessageType.RejectRequest:
                                RejectRequestMessageReceived(this, (RejectRequestMessage) message);
                                break;
                            case MessageType.AllowedFast:
                                AllowedFastMessageReceived(this, (AllowedFastMessage) message);
                                break;
                            case MessageType.ExtendedList:
                                ExtendedListMessageReceived(this, (ExtendedListMessage) message);
                                break;
                        }
                        message.Handle(this);
                    }

                    if (!_socket.ReceiveAsync(e))
                    {
                        rcvEventArg_Completed(this, e);
                    }
                }
            }
            else
            {
                ReceiveFail(this, null);
            }
            _timer.Start();
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
        /// Send bit field message asynchronously
        /// </summary>
        /// <param name="bitfield">The bit field of the requested file</param>
        public void SendBitfieldMessageAsync(bool[] bitfield)
        {
            BitfieldMessage message = new BitfieldMessage(bitfield);
            SendMessageAsync(message);
        }

        public void SendRequestMessageAsync(int index, int begin, int length)
        {
            RequestMessage message = new RequestMessage(index, begin, length);
            SendMessageAsync(message);
        }

        public void SendPieceMessageAsync(int index, int begin, byte[] block)
        {
            PieceMessage message = new PieceMessage(index, begin, block);
            SendMessageAsync(message);
        }

        public void SendCancelMessageAsync(int index, int begin, int length)
        {
            CancelMessage message = new CancelMessage(index, begin, length);
            SendMessageAsync(message);
        }

        public void SendPortMessageAsync(ushort port)
        {
            PortMessage message = new PortMessage(port);
            SendMessageAsync(message);
        }

        private void SendMessageAsync(Message message)
        {
            MessageSending(this, message);
            _timer.Stop();
            SocketAsyncEventArgs sndEventArg = new SocketAsyncEventArgs();
            byte[] buffer = message.Encode();
            sndEventArg.SetBuffer(buffer, 0, buffer.Length);
            sndEventArg.Completed += sndEventArg_Completed;
            if (!_socket.SendAsync(sndEventArg))
            {
                sndEventArg_Completed(this, sndEventArg);
            }
            _timer.Start();
        }

        void sndEventArg_Completed(object sender, SocketAsyncEventArgs e)
        {
            //If local socket sends message to remote socket wrong, close and dispose socket.
            if (e.SocketError != SocketError.Success)
            {
                SendFail(this, null);
            }
            e.Dispose();
        }

        #endregion

        #region Disconnect Methods

        /// <summary>
        /// Disconnect a remote peer
        /// </summary>
        public void Disconnect()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Disconnect(false);
            IsConnected = false;
        }

        #endregion

        #region Handle pieces Methods

        public void InitialBooleans(int booleansLength)
        {
            _bitfield = new bool[booleansLength];
            Array.Clear(_bitfield, 0, booleansLength);
        }

        public void SetBitfield(bool[] bitfield)
        {
            Debug.Assert(bitfield != null);
            _bitfield = bitfield;
        }

        public void SetBitfield(int index)
        {
            Debug.Assert(_bitfield != null);
            _bitfield[index] = true;
        }

        public bool[] GetBitfield()
        {
            Debug.Assert(_bitfield != null);
            return _bitfield;
        }

        public int[] GetRequestedIndexes()
        {
            return _requestedIndexes.ToArray();
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

        public override string ToString()
        {
            string result = string.Format("{0}:{1}", Host, Port);
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
            return Host.GetHashCode();
        }

        public void Dispose()
        {
            if (_socket != null)
            {
                _socket.Dispose();
            }
            _timer.Dispose();
        }

        #endregion
    }
}