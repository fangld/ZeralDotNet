﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ZeraldotNet.LibBitTorrent.NextFunctions;

namespace ZeraldotNet.LibBitTorrent
{
    public class EncryptedConnection
    {
        private Encrypter encrypter;

        private SingleSocket connection;

        private byte[] id;

        public byte[] ID
        {
            get { return this.id; }
            set { this.id = value; }
        }

        private bool isLocallyInitiated;

        public bool IsLocallyInitiated
        {
            get { return this.isLocallyInitiated; }
        }

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

            SendHandshakeMessage();
        }

        private void SendHandshakeMessage()
        {
            connection.Write(new byte[] { Globals.protocolNameLength });
            connection.Write(Globals.protocolName);
            connection.Write(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 });
            connection.Write(encrypter.DownloadID);
            connection.Write(encrypter.MyID);
        }

        private void BuildReadFunctionChain()
        {
            ReadFunction readMessage = new ReadMessage(0, this.encrypter, this);
            ReadFunction readLength = new ReadLength(readMessage, this.encrypter);
            readMessage.Next = readLength;
            ReadFunction readPeerID = new ReadPeerID(readLength, this.encrypter, this);
            ReadFunction readDownloadID = new ReadDownloadID(readPeerID, this.encrypter);
            ReadFunction readReserved = new ReadReserved(readDownloadID);
            ReadFunction readHeader = new ReadHeaderLength(Globals.protocolNameLength, readHeaderLength);
            ReadFunction readHeaderLength = new ReadHeaderLength(readHeader);
        }

        public NextFunction ReadHeaderLength(byte[] bytes)
        {
            if (bytes[0] != Globals.protocolNameLength)
            {
                return null;
            }
            return new NextFunction(Globals.protocolNameLength, new FuncDelegate(ReadHeader));
        }

        public NextFunction ReadHeader(byte[] bytes)
        {
            //string pName = Encoding.Default.GetString(bytes, 0, Globals.protocolNameLength);
            //if (pName != Globals.protocolName)
            //{
            //    return null;
            //}
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
            this.complete = true;
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

        public void DataCameIn(byte[] bytes)
        {
            int i;
            byte[] t, m;
            NextFunction nextFunc;
            do
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

                nextFunc = this.nextFunction(m);
                if (nextFunc == null)
                {
                    this.Close();
                    return;
                }

                this.nextLength = nextFunc.Length;
                this.nextFunction = nextFunc.NextFunc;
            } while (true);
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
            byte[] lengthBytes = new byte[4];
            Globals.Int32ToBytes(message.Length, lengthBytes, 0);
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
