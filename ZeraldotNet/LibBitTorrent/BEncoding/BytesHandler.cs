using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    /// <summary>
    /// Handler的字节数组类
    /// </summary>
    public class BytesHandler : Handler, IComparable<BytesHandler>, IEquatable<BytesHandler>
    {
        #region Private Field
        /// <summary>
        /// 字节数组
        /// </summary>
        private byte[] text;
        #endregion

        #region Public Properties

        /// <summary>
        /// 字节数组的访问器
        /// </summary>
        public byte[] ByteArray
        {
            get { return this.text; }
            set { this.text = value; }
        }

        /// <summary>
        /// 字符串的访问器
        /// </summary>
        public string StringText
        {
            get { return Encoding.Default.GetString(text); }

            set { this.text = Encoding.Default.GetBytes(value); }
        }

        #endregion

        #region Constructors
        /// <summary>
        /// 构造函数,定义元素类型为字节数组类型
        /// </summary>
        public BytesHandler() { }

        /// <summary>
        /// 构造函数,定义元素类型为字节数组类型
        /// </summary>
        /// <param name="value">字符串</param>
        public BytesHandler(byte[] value)
        {
            this.text = value;
        }

        public BytesHandler(string value)
            : this(Encoding.Default.GetBytes(value)) { }
        #endregion

        #region Methods
        public static implicit operator BytesHandler(string value)
        {
            return new BytesHandler(value);
        }

        public static implicit operator BytesHandler(byte[] value)
        {
            return new BytesHandler(value);
        }
                #endregion

        #region Overriden Methods
        /// <summary>
        /// Handler字符串类的解码函数
        /// </summary>
        /// <param name="bytes">待解码的字节数组</param>
        /// <param name="position">字节数组的解码位置</param>
        /// <returns>解码的字节数组长度</returns>
        public override int Decode(byte[] source, ref int position)
        {
            //保存初始位置
            int start = position;
            StringBuilder str = new StringBuilder();

            try
            {
                //当遇到字符':'(ASCII码为58),整数部分的解析结束
                while (source[position] != 58)
                {
                    str.Append((char)source[position]);
                    position++;
                }

                //跳过字符':'
                position++;
            }

            //当捕捉IndexOutOfRangeException,抛出BitTorrentException
            catch (IndexOutOfRangeException)
            {
                throw new BitTorrentException("BEnocde字节数组类的整数部分解析错误");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //判断整数解析的正确性,错误则抛出异常
            Regex r = new Regex("^(0|[1-9][0-9]*)$", RegexOptions.Compiled);
            if (!r.IsMatch(str.ToString()))
            {
                throw new BitTorrentException("BEnocde字节数组类的整数部分解析错误");
            }

            //保存字符串长度
            int length = int.Parse(str.ToString());

            text = new byte[length];

            //开始解析字节数组
            try
            {
                if (length > 0)
                {
                    int index = position;
                    int byteArrayStart = position;
                    position += length;
                    while (index < position)
                    {
                        text[index - byteArrayStart] = source[index++];
                    }
                }
            }

            //当捕捉IndexOutOfRangeException,抛出BitTorrentException
            catch (IndexOutOfRangeException)
            {
                throw new BitTorrentException("BEnocde字节数组类的字节数组长度异常");
            }


            //返回所解析的数组长度
            return position - start;
        }

        /// <summary>
        /// Handler字符串类的编码函数
        /// </summary>
        /// <param name="msw">待编码的内存写入流</param>
        public override void Encode(MemoryStream msw)
        {
            byte[] op = Encoding.Default.GetBytes(string.Format("{0:d}:", text.Length));
            msw.Write(op, 0, op.Length);
            msw.Write(text, 0, text.Length);
        }

        #region IComparable<ByteArrayHandler> Members

        public int CompareTo(BytesHandler other)
        {
            return this.StringText.CompareTo(other.StringText);
        }

        #endregion

        #region IEquatable<ByteArrayHandler> Members

        public bool Equals(BytesHandler other)
        {
            return this.StringText.Equals(other.StringText);
        }

        #endregion

        public override int GetHashCode()
        {
            int hash = 0;
            int index;
            for (index = 0; index < this.text.Length; index++)
            {
                hash += this.text[index];
            }
            return hash;
        }
        #endregion
    }
}
