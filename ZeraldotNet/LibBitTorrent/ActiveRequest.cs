using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 激活的请求信息
    /// </summary>
    public struct ActiveRequest : IEquatable<ActiveRequest>
    {
        /// <summary>
        /// 片断的索引号
        /// </summary>
        private int index;

        /// <summary>
        /// 访问和设置片断的索引号
        /// </summary>
        public int Index
        {
            get { return this.index; }
            set { this.index = value; }
        }

        /// <summary>
        /// 请求信息的起始位置
        /// </summary>
        private int begin;

        /// <summary>
        /// 访问和设置请求信息的起始位置
        /// </summary>
        public int Begin
        {
            get { return this.begin; }
            set { this.begin = value; }
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
            get { return this.length; }
            set { this.length = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="index">片断的索引号</param>
        /// <param name="begin">请求信息的起始位置</param>
        /// <param name="lengthBytes">请求信息的长度</param>
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
