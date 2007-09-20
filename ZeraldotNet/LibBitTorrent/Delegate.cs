using System.Net;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate void DataFlunkedDelegate(long length);
    public delegate void FinishedDelegate();
    public delegate void FailedDelegate(string message);
    public delegate bool PendingDelegate();
    public delegate void StatusDelegate(string message, double downloadRate, double uploadRate, double fractionDone, double timeEstimate);
    public delegate void ErrorDelegate(string message);
    public delegate bool WantDelegate(int piece);
    public delegate void TaskDelegate();
    public delegate void SchedulerDelegate(TaskDelegate func, double delay, string TaskName);
    public delegate void DataDelegate(long amount);
    public delegate long MeasureTotalDelegate();
    public delegate double MeasureRateDelegate();
    public delegate int HowManyDelegate();
    public delegate long AmountDelegate();
    public delegate void StartDelegate(IPEndPoint dns, byte[] id);
}
