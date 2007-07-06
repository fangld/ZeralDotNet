using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.ReadFunctions
{
    /// <summary>
    /// 分析保留位类
    /// </summary>
    public class ReadReserved : ReadFunction
    {
        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next">下一个分析字节流类</param>
        public ReadReserved(ReadFunction next)
            : base(8, next) { }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 分析字节流
        /// </summary>
        /// <param name="bytes">待分析的字节流</param>
        /// <returns>如果字节流正确，返回true，否则返回false</returns>
        public override bool ReadBytes(byte[] bytes)
        {
            return true;
        }

        #endregion
    }
}
