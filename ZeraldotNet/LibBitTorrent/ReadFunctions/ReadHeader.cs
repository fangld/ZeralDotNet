using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.ReadFunctions
{
    /// <summary>
    /// 分析协议名类
    /// </summary>
    public class ReadHeader : ReadFunction
    {
        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next">下一个分析字节流类</param>
        public ReadHeader(ReadFunction next)
            : base(19, next) { }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 分析字节流
        /// </summary>
        /// <param name="bytes">待分析的字节流</param>
        /// <returns>如果字节流正确，返回true，否则返回false</returns>
        public override bool ReadBytes(byte[] bytes)
        {
            //如果得到的协议名为"Bittorrent Protocol"，则返回true
            int i;
            for (i = 0; i < Globals.protocolName.Length; i++)
            {
                if (bytes[i] != Globals.protocolName[i])
                {
                    return false;
                }
            }
            
            //否则返回false
            return true;
        }

        #endregion
    }
}
