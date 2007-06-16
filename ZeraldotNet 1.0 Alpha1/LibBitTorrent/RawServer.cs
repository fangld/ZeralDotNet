using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate void TaskDelegate();
    public delegate void SchedulerDelegate(TaskDelegate func, double delay, string TaskName);

    public class RawServer
    {
        private List<SingleSocket> deadFromWrite;

        private Poll poll;

        public Poll Poll
        {
            get { return this.poll; }
            set { this.poll = value; }
        }


        public void AddToDeadFromWrite(SingleSocket item)
        {
            deadFromWrite.Add(item);
        }
    }
}
