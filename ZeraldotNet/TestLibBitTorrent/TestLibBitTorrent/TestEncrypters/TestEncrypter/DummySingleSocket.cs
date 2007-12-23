using System;
using System.Collections.Generic;
using System.Net.Sockets;
using ZeraldotNet.LibBitTorrent.Encrypters;
using ZeraldotNet.LibBitTorrent.RawServers;

namespace ZeraldotNet.TestLibBitTorrent.TestEncrypter
{
    public class DummySingleSocket : ISingleSocket
    {
        private bool isFlushed;

        private bool closed;

        private readonly List<byte[]> data;

        public bool Closed
        {
            get { return this.closed; }
            set { this.closed = value; }
        }

        public DummySingleSocket()
        {
            closed = false;
            data = new List<byte[]>();
            isFlushed = true;
        }

        public void SetFlushed(bool value)
        {
            this.isFlushed = value;
        }

        public byte[] Pop()
        {
            List<byte> result = new List<byte>();
            foreach (byte[] b in data)
            {
                result.AddRange(b);

            }
            data.Clear();
            return result.ToArray();
        }

        #region ISingleSocket Members

        public void Close()
        {
            closed = true;
        }

        public void Close(bool closing)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IEncrypter Handler
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

        public string IP
        {
            get { return "fake.ip"; }
        }

        public bool IsConnected
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

        public bool IsFlushed
        {
            get { return this.isFlushed; }
        }

        public DateTime LastHit
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

        public void ShutDown(SocketShutdown how)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Socket Socket
        {
            get
            {
                return null;
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void TryWrite()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(byte[] bytes)
        {
            this.data.Add(bytes);
        }

        #endregion
    }
}
