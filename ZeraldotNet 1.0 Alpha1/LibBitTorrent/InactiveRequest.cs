using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 
    /// </summary>
    public struct InactiveRequest : IComparable<InactiveRequest>, IEquatable<InactiveRequest>
    {
        /// <summary>
        /// 请求信息的起始位置
        /// </summary>
        private int begin;

        /// <summary>
        /// 访问和设置请求信息的起始位置
        /// </summary>
        public int Begin
        {
            get { return begin; }
            set { begin = value; }
        }

        /// <summary>
        /// 请求信息的长度
        /// </summary>
        private int length;

        /// <summary>
        /// 访问和设置请求信息的长度
        /// </summary>
        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="begin">请求信息的起始位置</param>
        /// <param name="length">请求信息的参与长度</param>
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