using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public struct InactiveRequest : IComparable<InactiveRequest>, IEquatable<InactiveRequest>
    {
        private int begin;

        public int Begin
        {
            get { return begin; }
            set { begin = value; }
        }

        private int length;

        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="begin"></param>
        /// <param name="length"></param>
        public InactiveRequest(int begin, int length)
        {
            this.begin = begin;
            this.length = length;
        }

        #region IComparable<InactiveRequest> Members

        public int CompareTo(InactiveRequest other)
        {
            return begin.CompareTo(other.Begin);
        }

        #endregion

        #region IEquatable<InactiveRequest> Members

        public bool Equals(InactiveRequest other)
        {
            return (begin == other.Begin && length == other.Length);
        }

        #endregion
    }
}
