using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.NextFunctions
{
    public class ReadHeader : ReadFunction
    {
        #region Constructors

        public ReadHeader(ReadFunction next)
            : this(19, next) { }

        #endregion

        #region Overriden Methods

        public override bool ReadBytes(byte[] bytes)
        {
            if (bytes.Length < this.Length)
                return false;

            int i;
            for (i = 0; i < Globals.protocolName.Length; i++)
            {
                if (bytes[i] != Globals.protocolName[i])
                {
                    return false;
                }
            }

            return true;
                //new NextFunction(8, new FuncDelegate(ReadReserved));
        }

        #endregion
    }
}
