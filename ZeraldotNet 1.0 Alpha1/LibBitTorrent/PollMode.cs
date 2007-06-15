using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public enum PollMode
    {
        PollIn = 1,
        PollOut = 2,
        PollInOut = 3,
        PollError = 8,
        PollHangUp = 16
    }
}
