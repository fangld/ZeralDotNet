using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZeraldotNet.LibBitTorrent
{
    public class EncryptedConnection
    {
        private const string protocolName = "BitTorrent protocol";

        private const byte protocolNameLength = 19;

        private Encrypter encrypter;

        private SingleSocket connection;

        private byte[] id;

        public byte[] ID
        {
            get { return this.id; }
            set { this.id = value; }
        }

        private bool isLocallyInitiated;

        private bool complete;

        public bool Complete
        {
            get { return this.complete; }
            set { this.complete = value; }
        }

        private bool closed;

        public bool Closed
        {
            get { return this.closed; }
            set { this.closed = value; }
        }

        public bool IsFlushed
        {
            get { return this.connection.IsFlushed(); }
        }

        public string IP
        {
            get { return this.connection.IP; }
        }

        public bool IsLocallyInitiated
        {
            get { return this.isLocallyInitiated; }
        }

        private MemoryStream buffer;

        private int nextLength;

        private FuncDelegate nextFunction;

        public EncryptedConnection(Encrypter encrypter, SingleSocket connection, byte[] id)
        {
            this.encrypter = encrypter;
            this.connection = connection;
            this.ID = id;
            this.isLocallyInitiated = (id != null);
            this.Complete = false;
            this.Closed = false;
            this.buffer = new MemoryStream();
            this.nextLength = 1;
            this.nextFunction = new FuncDelegate(ReadHeaderLength);
            connection.Write(new byte[] { protocolNameLength });
            connection.Write(Encoding.Default.GetBytes(protocolName));
            connection.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
            connection.Write(encrypter.DownloadID);
            connection.Write(encrypter.MyID);
        }

        public NextFunction ReadHeaderLength(byte[] bytes)
        {
            if (bytes[0] != protocolName.Length)
            {
                return null;
            }
            return new NextFunction(protocolName.Length, new FuncDelegate(ReadHeader));
        }

        public NextFunction ReadHeader(byte[] bytes)
        {
            string pName = Encoding.Default.GetString(bytes, 0, protocolNameLength);
            if (pName != protocolName)
            {
                return null;
            }
            return new NextFunction(8, new FuncDelegate(ReadReserved));
        }

        public NextFunction ReadReserved(byte[] bytes)
        {
            return new NextFunction(20, new FuncDelegate(ReadDownloadID));
        }                                            

        public NextFunction ReadDownloadID(byte[] bytes)
        {
            int i;
            for (i = 0; i < 20; i++)
            {
                if (bytes[i] != encrypter.DownloadID[i])
                {
                    return null;
                }
            }

            return new NextFunction(20, new FuncDelegate(ReadPeerID));
        }

        public NextFunction ReadPeerID(byte[] bytes)
        {
            if (this.id == null)
            {
                id = bytes;
            }
            else
            {
                int i;
                for (i = 0; i < 20; i++)
                {
                    if (bytes[i] != id[i])
                    {
                        return null;
                    }
                }
            }
            complete = true;
            encrypter.Connecter.MakeConnection(this);
            return new NextFunction(4, new FuncDelegate(ReadLength));
        }

        public NextFunction ReadLength(byte[] bytes)
        {
            int length = Globals.BytesToInt32(bytes, 0);
            if (length > encrypter.MaxLength)
            {
                return null;
            }
            return new NextFunction(length, new FuncDelegate(ReadMessage));

        }

        public NextFunction ReadMessage(byte[] bytes)
        {
            try
            {
                if (bytes.Length > 0)
                {
                    encrypter.Connecter.GetMessage(this, bytes);
                }
            }
            catch
            {
            }
            return new NextFunction(4, new FuncDelegate(ReadLength));
        }

        public void MakeConnection(EncryptedConnection encryptedConnection)
        {
            throw new NotImplementedException();
        }

        public void LoseConnection(EncryptedConnection encryptedConnection)
        {
            throw new NotImplementedException();
        }

        public void FlushConnection(EncryptedConnection encryptedConnection)
        {
            throw new NotImplementedException();
        }

        public void DataCameIn(byte[] bytes)
        {
            int i;
            byte[] t, m;
            while (true)
            {
                if (this.closed)
                {
                    return;
                }
                i = this.nextLength - (int)(this.buffer.Position);
                if (i > bytes.Length)
                {
                    this.buffer.Write(bytes, 0, bytes.Length);
                    return;
                }
                this.buffer.Write(bytes, 0, i);
                t = new byte[bytes.Length - i];
                Buffer.BlockCopy(bytes, i, t, 0, bytes.Length - i);
                bytes = t;
                m = this.buffer.ToArray();
                this.buffer.Close();
                this.buffer = new MemoryStream();
                NextFunction x = this.nextFunction(m);
                if (x == null)
                {
                    this.Close();
                    return;
                }
                this.nextLength = x.Length;
                this.nextFunction = x.NextFunc;

            }
        }

        public void Server()
        {
            this.closed = true;
            encrypter.Remove(connection);
            if (this.complete)
                encrypter.Connecter.LoseConnection(this);
        }

        public void SendMessage(byte message)
        {
            SendMessage(new byte[] { message });
        }

        public void SendMessage(byte[] message)
        {
            byte[] lengthBytes = BitConverter.GetBytes(message.Length);
            byte swap;
            swap = lengthBytes[0];
            lengthBytes[0] = lengthBytes[3];
            lengthBytes[3] = swap;
            swap = lengthBytes[1];
            lengthBytes[1] = lengthBytes[2];
            lengthBytes[2] = swap;
            connection.Write(lengthBytes);
            connection.Write(message);
        }

        public void Close()
        {
            if (!closed)
            {
                connection.Close();
                this.Server();
            }
        }
    }
}
