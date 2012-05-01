using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ZeraldotNet.LibBitTorrent.Messages;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// The peer that handles messages
    /// </summary>
    public class Peer
    {
        #region Fields

        private Socket _socket;

        private Queue<Message> _messageQueue;

        private BufferPool _bufferPool;

        private int _sumLength;

        private bool[] _booleans;

        #endregion

        #region Properties

        public string Host { get; set; }

        public int Port { get; set; }

        public byte[] InfoHash { get; set; }

        public byte[] LocalPeerId { get; set; }

        public bool AmChoking { get; set; }

        public bool AmInterested { get; set; }

        public bool PeerChoking { get; set; }

        public bool PeerInterested { get; set; }

        #endregion

        #region Events

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
            _sumLength = 0;
            _messageQueue = new Queue<Message>();
            _bufferPool = new BufferPool(Setting.BufferPoolCapacity);
            AmChoking = true;
            AmInterested = false;
            PeerChoking = true;
            PeerInterested = false;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Connect a remote peer
        /// </summary>
        public void Connect()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            _socket.Connect(Host, Port);
        }

        /// <summary>
        /// Disconnect a remote peer
        /// </summary>
        public void Disconnect()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Disconnect(false);
        }
        
        /// <summary>
        /// Receive the messages from a remote peer
        /// </summary>
        public void ReceiveAsnyc()
        {
            _sumLength = 0;
            byte[] buffer = new byte[Setting.BufferSize];
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(e_Completed);
            e.SetBuffer(buffer, 0, Setting.BufferSize);
            _socket.ReceiveAsync(e);
        }

        private void e_Completed(object sender, SocketAsyncEventArgs e)
        {
            byte[] readBytes = e.Buffer;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                _sumLength += e.BytesTransferred;
                _bufferPool.Write(e.Buffer, 0, e.BytesTransferred);

                int index = 0;

                for (int i = 0; i < e.BytesTransferred - 2; i += 2)
                {
                   // Console.Write("{0}:{1} {2:D3}:{3:D3}|{4}|{3:X2}   ", Host, Port, index++, readBytes[i], (char)readBytes[i]);
                    //Console.WriteLine("{0:D3}:{1:D3}|{2}|{1:X2}", index++, readBytes[i + 1], (char)readBytes[i + 1]);
                }

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
                while ((message = Message.Parse(_bufferPool, _booleans.Length)) != null)
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
                }

                SocketAsyncEventArgs asyncEventArgs = new SocketAsyncEventArgs();
                asyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(e_Completed);
                byte[] buffer = new byte[Setting.BufferSize];
                asyncEventArgs.SetBuffer(buffer, 0, Setting.BufferSize);
                _socket.ReceiveAsync(asyncEventArgs);
            }
        }
        
        public void SendHandshakeMessage(byte[] infoHash, byte[] peerId)
        {
            HandshakeMessage handshakeMessage = new HandshakeMessage(infoHash, peerId);
            _socket.Send(handshakeMessage.Encode());
        }

        public void SendKeepAliveMessage()
        {
            _socket.Send(KeepAliveMessage.Instance.Encode());
        }

        public void SendChokeMessage()
        {
            _socket.Send(ChokeMessage.Instance.Encode());
            AmChoking = true;
        }

        public void SendUnchokeMessage()
        {
            _socket.Send(UnchokeMessage.Instance.Encode());
            AmChoking = false;
        }

        public void SendInterestedMessage()
        {
            _socket.Send(InterestedMessage.Instance.Encode());
            AmInterested = true;
        }

        public void SendNotInterestedMessage()
        {
            _socket.Send(NotInterestedMessage.Instance.Encode());
            AmInterested = false;
        }

        public void SendHaveMessage(int index)
        {
            HaveMessage message = new HaveMessage(index);
            _socket.Send(message.Encode());
        }

        public void SendBitfieldMessage(bool[] booleans)
        {
            BitfieldMessage bitfieldMessage = new BitfieldMessage(booleans);
            _socket.Send(bitfieldMessage.Encode());
        }

        public void SendRequestMessage(int index, int begin, int length)
        {
            RequestMessage message = new RequestMessage(index, begin, length);
            _socket.Send(message.Encode());
        }

        public void SendPieceMessage(int index, int begin, byte[] block)
        {
            PieceMessage message = new PieceMessage(index, begin, block);
            _socket.Send(message.Encode());
        }

        public void SendCancelMessage(int index, int begin, int length)
        {
            CancelMessage message = new CancelMessage(index, begin, length);
            _socket.Send(message.Encode());
        }

        public void InitialBooleans(int booleansLength)
        {
            _booleans = new bool[booleansLength];
            Array.Clear(_booleans, 0, booleansLength);
        }

        public void SetBooleans(bool[] booleans)
        {
            _booleans = booleans;
        }

        public void SetBoolean(int index)
        {
            _booleans[index] = true;
        }

        public void ResetBoolean(int index)
        {
            _booleans[index] = false;
        }

        public Message GetNewMessage()
        {
            return _messageQueue.Dequeue();
        }

        public override string ToString()
        {
            return _socket.RemoteEndPoint.ToString();
        }

        #endregion
    }
}