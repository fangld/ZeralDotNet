using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using System.IO;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Handler整数类
    /// </summary>
    public class IntHandler : Handler
    {
        /// <summary>
        /// 整数
        /// </summary>
        private long value;

        /// <summary>
        /// 整数访问器
        /// </summary>
        public long Value
        {
            get
            {
                return this.value;
            }

            set
            {
                this.value = value;
            }
        }

        public static implicit operator IntHandler(long value)
        {
            return new IntHandler(value);
        }

        #region 构造函数
        /// <summary>
        /// 构造函数,定义元素类型为整数类型
        /// </summary>
        public IntHandler() { }

        /// <summary>
        /// 构造函数,定义元素类型为整数类型
        /// </summary>
        /// <param name="value">64位整数的值</param>
        public IntHandler(long value)
        {
            Value = value;
        }
        #endregion

        /// <summary>
        /// Handler整数类的解码函数
        /// </summary>
        /// <param name="bytes">待解码的字节数组</param>
        /// <param name="position">字节数组的解码位置</param>
        /// <returns>解码的字节数组长度</returns>
        public override int Decode(byte[] source, ref int position)
        {
            //保存初始位置
            int start = position;
            StringBuilder str = new StringBuilder();

            //跳过字符'i'
            position++;

            try
            {
                //当遇到字符'e'(ASCII码为101),解析结束
                while (source[position] != 101)
                {
                    str.Append((char)source[position]);
                    position++;
                }

                //跳过字符'e'
                position++;
            }

            //当捕捉IndexOutOfRangeException,抛出BitTorrentException
            catch (IndexOutOfRangeException)
            {
                throw new BitTorrentException("BEncode整数类的字节数组长度异常");
            }

            //判断整数解析的正确性,错误则抛出异常
            Regex r = new Regex("^(0|-?[1-9][0-9]*)$", RegexOptions.Compiled);
            if (r.IsMatch(str.ToString()))
            {
                //保存64位整数
                this.value = long.Parse(str.ToString());


                //返回所解析的数组长度
                return position - start;
            }

            else
            {
                throw new BitTorrentException("BEncode整数类的整数解码错误");
            }
        }

        /// <summary>
        /// Handler整数类的编码函数
        /// </summary>
        /// <param name="msw">待编码的内存写入流</param>
        public override void Encode(MemoryStream msw)
        {
            byte[] op = Encoding.Default.GetBytes(string.Format("i{0:d}e", value));
            msw.Write(op, 0, op.Length);
        }
    }
}
