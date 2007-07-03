using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.NextFunctions
{
    public class ReadMessage : ReadPeerID
    {
        #region Constructors

        public ReadMessage(int length, ReadLength next, Encrypter encrypter, EncryptedConnection encryptedConnection)
            : base(length, next, encrypter, encryptedConnection) { }

        public ReadMessage(int length, Encrypter encrypter, EncryptedConnection encryptedConnection)
            : base(length, null, encrypter, encryptedConnection) { }

        #endregion

        #region Overriden Methods

        public override bool ReadBytes(byte[] bytes)
        {
            try
            {
                if (bytes.Length > 0)
                {
                    Encrypter.Connecter.GetMessage(EncryptedConnection, bytes);
                }
            }
            catch
            {
            }
            return true;
            //new NextFunction(4, new FuncDelegate(ReadLength));
        }

        #endregion
    }
}
