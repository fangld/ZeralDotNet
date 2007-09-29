using System.IO;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    /// <summary>
    /// Handler的生成工厂
    /// </summary>
    public class BEncode
    {        /// <summary>
        /// 安全解码函数,带错误检测
        /// </summary>
        /// <param name="source">待解码的字符串</param>
        /// <returns>返回已解码的Handler基类</returns>
        public static BEncodedNode Decode(string source)
        {
            return Decode(Encoding.Default.GetBytes(source));
        }

        /// <summary>
        /// 安全解码函数,带错误检测
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
        /// <returns>返回已解码的Handler基类</returns>
        public static BEncodedNode Decode(byte[] source)
        {
            //初始化变量
            int position = 0;

            ///当待解码字节数组的长度为零,抛出异常
            if (source.Length == 0)
                throw new BitTorrentException("待解码的字节数组的长度为零");

            BEncodedNode interpreter = Decode(source, ref position);
            if (position != source.Length)
                throw new BitTorrentException("解码的字节数组长度异常");
            return interpreter;
        }

        /// <summary>
        /// 解码函数,不带错误判断
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
        /// <param name="position">字节数组的位置</param>
        /// <returns>返回Handler基类类型</returns>
        public static BEncodedNode Decode(byte[] source, ref int position)
        {
            byte b = source[position];

            BEncodedNode interpreter;

            //如果source[position]等于'l'(ASCII码为108),就返回ListHandler
            if (b == 108)
            {
                interpreter = new ListNode();
            }

            //如果source[position]等于'd'(ASCII码为100),就返回DictionaryHandler
            else if (b == 100)
            {
                interpreter = new DictNode();
            }

            //如果source[position]等于'index'(ASCII码为105),就返回IntHandler
            else if (b == 105)
            {
                interpreter = new IntNode();
            }

            //如果source[position]等于'0' - '9'(ASCII码为48 - 57),就返回ByteArrayHandler
            else if (b >= 48 && b <= 57)
            {
                interpreter = new BytesNode();
            }

            //其它的情况,抛出异常
            else
            {
                throw new BitTorrentException("待解码的字节数组的首位字节异常");
            }

            interpreter.Decode(source, ref position);
            return interpreter;
        }

        /// <summary>
        /// 编码函数,返回字节数组
        /// </summary>
        /// <param name="source">待编码的Handler对象</param>
        /// <returns>已编码的字节数组</returns>
        public static byte[] ByteArrayEncode(BEncodedNode source)
        {
            MemoryStream msw = new MemoryStream();
            source.Encode(msw);
            return msw.ToArray();
        }

        /// <summary>
        /// 编码函数,返回字符串
        /// </summary>
        /// <param name="source">待编码的Handler对象</param>
        /// <returns>已编码的字符串</returns>
        public static string StringEncode(BEncodedNode source)
        {
            return Encoding.Default.GetString(ByteArrayEncode(source));
        }
    }
}