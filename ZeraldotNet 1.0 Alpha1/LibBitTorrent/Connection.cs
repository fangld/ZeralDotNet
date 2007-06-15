using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZeraldotNet.LibBitTorrent
{
    public class Connection
    {
        public void SendBitField(bool[] bitField)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte(Message.BitField);
            byte[] message = BitField.ToBitField(bitField);
            ms.Write(message, 0, message.Length);
            
        }
    }
}
