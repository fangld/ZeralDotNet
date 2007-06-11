using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate void StatusDelegate(string message, double downloadRate, double uploadRate, double fractionDone, double timeEstimate);
    public delegate void ErrorDelegate(string message);

    public class Connecter
    {
    }
}
