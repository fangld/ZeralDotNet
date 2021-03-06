﻿using System;
using System.Collections.Generic;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 没有激活的请求信息
    /// </summary>
    public struct InactiveRequest : IComparable<InactiveRequest>, IEquatable<InactiveRequest>
    {
        #region Fields

        /// <summary>
        /// 请求信息的起始位置
        /// </summary>
        private int begin;

        /// <summary>
        /// 请求信息的长度
        /// </summary>
        private int length;

        #endregion

        #region Properties

        /// <summary>
        /// 访问和设置请求信息的起始位置
        /// </summary>
        public int Begin
        {
            get { return begin; }
            set { begin = value; }
        }

        /// <summary>
        /// 访问和设置请求信息的长度
        /// </summary>
        public int Length
        {
            get { return length; }
            set { length = value; }
        }

        #endregion

        #region Constructors

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

        #endregion

        #region Methods

        /// <summary>
        ///  从没有激活的请求信息列表中选择最小的没有激活的请求信息
        /// </summary>
        /// <param name="requests">没有激活的请求信息列表</param>
        /// <returns>返回最小的没有激活的请求信息/returns>
        public static InactiveRequest Min(IList<InactiveRequest> requests)
        {
            InactiveRequest minRequest = requests[0];
            int i;
            for (i = 1; i < requests.Count; i++)
            {
                if (requests[i].CompareTo(minRequest) < 0)
                {
                    minRequest = requests[i];
                }
            }

            return minRequest;
        }

        #endregion

        #region Overriden Methos

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

        #endregion
    }
}