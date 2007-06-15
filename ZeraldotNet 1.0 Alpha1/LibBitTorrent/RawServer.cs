using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate void TaskDelegate();
    public delegate void SchedulerDelegate(TaskDelegate func, double delay, string TaskName);

    class RawServer
    {
    }
}
