using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Encrypters;
using ZeraldotNet.LibBitTorrent.RawServers;

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

        public void ListenForever(IEncrypter encrypter)
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

        public ISingleSocket StartConnect(IPEndPoint dns, IEncrypter encrypter)
        {
            DummySingleSocket c = new DummySingleSocket();
            connects.Add(new object[] { dns, c });
            return c;
        }

        #endregion
    }
}
