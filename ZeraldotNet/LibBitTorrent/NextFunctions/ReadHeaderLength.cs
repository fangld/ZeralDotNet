using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.NextFunctions
{
    public class ReadHeaderLength : ReadFunction
    {
        #region Constructors

        protected ReadHeaderLength(ReadFunction next)
            : base(1, next) { }

        #endregion

        #region Overriden Methods

        public override bool ReadBytes(byte[] bytes)
        {
            if (bytes[0] != Length)
            {
                return false;
            }

            return true; 
                //new NextFunction(Globals.protocolNameLength, new FuncDelegate(ReadHeader));
        }

        #endregion
    }
}
