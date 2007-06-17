using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate NextFunction FuncDelegate(byte[] bytes);
    public delegate void StartDelegate(IPEndPoint dns, byte[] id);

    public class NextFunction
    {
    }
}
