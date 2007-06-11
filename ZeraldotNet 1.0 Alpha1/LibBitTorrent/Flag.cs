using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class Flag
    {
        private bool isSet = false;

        public bool IsSet
        {
            get { return isSet; }
        }

        public void Set()
        {
            lock (this)
            {
                isSet = true;
            }
        }

        public void Reset()
        {
            isSet = false;
        }
    }
}
