using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.NextFunctions
{
    public class ReadReserved : ReadFunction
    {
        #region Constructors

        public ReadReserved(ReadFunction next)
            : this(8, next) { }

        #endregion

        #region Overriden Methods

        public override bool ReadBytes(byte[] bytes)
        {
            if (bytes.Length < this.Length)
                return false;

            return true;
                //new NextFunction(8, new FuncDelegate(ReadReserved));
        }

        #endregion
    }
}
