using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Encrypters;
using ZeraldotNet.LibBitTorrent.RawServers;

namespace ZeraldotNet.TestLibBitTorrent.TestRawServer
{
    public class DummyEncrypter : IEncrypter
    {
        public List<ISingleSocket> externalDummySockets;
        public ArrayList dataIn;
        public List<ISingleSocket> lostDummySockets;

        public DummyEncrypter()
        {
            externalDummySockets = new List<ISingleSocket>();
            dataIn = new ArrayList();
            lostDummySockets = new List<ISingleSocket>();
        }

        public void LoseConnection(ISingleSocket singleSocket)
        {
            lostDummySockets.Add(singleSocket);
        }

        #region IEncrypter Members

        public void CloseConnection(ISingleSocket singleSocket)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IConnecter Connecter
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

        public void DataCameIn(ISingleSocket singleSocket, byte[] data)
        {
            dataIn.Add(new object[] { singleSocket, data });
        }

        public byte[] DownloadID
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

        public void FlushConnection(ISingleSocket singleSocket)
        {
        }

        public void MakeExternalConnection(ISingleSocket singleSocket)
        {
            externalDummySockets.Add(singleSocket);
        }

        public int MaxLength
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

        public byte[] MyID
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

        public void Remove(ISingleSocket keySocket)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void SendKeepAlives()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void StartConnect(IPEndPoint dns, byte[] id)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
