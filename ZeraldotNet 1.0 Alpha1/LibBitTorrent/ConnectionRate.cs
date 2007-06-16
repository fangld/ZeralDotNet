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
        /// <summary>
        /// 连接速率
        /// </summary>
        private double rate;


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
        private Connection conn;

        /// <summary>
        /// 连接类
        /// </summary>
        public Connection Conn
        {
            get { return this.conn; }
            set { this.conn = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rate">连接速率</param>
        /// <param name="conn">连接类</param>
        public ConnectionRate(double rate, Connection conn)
        {
            Rate = rate;
            Conn = conn;
        }
        
        #region IComparable<ConnectionRate> Members

        public int CompareTo(ConnectionRate other)
        {
            return rate.CompareTo(other.rate);
        }

        #endregion
    }
}
