using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net;
using ZeraldotNet.LibBitTorrent.RawServers;
using ZeraldotNet.LibBitTorrent.Encrypters;
using ZeraldotNet.LibBitTorrent;

namespace ZeraldotNet.TestLibBitTorrent.TestEncrypter
{
    class DummyRawServer : IRawServer
    {
        public ArrayList connects;

        public DummyRawServer()
        {
            connects = new ArrayList();
        }

        #region IRawServer Members

        public void AddExternalTask(TaskDelegate taskFunction, double delay, string taskName)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void AddTask(TaskDelegate taskFunction, double delay)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void AddToDeadFromWrite(ISingleSocket item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Bind(int port, string bind, bool reuse)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void HandleEvents(List<PollItem> events)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IEncrypter Handler
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void ListenForever(IEncrypter handler)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Poll Poll
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public void PopExternal()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void RemoveSingleSockets(IntPtr key)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void ScanForTimeouts()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public ISingleSocket StartConnect(IPEndPoint dns, IEncrypter handler)
        {
            DummySingleSocket c = new DummySingleSocket();
            connects.Add(new object[] { dns, c });
            return c;
        }

        #endregion
    }
}
