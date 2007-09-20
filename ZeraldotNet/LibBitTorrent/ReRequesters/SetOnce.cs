using System;
using System.Collections.Generic;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.ReRequesters
{
    public sealed class SetOnce
    {
        #region Fields

        private bool flag;

        #endregion

        #region Constructors

        public SetOnce()
        {
            this.flag = true;
        }

        #endregion

        #region Methods

        public void Reset()
        {
            lock(this)
            {
                flag = true;
            }
        }

        public bool Set()
        {
            lock(this)
            {
                if(!flag)
                {
                    return false;
                }
                flag = false;
                return true;
            }
        }

        #endregion
    }
}
