using System;
using System.Net;
using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Encrypters;

namespace ZeraldotNet.LibBitTorrent.RawServers
{    
    public interface IRawServer
    {
        void AddExternalTask(TaskDelegate taskFunction, double delay, string taskName);
        void AddTask(TaskDelegate taskFunction, double delay, string taskName);
        void AddToDeadFromWrite(ISingleSocket item);
        void Bind(int port, string bind, bool reuse);
        void HandleEvents(List<PollItem> events);
        IEncrypter Handler { get; set; }
        void ListenForever(IEncrypter encrypter);
        Poll Poll { get; set; }
        void PopExternal();
        void RemoveSingleSockets(IntPtr key);
        void ScanForTimeouts();
        ISingleSocket StartConnect(IPEndPoint dns, IEncrypter encrypter);
    }
}
