using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Summary description for Choker.
    /// </summary>
    public class Choker
    {
        private int maxUploads;

        public int MaxUploads
        {
            get { return this.maxUploads;}
            set { this.maxUploads = value;}
        }

        private SchedulerDelegate scheduleFunction;

        public SchedulerDelegate ScheduleFunction
        {
            get { return this.scheduleFunction; }
            set { this.scheduleFunction = value; }
        }

        public List<Connection> conections;

        private int count;

        private Flag done;

        public Flag Done
        {
            get { return this.done; }
            set { this.done = value; }
        }

        public Choker(int maxUploads, SchedulerDelegate schedule, Flag done)
        {
            MaxUploads = maxUploads;
            ScheduleFunction = scheduleFunction;
            Done = done;
            count = 0;
            conections = new List<Connection>();
            scheduleFunction(new TaskDelegate(RoundRobin), 10, "Round Robin");
        }

        private void RoundRobin()
        {
            scheduleFunction(new TaskDelegate(RoundRobin), 10, "Round Robin");
            count++;
            if (count % 3 == 0)
            {
                int i;
                for (i = 0; i < conections.Count; i++)
                {
                    Upload upload = conections[i].GetUpload();
                }
            }
        }
    }
}
