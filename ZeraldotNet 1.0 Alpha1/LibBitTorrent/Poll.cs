using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ZeraldotNet.LibBitTorrent
{
    public class Poll
    {
        private List<Socket> readList;

        private List<Socket> writeList;

        public Poll()
        {
            readList = new List<Socket>();
            writeList = new List<Socket>();
        }

        public void Register(Socket socket, PollMode mode)
        {
            if ((mode & PollMode.PollIn) != 0)
            {
                if (!readList.Contains(socket))
                {
                    readList.Add(socket);
                }
            }
            else
            {
                readList.Remove(socket);
            }

            if ((mode & PollMode.PollOut) != 0)
            {
                if (!writeList.Contains(socket))
                {
                    writeList.Add(socket);
                }
            }
            else
            {
                writeList.Remove(socket);
            }
        }

        public void Unregister(Socket socket)
        {
            readList.Remove(socket);
            writeList.Remove(socket);
        }

        public List<PollItem> PollRun(int timeout)
        {
            List<PollItem> result = new List<PollItem>();
            if (readList.Count > 0 || writeList.Count > 0)
            {
                List<Socket> tempReadList = new List<Socket>(readList);
                List<Socket> tempWriteList = new List<Socket>(writeList);

                Socket.Select(tempReadList, tempWriteList, null, timeout);

                foreach (Socket item in tempReadList)
                {
                    result.Add(new PollItem(item, PollMode.PollIn));
                }

                foreach (Socket item in tempWriteList)
                {
                    result.Add(new PollItem(item, PollMode.PollOut));
                }
            }

            else
            {
                Thread.Sleep(timeout);
            }

            return result;
        }
    }
}
