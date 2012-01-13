using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    /// <summary>
    /// Handler的字节数组类
    /// </summary>
    public class BytesNode : BEncodedNode
    {
        #region Fields

        /// <summary>
        /// 字节数组
        /// </summary>
        private byte[] _bytes;

        /// <summary>
        /// 编码
        /// </summary>
        private Encoding _encoding;

        #endregion

        #region Properties

        /// <summary>
        /// 字节数组的访问器
        /// </summary>
        public byte[] ByteArray
        {
            get { return this._bytes; }
            set { this._bytes = value; }
        }

        /// <summary>
        /// 字符串的访问器
        /// </summary>
        public string StringText
        {
            get { return _encoding.GetString(_bytes); }
            set { _bytes = _encoding.GetBytes(value); }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数,定义元素类型为字节数组类型
        /// </summary>
        public BytesNode()
        {
            _encoding = Encoding.UTF8;
        }

        /// <summary>
        /// 构造函数,定义元素类型为字节数组类型
        /// </summary>
        /// <param name="bytes">字节数组</param>
        public BytesNode(byte[] bytes)
        {
            this._bytes = bytes;
        }

        public BytesNode(string str, Encoding encoding)
            : this(encoding.GetBytes(str)) { }

        public BytesNode(string str)
            : this(str, Encoding.UTF8) { }

        #endregion

        #region Methods

        public static implicit operator BytesNode(string str)
        {
            return new BytesNode(str);
        }

        public static implicit operator BytesNode(byte[] bytes)
        {
            return new BytesNode(bytes);
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Handler字符串类的解码函数
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
        /// <param name="position">字节数组的解码位置</param>
        /// <returns>解码的字节数组长度</returns>
        public override int Decode(byte[] source, ref int position)
        {
            //保存初始位置
            int start = position;
            StringBuilder sb = new StringBuilder();

            try
            {
                //当遇到字符':'(ASCII码为58),整数部分的解析结束
                while (source[position] != 58)
                {
                    sb.Append((char)source[position]);
                    position++;
                }

                //跳过字符':'
                position++;
            }

            //当捕捉IndexOutOfRangeException,抛出BitTorrentException
            catch (IndexOutOfRangeException)
            {
                throw new BitTorrentException("BEnocde字节数组中的整数部分解析错误");
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //保存字符串长度
            uint length;
            bool success = uint.TryParse(sb.ToString(), out length);

            //判断整数解析的正确性,错误则抛出异常
            if (!success)
            {
                throw new BitTorrentException("BEnocde字节数组中的整数部分解析错误");
            }

            _bytes = new byte[length];

            //开始解析字节数组
            try
            {
                if (length > 0)
                {
                    Buffer.BlockCopy(source, position, _bytes, 0, (int)length);
                    position += (int)length;
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
        /// <param name="ms">待编码的内存写入流</param>
        public override void Encode(MemoryStream ms)
        {
            byte[] lengthBytes = Encoding.ASCII.GetBytes(string.Format("{0:d}:", _bytes.Length));
            ms.Write(lengthBytes, 0, lengthBytes.Length);
            ms.Write(_bytes, 0, _bytes.Length);
        }

        public override void SetEncoding(Encoding encoding)
        {
            _encoding = encoding;
        }

        #endregion
    }
}
