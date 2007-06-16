using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class SingleSocket : ISingleDownload
    {
        //public void Write(byte[] 
        #region ISingleDownload Members

        public bool IsSnubbed()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public double GetRate()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Disconnected()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GotChoke()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GotUnchoke()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GotHave(int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void GotHaveBitField(bool[] have)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool GotPiece(int index, int begin, byte[] piece)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Write(byte[] bytes)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
