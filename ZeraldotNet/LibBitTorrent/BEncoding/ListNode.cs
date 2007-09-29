using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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
        private readonly IList<BEncodedNode> items;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数,定义元素类型为列表类型
        /// </summary>
        public ListNode()
        {
            items = new List<BEncodedNode>();
        }

        /// <summary>
        /// 构造函数,定义元素类型为列表类型
        /// </summary>
        /// <param name="lHandler">Handler列表</param>
        public ListNode(IList<BEncodedNode> lHandler)
        {
            items = lHandler;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 添加Handler节点函数
        /// </summary>
        /// <param name="handler">待添加的节点</param>
        public void Add(BEncodedNode handler)
        {
            items.Add(handler);
        }

        #endregion

        #region Overriden Methods

        /// <summary>
        /// Handler列表类的解码函数
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
        /// <param name="position">字节数组的解码位置</param>
        /// <returns>解码的字节数组长度</returns>
        public override int Decode(byte[] source, ref int position)
        {
            //保存初始位置
            int start = position;

            //跳过字符'l'
            position++;

            try
            {
                //当遇到'e'(ASCII码为101),解析结束
                while (source[position] != 101)
                {
                    BEncodedNode handler = BEncode.Decode(source, ref position);

                    //当遇到'e'(ASCII码为101),解析结束
                    if (handler == null)
                        break;

                    //列表添加handler
                    items.Add(handler);
                }
            }

            //当捕捉IndexOutOfRangeException,抛出BitTorrentException
            catch (IndexOutOfRangeException)
            {
                throw new BitTorrentException("BEnocde列表类的字节数组长度异常");
            }

            //跳过字符'e'
            position++;

            //返回所解析的数组长度
            return position - start;
        }

        /// <summary>
        /// Handler列表类的解码函数
        /// </summary>
        /// <param name="msw">待解码的内存写入流</param>
        public override void Encode(MemoryStream msw)
        {
            //向内存流写入'l'(ASCII码为108)
            msw.WriteByte(108);

            //对于每一个Handler进行编码
            foreach (BEncodedNode bh in items)
            {
                bh.Encode(msw);
            }

            //向内存流写入'e'(ASCII码为101)
            msw.WriteByte(101);
        }

        #endregion

        #region IList<Handler> Members

        public int IndexOf(BEncodedNode item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, BEncodedNode item)
        {
            this.items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            this.items.RemoveAt(index);
        }

        public BEncodedNode this[int index]
        {
            get
            {
                return this.items[index];
            }
            set
            {
                this.items[index] = value;
            }
        }

        #endregion

        #region ICollection<Handler> Members

        public void Clear()
        {
            this.items.Clear();
        }

        public bool Contains(BEncodedNode item)
        {
            return this.items.Contains(item);
        }

        public void CopyTo(BEncodedNode[] array, int arrayIndex)
        {
            this.items.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return this.items.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.items.IsReadOnly; }
        }

        public bool Remove(BEncodedNode item)
        {
            return this.items.Remove(item);
        }

        #endregion

        #region IEnumerable<Handler> Members

        public IEnumerator<BEncodedNode> GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.items.GetEnumerator();
        }

        #endregion
    }
}
