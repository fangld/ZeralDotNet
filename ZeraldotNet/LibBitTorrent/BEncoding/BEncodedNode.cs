using System.IO;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    /// <summary>
    /// Handler基类
    /// </summary>
    public abstract class BEncodedNode
    { 
        #region Methods

        public static implicit operator BEncodedNode(string value)
        {
            return new BytesNode(value);
        }

        public static implicit operator BEncodedNode(byte[] value)
        {
            return new BytesNode(value);
        }

        public static implicit operator BEncodedNode(long value)
        {
            return new IntNode(value);
        }

        #endregion

        #region Base Methods

        /// <summary>
        /// Handler基类的解码函数
        /// </summary>
        /// <param name="source">待解码的字节数组</param>
        /// <param name="position">字节数组的解码位置</param>
        public abstract int Decode(byte[] source, ref int position);

        /// <summary>
        /// Handler基类的编码函数
        /// </summary>
        /// <param name="msw">待编码的内存写入流</param>
        public abstract void Encode(MemoryStream msw);

        #endregion

        #region Overriden Methods

        public override string ToString()
        {
            MemoryStream msw = new MemoryStream();
            this.Encode(msw);
            return Encoding.Default.GetString(msw.ToArray());
        }

        #endregion
    }
}