using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    /// <summary>
    /// Handler整数类
    /// </summary>
    public class IntNode : BEncodedNode
    {
        #region Fields

        private static Regex _regex = new Regex(@"^((-?[1-9]\d*)|0)$", RegexOptions.Compiled);

        #endregion

        #region Properties

        /// <summary>
        /// 64位整数
        /// </summary>
        public long Value { get; set; }

        /// <summary>
        /// 32位整数
        /// </summary>
        public int IntValue { get { return (int)Value; } }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数,定义元素类型为整数类型
        /// </summary>
        public IntNode() { }

        /// <summary>
        /// 构造函数,定义元素类型为整数类型
        /// </summary>
        /// <param name="value">64位整数的值</param>
        public IntNode(long value)
        {
            Value = value;
        }

        #endregion

        #region Methods

        public static implicit operator IntNode(long value)
        {
            return new IntNode(value);
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Handler整数类的解码函数
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
        /// <param name="position">字节数组的解码位置</param>
        /// <returns>解码的字节数组长度</returns>
        public override int Decode(byte[] source, ref int position)
        {
            //保存初始位置
            int start = position;
            int end = Array.IndexOf<byte>(source, 101, start);
            if (end == -1)
                throw new BitTorrentException("BEncode整数类的字节数组长度异常");

            StringBuilder sb = new StringBuilder(end - start);

            //跳过字符'i'
            position++;
            
            //当遇到字符'e'(ASCII码为101),解析结束
            while (source[position] != 101)
            {
                sb.Append((char) source[position]);
                position++;
            }

            //跳过字符'e'
            position++;

            string valueString = sb.ToString();

            if (valueString.Length != 0)
            {
                if (_regex.IsMatch(valueString))
                {
                    Value = long.Parse(valueString);
                }
                else
                {
                    throw new BitTorrentException("BEnocde字节数组中的整数部分解析错误");
                }
            }
            else
            {
                throw new BitTorrentException("BEnocde字节数组中的整数部分解析错误");
            }

            //返回所解析的数组长度
            return position - start;
        }

        /// <summary>
        /// Handler整数类的编码函数
        /// </summary>
        /// <param name="ms">待编码的内存写入流</param>
        public override void Encode(MemoryStream ms)
        {
            byte[] op = Encoding.ASCII.GetBytes(string.Format("i{0:d}e", Value));
            ms.Write(op, 0, op.Length);
        }

        #endregion
    }
}
