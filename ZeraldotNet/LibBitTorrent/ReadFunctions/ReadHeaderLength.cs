using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.ReadFunctions
{
    /// <summary>
    /// 分析协议名长度类
    /// </summary>
    public class ReadHeaderLength : ReadFunction
    {
        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next">下一个字节流类</param>
        public ReadHeaderLength(ReadFunction next)
            : base(1, next) { }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 分析字节流
        /// </summary>
        /// <param name="bytes">待分析的字节流</param>
        /// <returns>如果字节流正确，返回true，否则返回false</returns>
        public override bool ReadBytes(byte[] bytes)
        {
            //如果协议长度不等于19，返回false
            if (bytes[0] != Globals.protocolNameLength)
            {
                return false;
            }


            //否则返回true
            return true;
        }

        #endregion
    }
}
