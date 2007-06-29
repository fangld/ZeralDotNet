using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    /// <summary>
    /// Handler列表类
    /// </summary>
    public class ListHandler : Handler
    {
        #region Private Field
        /// <summary>
        /// Handler列表
        /// </summary>
        private IList<Handler> item;
        #endregion

        #region Public Properties
        /// <summary>
        /// Handler列表索引器,索引为整数
        /// </summary>
        /// <param name="index">整数索引</param>
        /// <returns>Handler节点</returns>
        public Handler this[int index]
        {
            get { return item[index]; }
        }

        /// <summary>
        /// ListHandler长度访问器
        /// </summary>
        public int Count
        {
            get { return item.Count; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// 构造函数,定义元素类型为列表类型
        /// </summary>
        public ListHandler()
        {
            item = new List<Handler>();
        }

        /// <summary>
        /// 构造函数,定义元素类型为列表类型
        /// </summary>
        /// <param name="lHandler">Handler列表</param>
        public ListHandler(IList<Handler> lHandler)
        {
            item = lHandler;
        }
        #endregion

        #region Methods
        /// <summary>
        /// 添加Handler节点函数
        /// </summary>
        /// <param name="listHandler">待添加的节点</param>
        public void Add(Handler handler)
        {
            item.Add(handler);
        }
                #endregion

        #region Overriden Methods
        /// <summary>
        /// Handler列表类的解码函数
        /// </summary>
        /// <param name="bytes">待解码的字节数组</param>
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
                    Handler handler = BEncode.Decode(source, ref position);

                    //当遇到'e'(ASCII码为101),解析结束
                    if (handler == null)
                        break;

                    //列表添加handler
                    item.Add(handler);
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
            foreach (Handler bh in item)
            {
                bh.Encode(msw);
            }

            //向内存流写入'e'(ASCII码为101)
            msw.WriteByte(101);
        }
        #endregion
    }
}
