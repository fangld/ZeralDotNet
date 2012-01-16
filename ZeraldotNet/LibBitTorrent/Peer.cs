using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using ZeraldotNet.LibBitTorrent.Messages;

namespace ZeraldotNet.LibBitTorrent
{
    public class Peer
    {
        #region Fields

        private Socket _socket;

        private Queue<Message> _messageQueue;

        private BufferPool _bufferPool;

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
            _messageQueue = new Queue<Message>();
            _bufferPool = new BufferPool(Setting.BufferPool);
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

            //socket.Send(new byte[] {0, 0, 0, 3, 5, 255, 240});
            //socket.Send(new byte[] {0, 0, 0, 5, 4, 0, 0, 0, 7});
            //socket.Send(new byte[] {0, 0, 0, 1, 1});
            //socket.Send(new byte[] {0, 0, 0, 1, 2});

            //socket.Send(new byte[] {0, 0, 0, 13, 6, 0, 0, 0, 6, 0, 0, 128, 0, 0, 0, 0, 0});
            //request: <len=0013><id=6><index><begin><length> 


        }

        public void Handle()
        {
            
        }

        public void Read()
        {
            byte[] bytes = new byte[Setting.BufferSize];
            int readLength;
            int sumLength = 0;
            int index = 0;

            SocketAsyncEventArgs e = new SocketAsyncEventArgs();
            e.Completed += new EventHandler<SocketAsyncEventArgs>(e_Completed);
            e.SetBuffer(bytes, 0, Setting.BufferSize);

            //e.SetBuffer(0, Setting.BufferSize);
            //e.UserToken = new AsyncUserToken();

            _socket.ReceiveAsync(e);

            //do
            //{
            //    readLength = _socket.Receive(bytes, SocketFlags.None);
            //    sumLength += readLength;
            //    Console.WriteLine("readLength:{0}", readLength);
            //    for (int i = 0; i < readLength - 2; i += 2)
            //    {
            //        Console.Write("i:{0:D3}: , b:{2:D3}|{2:X2}   ", index++, (char)bytes[i], bytes[i]);
            //        Console.WriteLine("i:{0:D3}: , b:{2:D3}|{2:X2}", index++, (char)bytes[i + 1], bytes[i + 1]);
            //        //index += 2;
            //    }

            //    //socket.Send(new byte[] { 0, 0, 0, 5, 4, 0, 0, 0, 7 });
            //    //socket.Send(new byte[] { 0, 0, 0, 1, 1 });
            //    //socket.Send(new byte[] { 0, 0, 0, 1, 2 });
            //    //ThreadStaticAttribute.

            //    //socket.Send(new byte[] { 0, 0, 0, 13, 6, 0, 0, 0, 6, 0, 0, 128, 0, 0, 0, 0, 0 });

            //    if (readLength % 2 == 1)
            //    {
            //        Console.WriteLine("i:{0:D3}: , b:{2:D3}|{2:X2}", index++, (char)bytes[readLength - 1], bytes[readLength - 1]);
            //    }
            //    else if (readLength != 0)
            //    {
            //        Console.Write("i:{0:D3}: , b:{2:D3}|{2:X2}   ", index++, (char)bytes[readLength - 2], bytes[readLength - 2]);
            //        Console.WriteLine("i:{0:D3}: , b:{2:D3}|{2:X2}", index++, (char)bytes[readLength - 1], bytes[readLength - 1]);
            //    }

            //    if (sumLength == 89)
            //    {
            //        _socket.Send(new byte[] { 0, 0, 0, 3, 5, 255, 240 });
            //        _socket.Send(new byte[] { 0, 0, 0, 1, 1 });
            //    }

            //    //if (sumLength == 94)
            //    //{
            //    //    socket.Send()
            //    //}

            //    //Console.WriteLine();
            //    //nsole.WriteLine();
            //    //Console.ReadLine();
            //    Console.WriteLine("SumLength:{0}", sumLength);
            //} while (readLength != 0);
        }

        void e_Completed(object sender, SocketAsyncEventArgs e)
        {
            byte[] readBytes = e.Buffer;
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                _bufferPool.Write(e.Buffer, 0, e.BytesTransferred);

                int index = 0;
                for (int i = 0; i < e.BytesTransferred - 2; i += 2)
                {
                    Console.Write("i:{0:D3}: , b:{2:D3}|{2:X2}   ", index++, (char)readBytes[i], readBytes[i]);
                    Console.WriteLine("i:{0:D3}: , b:{2:D3}|{2:X2}", index++, (char)readBytes[i + 1], readBytes[i + 1]);
                }

                if (e.BytesTransferred % 2 == 1)
                {
                    Console.WriteLine("i:{0:D3}: , b:{2:D3}|{2:X2}", index++, (char)readBytes[e.BytesTransferred - 1], readBytes[e.BytesTransferred - 1]);
                }
                else if (e.BytesTransferred != 0)
                {
                    Console.Write("i:{0:D3}: , b:{2:D3}|{2:X2}   ", index++, (char)readBytes[e.BytesTransferred - 2], readBytes[e.BytesTransferred - 2]);
                    Console.WriteLine("i:{0:D3}: , b:{2:D3}|{2:X2}", index++, (char)readBytes[e.BytesTransferred - 1], readBytes[e.BytesTransferred - 1]);
                }

                Message message;
                do
                {
                    message = MessageDecoder.Decode(_bufferPool);
                    _messageQueue.Enqueue(message);
                } while (message != null);

                SocketAsyncEventArgs asyncEventArgs = new SocketAsyncEventArgs();
                asyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(e_Completed);
                byte[] bytes = new byte[Setting.BufferSize];
                asyncEventArgs.SetBuffer(bytes, 0, Setting.BufferSize);
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
