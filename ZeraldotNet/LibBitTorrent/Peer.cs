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
    public class Peer
    {
        #region Fields

        private Socket _socket;

        private Queue<Message> _messageQueue;

        private BufferPool _bufferPool;

        private int _sumLength;

        #endregion

        #region Properties

        public string Host { get; set; }

        public int Port { get; set; }

        public byte[] InfoHash { get; set; }

        public byte[] PeerId { get; set; }

        public bool AmChoking { get; set; }

        public bool AmInterested { get; set; }

        public bool PeerChoking { get; set; }

        public bool PeerInterested { get; set; }

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

        public void Connect()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            _socket.Connect(Host, Port);
        }

        public void Handle()
        {

        }

        public void ReceiveAsnyc()
        {
            _sumLength = 0;
            byte[] buffer = new byte[Setting.BufferSize];
            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(e_Completed);

            e.SetBuffer(buffer, 0, Setting.BufferSize);

            _socket.ReceiveAsync(e);
        }

        public void Receive()
        {
            byte[] bytes = new byte[Setting.BufferSize];
            int readLength;
            int index = 0;

            do
            {
                readLength = _socket.Receive(bytes, SocketFlags.None);
                Console.WriteLine("readLength:{0}", readLength);
                for (int i = 0; i < readLength - 2; i += 2)
                {
                    Console.Write("{0}:{1} {2:D3}:{3:D3}|{3:X2}   ", Host, Port, index++, bytes[i]);
                    Console.WriteLine("{0:D3}:{1:D3}|{1:X2}", index++, bytes[i + 1]);
                }

                if (readLength % 2 == 1)
                {
                    Console.WriteLine("{0}:{1} {2:D3}:{3:D3}|{3:X2}   ", Host, Port, index++, bytes[readLength - 1]);
                }
                else if (readLength != 0)
                {
                    Console.Write("{0}:{1} {2:D3}:{3:D3}|{3:X2}   ", Host, Port, index++, bytes[readLength - 2]);
                    Console.WriteLine("{0:D3}:{1:D3}|{1:X2}", index++, bytes[readLength - 1]);
                }

                Console.WriteLine("SumLength:{0}", _sumLength);
            } while (true); //readLength != 0);
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
                    Console.Write("{0}:{1} {2:D3}:{3:D3}|{4}|{3:X2}   ", Host, Port, index++, readBytes[i], (char)readBytes[i]);
                    Console.WriteLine("{0:D3}:{1:D3}|{2}|{1:X2}", index++, readBytes[i + 1], (char)readBytes[i + 1]);
                }

                if (e.BytesTransferred%2 == 1)
                {
                    Console.WriteLine("{0}:{1} {2:D3}:{3:D3}|{4}|{3:X2}   ", Host, Port, index++, readBytes[e.BytesTransferred - 1], (char)readBytes[e.BytesTransferred - 1]);
                }
                else if (e.BytesTransferred != 0)
                {
                    Console.Write("{0}:{1} {2:D3}:{3:D3}|{4}|{3:X2}   ", Host, Port, index++, readBytes[e.BytesTransferred - 2], (char)readBytes[e.BytesTransferred - 2]);
                    Console.WriteLine("{0:D3}:{1:D3}|{2}|{1:X2}", index++, readBytes[e.BytesTransferred - 1], (char)readBytes[e.BytesTransferred - 1]);
                }

                Console.WriteLine("SumLength:{0}", _sumLength);

                Message message;
                do
                {
                    message = Message.Parse(_bufferPool);
                    _messageQueue.Enqueue(message);
                } while (message != null);

                SocketAsyncEventArgs asyncEventArgs = new SocketAsyncEventArgs();
                asyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(e_Completed);
                byte[] buffer = new byte[Setting.BufferSize];
                asyncEventArgs.SetBuffer(buffer, 0, Setting.BufferSize);
                _socket.ReceiveAsync(asyncEventArgs);
            }
        }

        public void SendBitfieldMessage()
        {
            //send bitfield message
            bool[] booleans = new bool[12];
            Array.Clear(booleans, 0, booleans.Length);
            BitfieldMessage bitfieldMessage = new BitfieldMessage(booleans);
            _socket.Send(bitfieldMessage.Encode());
        }

        public void SendHandshakeMessage()
        {
            HandshakeMessage handshakeMessage = new HandshakeMessage(InfoHash, PeerId);
            _socket.Send(handshakeMessage.Encode());
        }

        public void SendInterestedMessage()
        {
            //interested message
            InterestedMessage message = new InterestedMessage();
            _socket.Send(message.Encode());
        }

        public void SendUnchokeMessage()
        {
            //unchoke message
            UnchokeMessage message = new UnchokeMessage();
            _socket.Send(message.Encode());
        }

        public Message GetNewMessage()
        {
            return _messageQueue.Dequeue();
        }

        #endregion
    }
}