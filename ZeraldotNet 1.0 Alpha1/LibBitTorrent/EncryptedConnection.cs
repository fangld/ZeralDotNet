using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class EncryptedConnection
    {
        private const string protocolName = "BitTorrent protocol";

        private SingleSocket connection;

        private void SendMessage(byte message)
        {
            SendMessage(new byte[] { message });
        }

        private void SendMessage(byte[] message)
        {
            byte[] length = BitConverter.GetBytes(message.Length);
            byte swap;
            swap = length[0];
            length[0] = length[3];
            length[3] = swap;
            swap = length[1];
            length[1] = length[2];
            length[2] = swap;
            connection.Write(length);
            connection.Write(message);
        }
    }
}
