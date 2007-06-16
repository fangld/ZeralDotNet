using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ZeraldotNet.LibBitTorrent
{
    public class SingleSocket : ISingleDownload
    {
        private RawServer rawServ;

        public RawServer RawServ
        {
            set { rawServ = value; }
        }

        private Socket sock;

        public Socket Sock
        {
            get { return this.sock; }
            set { this.sock = value; }
        }

        private DateTime lashHit;

        public DateTime LashHit
        {
            get { return this.lashHit; }
            set { this.lashHit = value; }
        }

        private bool connected;

        public bool Connected
        {
            get { return this.connected; }
            set { this.connected = value; }
        }

        private List<byte[]> buffer;

        private Encrpyter handler;

        public Encrpyter Handler
        {
            get { return this.handler; }
            set { this.handler = value; }
        }

        public SingleSocket(RawServer rawServ, Socket sock, Encrpyter handler)
        {
            RawServ = rawServ;
            Sock = sock;
            Handler = handler;

            buffer = new List<byte[]>();
            lashHit = DateTime.Now;
            connected = false;
        }

        public string IP
        {
            get
            {
                try
                {
                    return ((IPEndPoint)sock.RemoteEndPoint).Address.ToString();
                }
                catch (SocketException)
                {
                    return "无连接";
                }
            }
        }

        public void Close()
        {
            Close(false);
        }

        public void Close(bool closing)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] bytes)
        {
            buffer.Add(bytes);
            if (buffer.Count == 1)
                TryWrite();
        }

        public void TryWrite()
        {
            if (connected)
            {
                try
                {
                    while (buffer.Count > 0)
                    {
                        byte[] bytes = buffer[0];
                        int amount = sock.Send(bytes);
                        if (amount != bytes.Length)
                        {
                            if (amount != 0)
                            {
                                byte[] two = new byte[bytes.Length - amount];
                                Buffer.BlockCopy(bytes, amount, two, 0, two.Length);
                                buffer[0] = two;
                            }
                            break;
                        }
                        buffer.RemoveAt(0);
                    }
                }

                catch (SocketException sockEx)
                {
                    if (sockEx.ErrorCode != 10035) //WSAE would block
                    {
                        rawServ.AddToDeadFromWrite(this);
                        return;
                    }
                }

                if (buffer.Count == 0)
                {
                    rawServ.Poll.Register(sock, PollMode.PollIn);
                }
                else
                {
                    rawServ.Poll.Register(sock, PollMode.PollOut);
                }

            }
        }



        #region ISingleDownload Members

        public bool Snubbed
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public double Rate
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void Disconnected()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GotChoke()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GotUnchoke()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GotHave(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GotHaveBitField(bool[] have)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool GotPiece(int index, int begin, byte[] piece)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
