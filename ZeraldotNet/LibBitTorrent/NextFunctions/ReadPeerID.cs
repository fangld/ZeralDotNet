using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.NextFunctions
{
    public class ReadPeerID : ReadLength
    {
        #region Private Field

        private EncryptedConnection encryptedConnection;

        #endregion

        #region Public Properties

        public EncryptedConnection EncryptedConnection
        {
            get { return this.encryptedConnection; }
            set { this.encryptedConnection = value; }
        }

        #endregion

        #region Constructors

        protected ReadPeerID(int length, ReadFunction next, Encrypter encrypter, EncryptedConnection encryptedConnection)
            : base(length, next, encrypter) 
        {
            this.encryptedConnection = encryptedConnection;
        }

        public ReadPeerID(ReadFunction next, Encrypter encrypter, EncryptedConnection encryptedConnection)
            : this(20, next, encrypter, encryptedConnection){}

        #endregion

        #region Overriden Methods

        public override bool ReadBytes(byte[] bytes)
        {
            if (encryptedConnection.ID == null)
            {
                encryptedConnection.ID = bytes;
            }
            else
            {
                int i;
                for (i = 0; i < 20; i++)
                {
                    if (bytes[i] != encryptedConnection.ID[i])
                    {
                        return false;
                    }
                }
            }
            encryptedConnection.Complete = true;
            Encrypter.Connecter.MakeConnection(encryptedConnection);
            return true;
            //new NextFunction(4, new FuncDelegate(ReadLength));
        }

        #endregion
    }
}
