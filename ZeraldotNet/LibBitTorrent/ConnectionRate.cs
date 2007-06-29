using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
   /// <summary>
   /// 连接速率类
   /// </summary>
    public class ConnectionRate : IComparable<ConnectionRate>
    {
        #region Private Field

        /// <summary>
        /// 连接速率
        /// </summary>
        private double rate;

        /// <summary>
        /// 连接类
        /// </summary>
        private Connection connection;

        #endregion

        #region Public Properties

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
        public Connection Connection
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
        public ConnectionRate(double rate, Connection connection)
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
