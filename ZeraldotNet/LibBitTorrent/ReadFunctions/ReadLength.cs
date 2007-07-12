using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.LibBitTorrent.ReadFunctions
{
    /// <summary>
    /// 分析网络信息长度类
    /// </summary>
    public class ReadLength : ReadFunction
    {
        #region Protected Field

        /// <summary>
        /// 封装连接类
        /// </summary>
        protected IEncrypter encrypter;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="length">分析的长度</param>
        /// <param name="next">下一个分析字节流类</param>
        /// <param name="encrypter">封装连接类</param>
        protected ReadLength(int length, ReadFunction next, IEncrypter encrypter)
            : base(length, next)
        {
            this.encrypter = encrypter;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next">下一个分析字节流类</param>
        /// <param name="encrypter">封装连接类</param>
        public ReadLength(ReadFunction next, IEncrypter encrypter)
            : this(4, next, encrypter) { }

        #endregion

        #region Override Methods

        /// <summary>
        /// 分析字节流
        /// </summary>
        /// <param name="bytes">待分析的字节流</param>
        /// <returns>如果字节流正确，返回true，否则返回false</returns>
        public override bool ReadBytes(byte[] bytes)
        {
            //如果字节流长度在允许范围，则返回true，否则返回false
            int length = Globals.BytesToInt32(bytes, 0);
            if (length > encrypter.MaxLength)
            {
                foreach (byte item in bytes)
                {
                    Console.Write("{0:X} ", item);
                }
                Console.WriteLine(length);
                return false;
            }
            this.Next.Length = length;
            return true;
        }

        #endregion
    }
}
