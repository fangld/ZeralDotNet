using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Handler字典类
    /// </summary>
    public class DictionaryHandler : Handler
    {
        /// <summary>
        /// string, Handler字典
        /// </summary>
        private IDictionary<ByteArrayHandler, Handler> dict;

        /// <summary>
        /// Handler字典索引器,索引为字符串
        /// </summary>
        /// <param name="index">字符串索引</param>
        /// <returns>Handler节点</returns>
        public Handler this[string key]
        {
            get
            {
                if (ContainsKey(key))
                {
                    ByteArrayHandler keyHandler = new ByteArrayHandler(key);
                    return dict[keyHandler];
                }
                else
                    throw new BitTorrentException("给定的字符串关键字不包含BEnocde字典类中");
            }
        }

        public bool ContainsKey(string key)
        {
            return dict.ContainsKey(new ByteArrayHandler(key));
        }

        /// <summary>
        /// DictionaryHandler长度访问器
        /// </summary>
        public int Count
        {
            get
            {
                return dict.Count;
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DictionaryHandler()
        {
            dict = new SortedDictionary<ByteArrayHandler, Handler>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dHandler">ByteArray, Handler字典</param>
        public DictionaryHandler(IDictionary<ByteArrayHandler, Handler> dictionaryHandler)
        {
            dict = dictionaryHandler;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key">Handler关键字</param>
        /// <param name="value">Handler节点</param>
        public DictionaryHandler(ByteArrayHandler key, Handler value) 
            : this() 
        {
            dict.Add(key, value);
        }

        /// <summary>
        /// Handler字典类的解码函数
        /// </summary>
        /// <param name="bytes">待解码的字节数组</param>
        /// <param name="position">字节数组的解码位置</param>
        /// <returns>解码的字节数组长度</returns>
        public override int Decode(byte[] source, ref int position)
        {
            //保存初始位置
            int start = position;

            //跳过字符'd'
            position++;

            try
            {
                //当遇到'e'(ASCII码为101),解析结束
                while (source[position] != 101)
                {
                    byte[] key;

                    //解析字符串
                    ByteArrayHandler keyHandler = new ByteArrayHandler();
                    keyHandler.Decode(source, ref position);
                    key = keyHandler.ByteArrayValue;
                    if (key.LongLength == 0)
                        throw new BitTorrentException("待添加的字符串长度为0");

                    //解析Handler
                    Handler valueHandler = BEncode.Decode(source, ref position);

                    //'e'(ASCII码为101),解析结束
                    if (valueHandler == null)
                        throw new BitTorrentException("待添加的Handler节点为空");

                    //字典添加key和handler
                    dict.Add(keyHandler, valueHandler);
                }
            }

            //当捕捉IndexOutOfRangeException,抛出BitTorrentException
            catch (IndexOutOfRangeException)
            {
                throw new BitTorrentException("BEnocde字典类的字符串长度异常");
            }

            //当捕捉ArgumentException,抛出BitTorrentException
            catch (ArgumentException)
            {
                throw new BitTorrentException("BEnocde字典类中包含相同的字符串关键字");
            }

            //跳过字符'e'
            position++;

            //返回所解析的数组长度
            return position - start;
        }

        /// <summary>
        /// Handler字典类的编码函数
        /// </summary>
        /// <param name="msw">待编码的内存写入流</param>
        public override void Encode(MemoryStream msw)
        {
            //向内存流写入'd'(ASCII码为100)
            msw.WriteByte(100);

            ////获取关键字列表
            //List<string> keys = new List<string>(dict.Keys);

            //对于每一个Handler进行编码
            foreach (ByteArrayHandler key in dict.Keys)
            {
                key.Encode(msw);
                dict[key].Encode(msw);
            }

            //向内存流写入'e'(ASCII码为101)
            msw.WriteByte(101);
        }

        /// <summary>
        /// 添加Handler节点函数,并且关键字为字符串
        /// </summary>
        /// <param name="key">待添加的字符串关键字</param>
        /// <param name="value">待添加的Handler节点</param>
        public void Add(ByteArrayHandler key, Handler value)
        {
            dict.Add(key, value);
        }
    }
}