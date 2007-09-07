namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 标志类
    /// </summary>
    public class Flag
    {
        #region Fields

        /// <summary>
        /// 是否被设置
        /// </summary>
        private bool isSet;

        #endregion

        #region Properties

        /// <summary>
        /// 访问是否被设置
        /// </summary>
        public bool IsSet
        {
            get { return isSet; }
        }

        #endregion

        #region Constructors
        /// <summary>
        /// 构造函数，默认是否被设置为false
        /// </summary>
        public Flag()
        {
            this.isSet = false;
        }
        #endregion

        #region Methods
        /// <summary>
        /// 设置函数
        /// </summary>
        public void Set()
        {
            lock (this)
            {
                isSet = true;
            }
        }

        /// <summary>
        /// 取消设置函数
        /// </summary>
        public void Reset()
        {
            isSet = false;
        }
        #endregion
    }
}
