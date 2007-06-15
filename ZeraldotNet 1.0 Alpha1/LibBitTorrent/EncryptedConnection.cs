using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class EncryptedConnection
    {
        private const string protocolName = "BitTorrent protocol";

        private void SendMessage(byte[] message)
        {
            byte[] temp = BitConverter.GetBytes(message.Length);
            byte t;
            t = temp[0];
            temp[0] = temp[3];
            temp[3] = t;
            t = temp[1];
            temp[1] = temp[2];
            temp[2] = t;
        }
    }
}
