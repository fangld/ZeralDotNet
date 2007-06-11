using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class Flag
    {
        bool flag = false;

        public void Set()
        {
            lock (this)
            {
                flag = true;
            }
        }

        public bool isSet()
        {
            return flag;
        }

        public void Reset()
        {
            flag = false;
        }
    }
}
