using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.NextFunctions
{
    public class ReadLength : ReadFunction
    {
        #region Private Field

        private Encrypter encrypter;

        #endregion

        #region Public Properties

        public Encrypter Encrypter
        {
            get { return this.encrypter; }
            set { this.encrypter = value; }
        }

        #endregion

        #region Constructors

        protected ReadLength(int length, ReadFunction next, Encrypter encrypter)
            : base(length, next)
        {
            this.encrypter = encrypter;
        }

        public ReadLength(ReadFunction next, Encrypter encrypter)
            : this(4, next, encrypter) { }

        #endregion

        #region Override Methods

        public override bool ReadBytes(byte[] bytes)
        {
            int length = Globals.BytesToInt32(bytes, 0);
            if (length > encrypter.MaxLength)
            {
                return false;
            }
            this.Next.Length = length;
            return true; 
                //new NextFunction(length, new FuncDelegate(ReadMessage));
        }

        #endregion
    }
}
