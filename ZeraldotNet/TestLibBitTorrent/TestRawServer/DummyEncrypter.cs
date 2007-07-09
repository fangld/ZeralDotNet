using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.TestLibBitTorrent.TestRawServer
{
    public class DummyEncrypter
    {
        public List<DummySingleSocket> externalDummySockets;
        public ArrayList dataIn;
        public List<DummySingleSocket> lostDummySockets;

        public DummyEncrypter()
        {
            externalDummySockets = new List<DummySingleSocket>();
            dataIn = new ArrayList();
            lostDummySockets = new List<DummySingleSocket>();
        }

        public void MakeExternalConnection(DummySingleSocket s)
        {
            externalDummySockets.Add(s);
        }

        public void DataCameIn(DummySingleSocket s, byte[] data)
        {
            dataIn.Add(new object[] { s, data });
        }

        public void LoseConnection(DummySingleSocket s)
        {
            lostDummySockets.Add(s);
        }

        public void FlushConnection(DummySingleSocket s)
        {
        }
    }
}
