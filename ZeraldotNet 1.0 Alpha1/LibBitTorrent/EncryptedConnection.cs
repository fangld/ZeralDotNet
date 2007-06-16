using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class EncryptedConnection
    {
        private const string protocolName = "BitTorrent protocol";

        //private Encrypter 

        private SingleSocket connection;

        public void SendMessage(byte message)
        {
            SendMessage(new byte[] { message });
        }

        public void SendMessage(byte[] message)
        {
            byte[] lengthBytes = BitConverter.GetBytes(message.Length);
            byte swap;
            swap = lengthBytes[0];
            lengthBytes[0] = lengthBytes[3];
            lengthBytes[3] = swap;
            swap = lengthBytes[1];
            lengthBytes[1] = lengthBytes[2];
            lengthBytes[2] = swap;
            connection.Write(lengthBytes);
            connection.Write(message);
        }

        public void Close()
        {
            throw new NotImplementedException();
        }
    }
}
