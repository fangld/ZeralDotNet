﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Handler基类
    /// </summary>
    public abstract class Handler
    {
        public static implicit operator Handler(string value)
        {
            return new BytesHandler(value);
        }

        public static implicit operator Handler(byte[] value)
        {
            return new BytesHandler(value);
        }

        public static implicit operator Handler(long value)
        {
            return new IntHandler(value);
        }

        #region 构造函数
        protected Handler() { }
        #endregion

        /// <summary>
        /// Handler基类的解码函数
        /// </summary>
        /// <param name="bytes">待解码的字节数组</param>
        /// <param name="position">字节数组的解码位置</param>
        public abstract int Decode(byte[] source, ref int position);

        /// <summary>
        /// Handler基类的编码函数
        /// </summary>
        /// <param name="msw">待编码的内存写入流</param>
        public abstract void Encode(MemoryStream msw);

        public override string ToString()
        {
            MemoryStream msw = new MemoryStream();
            this.Encode(msw);
            return Encoding.Default.GetString(msw.ToArray());
        }
    }
}