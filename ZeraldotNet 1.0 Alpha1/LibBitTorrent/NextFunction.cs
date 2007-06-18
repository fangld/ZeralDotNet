using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate NextFunction FuncDelegate(byte[] bytes);
    public delegate void StartDelegate(IPEndPoint dns, byte[] id);

    public class NextFunction
    {
        private int length;

        public int Length
        {
            get { return this.length; }
            set { this.length = value; }
        }

        private FuncDelegate nextFunc;

        public FuncDelegate NextFunc
        {
            get { return this.nextFunc; }
            set { this.nextFunc = value; }
        }

        public NextFunction(int length, FuncDelegate nextFunc)
        {
            this.length = length;
            this.nextFunc = nextFunc;
        }
    }
}
