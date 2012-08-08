using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    /// <summary>
    /// Handler列表类
    /// </summary>
    public class ListNode : BEncodedNode, IList<BEncodedNode>
    {
        #region Fields

        /// <summary>
        /// Handler列表
        /// </summary>
        private readonly IList<BEncodedNode> _items;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数,定义元素类型为列表类型
        /// </summary>
        public ListNode()
        {
            _items = new List<BEncodedNode>();
        }

        /// <summary>
        /// 构造函数,定义元素类型为列表类型
        /// </summary>
        /// <param name="nodes">Handler列表</param>
        public ListNode(IList<BEncodedNode> nodes)
        {
            _items = nodes;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 添加Handler节点函数
        /// </summary>
        /// <param name="node">待添加的节点</param>
        public void Add(BEncodedNode node)
        {
            _items.Add(node);
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Handler列表类的解码函数
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
        /// <param name="index">字节数组的解码位置</param>
        /// <returns>解码的字节数组长度</returns>
        public override int Decode(byte[] source, ref int index)
        {
            //保存初始位置
            int start = index;

            //跳过字符'l'
            index++;

            try
            {
                //当遇到'e'(ASCII码为101),解析结束
                while (source[index] != 101)
                {
                    BEncodedNode node = BEncodingFactory.Decode(source, ref index);

                    //当遇到'e'(ASCII码为101),解析结束
                    if (node == null)
                        break;

                    //列表添加handler
                    _items.Add(node);
                }
            }

            //当捕捉IndexOutOfRangeException,抛出BitTorrentException
            catch (IndexOutOfRangeException)
            {
                throw new BitTorrentException("BEnocde列表类的字节数组长度异常");
            }

            //跳过字符'e'
            index++;

            //返回所解析的数组长度
            return index - start;
        }

        /// <summary>
        /// Handler列表类的解码函数
        /// </summary>
        /// <param name="ms">待解码的内存写入流</param>
        public override void Encode(MemoryStream ms)
        {
            //向内存流写入'l'(ASCII码为108)
            ms.WriteByte(108);

            //对于每一个Handler进行编码
            foreach (BEncodedNode node in _items)
            {
                node.Encode(ms);
            }

            //向内存流写入'e'(ASCII码为101)
            ms.WriteByte(101);
        }

        #endregion

        #region IList<Handler> Members

        public int IndexOf(BEncodedNode item)
        {
            return _items.IndexOf(item);
        }

        public void Insert(int index, BEncodedNode item)
        {
            this._items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this._items.RemoveAt(index);
        }

        public BEncodedNode this[int index]
        {
            get
            {
                return this._items[index];
            }
            set
            {
                this._items[index] = value;
            }
        }

        #endregion

        #region ICollection<Handler> Members

        public void Clear()
        {
            this._items.Clear();
        }

        public bool Contains(BEncodedNode item)
        {
            return this._items.Contains(item);
        }

        public void CopyTo(BEncodedNode[] array, int arrayIndex)
        {
            this._items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this._items.Count; }
        }

        public bool IsReadOnly
        {
            get { return this._items.IsReadOnly; }
        }

        public bool Remove(BEncodedNode item)
        {
            return this._items.Remove(item);
        }

        #endregion

        #region IEnumerable<Handler> Members

        public IEnumerator<BEncodedNode> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        #endregion
    }
}
