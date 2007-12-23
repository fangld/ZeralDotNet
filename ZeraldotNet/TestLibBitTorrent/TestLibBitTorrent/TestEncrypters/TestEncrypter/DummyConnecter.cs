using System;
using System.Collections;
using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.TestLibBitTorrent.TestEncrypter
{
    public class DummyConnecter : IConnecter
    {
        public ArrayList log;
        private bool closeNext;

        public bool CloseNext
        {
            get { return closeNext; }
            set { closeNext = value; }
        }

        public DummyConnecter()
        {
            this.log = new ArrayList();
            this.closeNext = false;
        }

        #region IConnecter Members

        public void CheckEndgame()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void CloseConnection(IEncryptedConnection encryptedConnection)
        {
            log.Add(new object[] { "lose ", encryptedConnection });
        }

        public ICollection<IConnection> Connections
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public int ConnectionsCount
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void FlushConnection(IEncryptedConnection encryptedConnection)
        {
            log.Add(new object[] { "flush ", encryptedConnection });
        }

        public void GetMessage(IEncryptedConnection encryptedConnection, byte[] message)
        {
            log.Add(new object[] { "get ", encryptedConnection, message });
            Console.WriteLine("Connecter Get Message:{0}",this.closeNext);
            if (this.closeNext)
            {
                Console.WriteLine("Connecter close Encrypted Connection");
                encryptedConnection.Close();
            }
        }

        public void MakeConnection(IEncryptedConnection encryptedConnection)
        {
            log.Add(new object[] { "make ", encryptedConnection });
        }

        public int PiecesNumber
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool RateCapped
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void UnCap()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void UpdateUploadRate(int amount)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
