using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.ReadFunctions
{
    /// <summary>
    /// 分析下载文件字节流类
    /// </summary>
    public class ReadDownloadID : ReadLength
    {
        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next">下一个字节流类</param>
        /// <param name="encrypter">封装连接器类</param>
        public ReadDownloadID(ReadFunction next, Encrypter encrypter)
            : base(20, next, encrypter) { }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// 分析字节流
        /// </summary>
        /// <param name="bytes">待分析的字节流</param>
        /// <returns>如果字节流正确，返回true，否则返回false</returns>
        public override bool ReadBytes(byte[] bytes)
        {
            //如果下载文件的SHA1码相同，则返回true，否则返回false
            int i;
            for (i = 0; i < 20; i++)
            {
                if (bytes[i] != Encrypter.DownloadID[i])
                {
                    return false;
                }
            }
            return true;
        }

        #endregion
    }
}
