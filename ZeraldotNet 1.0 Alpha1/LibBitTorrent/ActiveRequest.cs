using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public struct ActiveRequest : IEquatable<ActiveRequest>
    {
        private int index;

        public int Index
        {
            get { return this.index; }
            set { this.index = value; }
        }

        private int begin;

        public int Begin
        {
            get { return this.begin; }
            set { this.begin = value; }
        }

        private int length;

        public int Length
        {
            get { return this.length; }
            set { this.length = value; }
        }

        public ActiveRequest(int index, int begin, int length)
        {
            this.index = index;
            this.begin = begin;
            this.length = length;
        }

        #region IEquatable<ActiveRequest> Members

        public bool Equals(ActiveRequest other)
        {
            return (index == other.Index && begin == other.Begin && length == other.Length);
        }

        #endregion
    }
}
