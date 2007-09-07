namespace ZeraldotNet.LibBitTorrent.ReadFunctions
{
    /// <summary>
    /// 分析字节流类
    /// </summary>
    public abstract class ReadFunction
    {
        #region Fields

        /// <summary>
        /// 分析的长度
        /// </summary>
        private int length;

        /// <summary>
        /// 下一个分析类
        /// </summary>
        private ReadFunction next;

        #endregion

        #region Properties

        /// <summary>
        /// 访问和设置分析的长度
        /// </summary>
        public int Length
        {
            get { return this.length; }
            set { this.length = value; }
        }

        /// <summary>
        /// 访问和设置下一个分析类
        /// </summary>
        public ReadFunction Next
        {
            get { return this.next; }
            set { this.next = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="length">分析的长度</param>
        /// <param name="next">下一个分析类</param>
        public ReadFunction(int length, ReadFunction next)
        {
            this.length = length;
            this.next = next;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 分析字节流
        /// </summary>
        /// <param name="bytes">待分析的字节流</param>
        /// <returns>如果字节流正确，返回true，否则返回false</returns>
        public abstract bool ReadBytes(byte[] bytes);

        #endregion
    }
}
