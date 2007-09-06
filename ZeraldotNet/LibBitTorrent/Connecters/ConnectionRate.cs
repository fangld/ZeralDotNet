using System;

namespace ZeraldotNet.LibBitTorrent.Connecters
{
   /// <summary>
   /// 连接速率类
   /// </summary>
    public class ConnectionRate : IComparable<ConnectionRate>
    {
        #region Fields

        /// <summary>
        /// 连接速率
        /// </summary>
        private double rate;

        /// <summary>
        /// 连接类
        /// </summary>
        private IConnection connection;

        #endregion

        #region Properties

        /// <summary>
        /// 访问和设置连接速率
        /// </summary>
        public double Rate
        {
            get { return this.rate; }
            set { this.rate = value; }
        }

        /// <summary>
        /// 连接类
        /// </summary>
        public IConnection Connection
        {
            get { return this.connection; }
            set { this.connection = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rate">连接速率</param>
        /// <param name="connection">连接类</param>
        public ConnectionRate(double rate, IConnection connection)
        {
            Rate = rate;
            Connection = connection;
        }

        #endregion

        #region Overriden Methods

        #region IComparable<ConnectionRate> Members

        public int CompareTo(ConnectionRate other)
        {
            return rate.CompareTo(other.rate);
        }

        #endregion

        #endregion
    }
}
