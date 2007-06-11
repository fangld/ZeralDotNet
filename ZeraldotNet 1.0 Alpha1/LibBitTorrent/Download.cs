using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate void DataFlunkedDelegate(long length);
    public delegate void FinishedDelegate();
    public delegate void FailedDelegate(string message);
    public delegate bool PendingDelegate();

    class Download
    {
    }
}
