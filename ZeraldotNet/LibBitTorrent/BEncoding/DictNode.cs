using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    /// <summary>
    /// Handler字典类
    /// </summary>
    public class DictNode : BEncodedNode, IDictionary<string, BEncodedNode>
    {
        #region Fields

        /// <summary>
        /// string, Handler字典
        /// </summary>
        private readonly IDictionary<string, BEncodedNode> _dict;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public DictNode()
        {
            _dict = new SortedDictionary<string, BEncodedNode>();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dictionaryHandler">string, BEncodedNode字典</param>
        public DictNode(IDictionary<string, BEncodedNode> dictionaryHandler)
        {
            _dict = dictionaryHandler;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="key">关键字</param>
        /// <param name="value">Node节点</param>
        public DictNode(string key, BEncodedNode value)
            : this()
        {
            _dict.Add(key, value);
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Node字典类的解码函数
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
                    //解析字符串
                    BytesNode keyNode = new BytesNode();
                    keyNode.Decode(source, ref position);
                    if (keyNode.ByteArray.LongLength == 0)
                    {
                        throw new BitTorrentException("待添加的字符串长度为0");
                    }

                    //解析Handler
                    BEncodedNode valueNode = BEncoder.Decode(source, ref position);

                    //'e'(ASCII码为101),解析结束
                    if (valueNode == null)
                    {
                        throw new BitTorrentException("待添加的Handler节点为空");
                    }

                    //字典添加key和value
                    _dict.Add(keyNode.StringText, valueNode);
                }
            }

            //当捕捉IndexOutOfRangeException,抛出BitTorrentException
            catch (IndexOutOfRangeException)
            {
                throw new BitTorrentException("BEncode字典类的字符串长度异常");
            }

            //当捕捉ArgumentException,抛出BitTorrentException
            catch (ArgumentException)
            {
                throw new BitTorrentException("BEncode字典类中包含相同的字符串关键字");
            }

            //跳过字符'e'
            position++;

            //返回所解析的数组长度
            return position - start;
        }

        /// <summary>
        /// Handler字典类的编码函数
        /// </summary>
        /// <param name="ms">待编码的内存写入流</param>
        public override void Encode(MemoryStream ms)
        {
            //向内存流写入'd'(ASCII码为100)
            ms.WriteByte(100);

            //对于每一个node进行编码
            foreach (string key in _dict.Keys)
            {
                BytesNode keyNode = new BytesNode(key, Encoding.UTF8);
                keyNode.Encode(ms);
                _dict[key].Encode(ms);
            }

            //向内存流写入'e'(ASCII码为101)
            ms.WriteByte(101);
        }

        public override void SetEncoding(Encoding encoding)
        {
            _dict.Values.AsParallel<BEncodedNode>().ForAll<BEncodedNode>(delegate(BEncodedNode node) { node.SetEncoding(encoding); });
        }

        #endregion

        #region IDictionary<BytesNode, BEncodedNode> Members

        public void Add(string key, BEncodedNode value)
        {
            _dict.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return _dict.Keys; }
        }

        public bool Remove(string key)
        {
            return _dict.Remove(key);
        }

        public bool TryGetValue(string key, out BEncodedNode value)
        {
            return _dict.TryGetValue(key, out value);
        }

        public ICollection<BEncodedNode> Values
        {
            get { return _dict.Values; }
        }

        public BEncodedNode this[string key]
        {
            get
            {
                return _dict[key];
            }
            set
            {
                _dict[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string, BEncodedNode>> Members

        public void Add(KeyValuePair<string, BEncodedNode> item)
        {
            _dict.Add(item);
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool Contains(KeyValuePair<string, BEncodedNode> item)
        {
            return _dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, BEncodedNode>[] array, int arrayIndex)
        {
            _dict.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _dict.Count; }
        }

        public bool IsReadOnly
        {
            get { return _dict.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, BEncodedNode> item)
        {
            return _dict.Remove(item);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string, BEncodedNode>> Members

        public IEnumerator<KeyValuePair<string, BEncodedNode>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        #endregion
    }
}