using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.NextFunctions
{
    public class ReadDownloadID : ReadLength
    {
        #region Constructors

        public ReadDownloadID(ReadFunction next, Encrypter encrypter)
            : base(20, next, encrypter) { }

        #endregion

        #region Overriden Methods

        public override bool ReadBytes(byte[] bytes)
        {
            int i;
            for (i = 0; i < 20; i++)
            {
                if (bytes[i] != Encrypter.DownloadID[i])
                {
                    return false;
                }
            }
            return true;
            //new NextFunction(20, new FuncDelegate(ReadPeerID));
        }

        #endregion
    }
}
