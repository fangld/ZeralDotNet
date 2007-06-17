using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class Encrpyter
    {
        private Connecter connecter;

        public Connecter Connecter
        {
            get { return this.connecter; }
            set { this.connecter = value; }
        }

        private RawServer rawServer;

        private int maxInitiates;
    }
}
