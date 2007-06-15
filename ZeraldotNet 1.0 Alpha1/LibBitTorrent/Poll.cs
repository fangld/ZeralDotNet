using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 
    /// </summary>
    public class Poll
    {
        /// <summary>
        /// 可读的Socket列表
        /// </summary>
        private List<Socket> readList;

        /// <summary>
        /// 可写的Socket列表
        /// </summary>
        private List<Socket> writeList;

        /// <summary>
        /// 构造函数
        /// </summary>
        public Poll()
        {
            readList = new List<Socket>();
            writeList = new List<Socket>();
        }

        /// <summary>
        /// 注册Socket
        /// </summary>
        /// <param name="socket">待注册的Socket</param>
        /// <param name="mode">注册的状态</param>
        public void Register(Socket socket, PollMode mode)
        {
            //如果注册的状态为可写
            RegisterReadSocket(socket, mode);

            //如果注册的状态为可读
            RegisterWriteSocket(socket, mode);
        }

        /// <summary>
        /// 注册可写的Socket
        /// </summary>
        /// <param name="socket">待注册可写的Socket</param>
        /// <param name="mode">注册状态</param>
        private void RegisterWriteSocket(Socket socket, PollMode mode)
        {
            //如果注册状态为可写
            if ((mode & PollMode.PollOut) != 0)
            {
                if (!writeList.Contains(socket))
                {
                    writeList.Add(socket);
                }
            }
            
            //否则取消Socket
            else
            {
                writeList.Remove(socket);
            }
        }

        /// <summary>
        /// 注册可读的Socket
        /// </summary>
        /// <param name="socket">待注册可读的Socket</param>
        /// <param name="mode">注册状态</param>
        private void RegisterReadSocket(Socket socket, PollMode mode)
        {
            //如果注册状态为可读
            if ((mode & PollMode.PollIn) != 0)
            {
                if (!readList.Contains(socket))
                {
                    readList.Add(socket);
                }
            }

            //否则取消Socket
            else
            {
                readList.Remove(socket);
            }
        }

        /// <summary>
        /// 取消Socket
        /// </summary>
        /// <param name="socket">待取消的Socket</param>
        public void Unregister(Socket socket)
        {
            readList.Remove(socket);
            writeList.Remove(socket);
        }

        /// <summary>
        /// 判断Socket的状态
        /// </summary>
        /// <param name="timeout">测试Socket的连接时间，-1代表无限时间</param>
        /// <returns>返回Socket及其读写状态</returns>
        public List<PollItem> Select(int timeout)
        {
            List<PollItem> result = new List<PollItem>();

            //如果可读Socket列表或者可写Socket列表不为空
            if (readList.Count > 0 || writeList.Count > 0)
            {
                List<Socket> checkRead = new List<Socket>(readList);
                List<Socket> checkWrite = new List<Socket>(writeList);

                //调用Socket.Select函数判断Socket的状态
                Socket.Select(checkRead, checkWrite, null, timeout);

                //将可读状态的列表添加返回结果中
                foreach (Socket item in checkRead)
                {
                    result.Add(new PollItem(item, PollMode.PollIn));
                }

                //将可写状态的列表添加返回结果中
                foreach (Socket item in checkWrite)
                {
                    result.Add(new PollItem(item, PollMode.PollOut));
                }
            }

            //否则，休眠timeout毫秒
            else
            {
                Thread.Sleep(timeout);
            }

            return result;
        }
    }
}
