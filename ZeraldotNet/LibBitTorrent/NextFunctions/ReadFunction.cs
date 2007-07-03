using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.NextFunctions
{
    public abstract class ReadFunction
    {
        #region Private Field

        private int length;

        private ReadFunction next;

        #endregion

        #region Public Properties

        public int Length
        {
            get { return this.length; }
            set { this.length = value; }
        }

        public ReadFunction Next
        {
            get { return this.next; }
            set { this.next = value; }
        }

        #endregion

        #region Constructors

        public ReadFunction(int length, ReadFunction next)
        {
            this.length = length;
            this.next = next;
        }

        #endregion

        #region Methods

        public abstract bool ReadBytes(byte[] bytes);

        #endregion
    }
}
