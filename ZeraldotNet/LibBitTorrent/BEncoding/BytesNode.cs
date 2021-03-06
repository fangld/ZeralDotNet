﻿using System;
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

        private static Regex _regex = new Regex(@"^([1-9]\d*|0)$", RegexOptions.Compiled);

        #endregion

        #region Properties

        /// <summary>
        /// 字节数组的访问器
        /// </summary>
        public byte[] ByteArray
        {
            get { return _bytes; }
            set { _bytes = value; }
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
        /// <param name="index">字节数组的解码位置</param>
        /// <returns>解码的字节数组长度</returns>
        public override int Decode(byte[] source, ref int index)
        {
            //保存初始位置
            int start = index;

            //当遇到字符':'(ASCII码为58),整数部分的解析结束
            int end = Array.IndexOf<byte>(source, 58, start);
            if (end == -1)
                throw new BitTorrentException("BEncode整数类的字节数组长度异常");

            StringBuilder sb = new StringBuilder(end - start);

            do
            {
                sb.Append((char) source[index]);
                index++;
            } while (source[index] != 58);

            //跳过字符':'
            index++;

            int length;
            string lengthString= sb.ToString();

            if (_regex.IsMatch(lengthString))
            {
                length = int.Parse(lengthString);
            }
            else
            {
                throw new BitTorrentException("BEnocde字节数组中的整数部分解析错误");
            }

            _bytes = new byte[length];

            //开始解析字节数组
            if (length >= 0 && length <= source.Length - index)
            {
                Buffer.BlockCopy(source, index, _bytes, 0, length);
                index += length;
            }
            else
            {
                throw new BitTorrentException("BEnocde字节数组类的字节数组长度异常");
            }

            //返回所解析的数组长度
            return index - start;
        }

        /// <summary>
        /// Handler字符串类的编码函数
        /// </summary>
        /// <param name="ms">待编码的内存写入流</param>
        public override void Encode(MemoryStream ms)
        {
            byte[] lengthBytes = Encoding.UTF8.GetBytes(string.Format("{0:d}:", _bytes.Length));
            ms.Write(lengthBytes, 0, lengthBytes.Length);
            ms.Write(_bytes, 0, _bytes.Length);
        }

        #endregion
    }
}
