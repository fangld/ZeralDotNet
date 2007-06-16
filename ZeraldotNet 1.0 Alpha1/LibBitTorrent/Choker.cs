using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 阻塞类
    /// </summary>
    public class Choker
    {
        /// <summary>
        /// 最大连接数量
        /// </summary>
        private int maxUploads;

        /// <summary>
        /// 设置最大连接数量
        /// </summary>
        public int MaxUploads
        {
            set { this.maxUploads = value;}
        }

        /// <summary>
        /// 计划函数
        /// </summary>
        private SchedulerDelegate scheduleFunction;

        /// <summary>
        /// 设置计划函数
        /// </summary>
        public SchedulerDelegate ScheduleFunction
        {
            set { this.scheduleFunction = value; }
        }

        /// <summary>
        /// 连接列表
        /// </summary>
        private List<Connection> connections;

        /// <summary>
        /// 访问连接列表
        /// </summary>
        /// <returns>返回访问连接列表</returns>
        public List<Connection> GetConnections()
        {
            return connections;
        }

        /// <summary>
        /// 循环次数
        /// </summary>
        private int count;

        /// <summary>
        /// 设置循环次数
        /// </summary>
        public int Count
        {
            set { this.count = value; }
        }

        /// <summary>
        /// 完成标志
        /// </summary>
        private Flag done;

        /// <summary>
        /// 设置完成标志
        /// </summary>
        public Flag Done
        {
            set { this.done = value; }
        }

        /// <summary>
        /// 随机数产生类
        /// </summary>
        private Random ran;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxUploads">最大连接数量</param>
        /// <param name="schedule">计划函数</param>
        /// <param name="done">完成标志</param>
        public Choker(int maxUploads, SchedulerDelegate scheduleFunction, Flag done)
        {
            MaxUploads = maxUploads;
            ScheduleFunction = scheduleFunction;
            Done = done;
            count = 0;
            connections = new List<Connection>();
            scheduleFunction(new TaskDelegate(RoundRobin), 10, "Round Robin");
            ran = new Random();
        }

        private void RoundRobin()
        {
            scheduleFunction(new TaskDelegate(RoundRobin), 10, "Round Robin");
            count++;
            if (count % 3 == 0)
            {
                int i;
                for (i = 0; i < connections.Count; i++)
                {
                    Upload upload = connections[i].Upload;

                    if (upload.Choked && upload.Interested)
                    {
                        List<Connection> newConnection = new List<Connection>(connections.Count);
                        newConnection.AddRange(connections.GetRange(i, connections.Count - i));
                        newConnection.AddRange(connections.GetRange(0, i));
                        connections = newConnection;
                        break;
                    }
                }
            }

            Rechoke();
        }

        /// <summary>
        /// 是否拒绝该连接
        /// </summary>
        /// <param name="conn">检验的连接</param>
        /// <returns>返回是否拒绝该连接</returns>
        private bool Snubbed(Connection conn)
        {
            //如果已经完成，则返回false
            if (done.IsSet)
            {
                return false;
            }
            
            //否则返回连接是否被拒绝
            return conn.Download.Snubbed;
        }

        private double Rate(Connection conn)
        {
            //如果已经完成，则返回上传速率
            if (done.IsSet)
            {
                return conn.Upload.Rate;
            }

            //否则返回下载速率
            return conn.Download.Rate;
        }

        /// <summary>
        /// 重阻塞
        /// </summary>
        private void Rechoke()
        {
            List<ConnectionRate> prefferConnRate = new List<ConnectionRate>();
            List<Connection> prefferConn;
            int count = 0;

            foreach (Connection conn in connections)
            {
                if (!Snubbed(conn) && conn.Upload.Interested)
                {
                    prefferConnRate.Add(new ConnectionRate(-Rate(conn), conn));
                    count++;
                }
            }

            prefferConn = new List<Connection>(count);

            prefferConnRate.Sort();

            if (prefferConnRate.Count > maxUploads - 1)
            {
                prefferConnRate = prefferConnRate.GetRange(0, maxUploads - 1);
            }

            int i;
            for (i = 0; i <prefferConnRate.Count; i++)
            {
                prefferConn[i] = prefferConnRate[i].Conn;
            }

            count = prefferConnRate.Count;

            foreach (Connection conn in connections)
            {
                Upload upload = conn.Upload;
                if (prefferConn.Contains(conn))
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
                        upload.Unchoke();
                    }
                }
            }
        }

        /// <summary>
        /// 建立连接，不指定连接次序
        /// </summary>
        /// <param name="conn"></param>
        public void MakeConnection(Connection conn)
        {
            int index = ran.Next(-2, connections.Count + 1);
            MakeConnection(conn, (int)Math.Max(index, 0));
        }

        /// <summary>
        /// 建立连接，并且指定所连接的次序
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="index"></param>
        public void MakeConnection(Connection conn, int index)
        {
            connections.Insert(index, conn);
            Rechoke();
        }

        /// <summary>
        /// 丢失连接
        /// </summary>
        /// <param name="conn">所丢失连接</param>
        public void LoseConnection(Connection conn)
        {
            connections.Remove(conn);

            //如果丢失，则重阻塞
            if (conn.Upload.Interested && !conn.Upload.Choked)
                Rechoke();
        }

        public void NotInterested(Connection conn)
        {
            if (!conn.Upload.Choked)
                Rechoke();
        }

        public void Interested(Connection conn)
        {
            NotInterested(conn);
        }
    }
}
