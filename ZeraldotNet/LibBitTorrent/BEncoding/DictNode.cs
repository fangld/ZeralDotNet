using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    /// <summary>
    /// Handler字典类
    /// </summary>
    public class DictNode : BEncodedNode, IDictionary<BytesNode, BEncodedNode>
    {
        #region Fields

        /// <summary>
        /// string, Handler字典
        /// </summary>
        private readonly IDictionary<BytesNode, BEncodedNode> dict;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public DictNode()
        {
            dict = new SortedDictionary<BytesNode, BEncodedNode>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dictionaryHandler">ByteArray, Handler字典</param>
        public DictNode(IDictionary<BytesNode, BEncodedNode> dictionaryHandler)
        {
            dict = dictionaryHandler;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key">Handler关键字</param>
        /// <param name="value">Handler节点</param>
        public DictNode(BytesNode key, BEncodedNode value) 
            : this() 
        {
            dict.Add(key, value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 添加Handler节点函数,并且关键字为字符串
        /// </summary>
        /// <param name="key">待添加的字符串关键字</param>
        /// <param name="value">待添加的Handler节点</param>
        public void Add(BytesNode key, BEncodedNode value)
        {
            dict.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return this.ContainsKey(new BytesNode(key));
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Handler字典类的解码函数
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
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
                    BytesNode keyHandler = new BytesNode();
                    keyHandler.Decode(source, ref position);
                    key = keyHandler.ByteArray;
                    if (key.LongLength == 0)
                    {
                        throw new BitTorrentException("待添加的字符串长度为0");
                    }

                    //解析Handler
                    BEncodedNode valueHandler = BEncode.Decode(source, ref position);

                    //'e'(ASCII码为101),解析结束
                    if (valueHandler == null)
                    {
                        throw new BitTorrentException("待添加的Handler节点为空");
                    }

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
            foreach (BytesNode key in dict.Keys)
            {
                key.Encode(msw);
                dict[key].Encode(msw);
            }

            //向内存流写入'e'(ASCII码为101)
            msw.WriteByte(101);
        }

        #endregion

        #region IDictionary<BytestringHandler,Handler> Members

        public bool ContainsKey(BytesNode key)
        {
            return this.dict.ContainsKey(key);
        }

        public ICollection<BytesNode> Keys
        {
            get { return this.dict.Keys; }
        }

        public bool Remove(BytesNode key)
        {
            return this.dict.Remove(key);
        }

        public bool TryGetValue(BytesNode key, out BEncodedNode value)
        {
            return this.dict.TryGetValue(key, out value);
        }

        public ICollection<BEncodedNode> Values
        {
            get { return this.dict.Values; }
        }

        public BEncodedNode this[BytesNode key]
        {
            get
            {
                return this.dict[key];
            }
            set
            {
                this.dict[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<BytesHandler,Handler>> Members

        public void Add(KeyValuePair<BytesNode, BEncodedNode> item)
        {
            this.dict.Add(item);
        }

        public void Clear()
        {
            this.dict.Clear();
        }

        public bool Contains(KeyValuePair<BytesNode, BEncodedNode> item)
        {
            return this.Contains(item);
        }

        public void CopyTo(KeyValuePair<BytesNode, BEncodedNode>[] array, int arrayIndex)
        {
            this.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.dict.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<BytesNode, BEncodedNode> item)
        {
            return this.Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<BytesHandler,Handler>> Members

        public IEnumerator<KeyValuePair<BytesNode, BEncodedNode>> GetEnumerator()
        {
            return this.dict.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.dict.GetEnumerator();
        }

        #endregion
    }
}