using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Uploads;

namespace ZeraldotNet.LibBitTorrent.Chokers
{
    /// <summary>
    /// 阻塞类
    /// </summary>
    public class Choker : IChoker
    {
        #region Private Field
        /// <summary>
        /// 最大连接数量
        /// </summary>
        private int maxUploads;

        /// <summary>
        /// 计划函数
        /// </summary>
        private SchedulerDelegate scheduleFunction;

        /// <summary>
        /// 连接列表
        /// </summary>
        private List<IConnection> connections;

        /// <summary>
        /// 循环次数
        /// </summary>
        private int count;

        /// <summary>
        /// 完成标志
        /// </summary>
        private Flag doneFlag;

        /// <summary>
        /// 随机数产生类
        /// </summary>
        private Random ran;
        #endregion

        #region Constructors
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxUploads">最大连接数量</param>
        /// <param name="schedule">计划函数</param>
        /// <param name="done">完成标志</param>
        public Choker(int maxUploads, SchedulerDelegate scheduleFunction, Flag doneFlag)
        {
            this.maxUploads = maxUploads;
            this.scheduleFunction = scheduleFunction;
            this.doneFlag = doneFlag;
            count = 0;
            connections = new List<IConnection>();
            scheduleFunction(new TaskDelegate(RoundRobin), 10, "Round Robin");
            ran = new Random();
        }
        #endregion

        #region Methods

        /// <summary>
        /// 访问连接列表
        /// </summary>
        /// <returns>返回访问连接列表</returns>
        public List<IConnection> GetConnections()
        {
            return connections;
        }

        private void RoundRobin()
        {
            scheduleFunction(new TaskDelegate(RoundRobin), 10, "Round Robin");
            count++;

            if (count % 3 == 0)
            {
                List<IConnection> newConnections;
                IUpload upload;
                int i;
                for (i = 0; i < connections.Count; i++)
                {
                    upload = connections[i].Upload;

                    if (upload.Choked && upload.Interested)
                    {
                        newConnections = new List<IConnection>(connections.Count);
                        newConnections.AddRange(connections.GetRange(i, connections.Count - i));
                        newConnections.AddRange(connections.GetRange(0, i));
                        connections = newConnections;
                        break;
                    }
                }
            }

            Rechoke();
        }

        /// <summary>
        /// 是否拒绝该连接
        /// </summary>
        /// <param name="connection">检验的连接</param>
        /// <returns>返回是否拒绝该连接</returns>
        private bool Snubbed(IConnection connection)
        {
            //如果已经完成，则返回false
            if (doneFlag.IsSet)
            {
                return false;
            }

            //否则返回连接是否被拒绝
            return connection.Download.Snubbed;
        }

        /// <summary>
        /// 返回速率
        /// </summary>
        /// <param name="connection">待测试的连接</param>
        /// <returns>返回该连接的速率</returns>
        private double Rate(IConnection connection)
        {
            //如果已经完成，则返回上传速率
            if (doneFlag.IsSet)
            {
                return connection.Upload.Rate;
            }

            //否则返回下载速率
            return connection.Download.Rate;
        }

        /// <summary>
        /// 再阻塞
        /// </summary>
        private void Rechoke()
        {
            List<ConnectionRate> prefferConnectionRates = new List<ConnectionRate>();
            HashSet<IConnection> prefferConnections = new HashSet<IConnection>();
            int count;

            foreach (IConnection connection in connections)
            {
                if (!Snubbed(connection) && connection.Upload.Interested)
                {
                    prefferConnectionRates.Add(new ConnectionRate(-Rate(connection), connection));
                }
            }

            prefferConnectionRates.Sort();

            if (prefferConnectionRates.Count > maxUploads - 1)
            {
                prefferConnectionRates = prefferConnectionRates.GetRange(0, maxUploads - 1);
            }

            int index;
            for (index = 0; index < prefferConnectionRates.Count; index++)
            {
                prefferConnections.Add(prefferConnectionRates[index].Connection);
            }

            count = prefferConnectionRates.Count;

            IUpload upload;
            foreach (IConnection connection in connections)
            {
                upload = connection.Upload;
                if (prefferConnections.Contains(connection))
                {
                    upload.Unchoke();
                }

                else
                {
                    if (count < maxUploads)
                    {
                        upload.Unchoke();
                        if (upload.Interested)
                        {
                            count++;
                        }
                    }
                    else
                    {
                        upload.Choke();
                    }
                }
            }
        }

        /// <summary>
        /// 建立连接，不指定连接次序
        /// </summary>
        /// <param name="connection"></param>
        public void MakeConnection(IConnection connection)
        {
            int index = ran.Next(-2, connections.Count + 1);
            this.MakeConnection(connection, (int)Math.Max(index, 0));
        }

        /// <summary>
        /// 建立连接，并且指定所连接的次序
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="index"></param>
        public void MakeConnection(IConnection connection, int index)
        {
            connections.Insert(index, connection);
            this.Rechoke();
        }

        /// <summary>
        /// 丢失连接
        /// </summary>
        /// <param name="connection">所丢失连接</param>
        public void CloseConnection(IConnection connection)
        {
            connections.Remove(connection);

            //如果丢失，则重阻塞
            if (connection.Upload.Interested && !connection.Upload.Choked)
            {
                this.Rechoke();
            }
        }

        public void NotInterested(IConnection connection)
        {
            if (!connection.Upload.Choked)
            {
                this.Rechoke();
            }
        }

        public void Interested(IConnection connection)
        {
            this.NotInterested(connection);
        }

        #endregion
    }
}
