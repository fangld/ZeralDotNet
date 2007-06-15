using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZeraldotNet.LibBitTorrent
{
    public class Connection
    {
        private En

        private Upload upload;

        public Upload Upload
        {
            get { return this.upload; }
        }

        private Connecter connecter;

        private bool gotAnything;

        public bool GotAnything
        {
            get { return this.gotAnything; }
            set { this.gotAnything = value; }
        }

        public void SendBitField(bool[] bitField)
        {
            MemoryStream ms = new MemoryStream();
            ms.WriteByte((byte)Message.BitField);
            byte[] message = BitField.ToBitField(bitField);
            ms.Write(message, 0, message.Length);


            
        }
    }
}
