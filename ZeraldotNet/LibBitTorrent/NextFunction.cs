﻿using System;
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
        #region Private Field

        private int length;

        private FuncDelegate nextFunc;

        #endregion

        #region Public Properties

        public int Length
        {
            get { return this.length; }
            set { this.length = value; }
        }

        public FuncDelegate NextFunc
        {
            get { return this.nextFunc; }
            set { this.nextFunc = value; }
        }

        #endregion

        #region Constructors

        public NextFunction(int length, FuncDelegate nextFunc)
        {
            this.length = length;
            this.nextFunc = nextFunc;
        }

        #endregion
    }
}
