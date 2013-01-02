using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZeraldotNet.LibBitTorrent.BEncoding;

namespace ZeraldotNet.LibBitTorrent.Storages
{
    public abstract class Storage : IDisposable
    {
        #region Methods

        public static Storage Create(MetaInfo metaInfo, string saveAsDirectory)
        {
            Storage result;
            if (metaInfo.Mode == MetaInfoMode.SingleFile)
            {
                result = new SingleFileStorage(metaInfo, saveAsDirectory);
            }
            else
            {
                result = new MultiFileStorage(metaInfo, saveAsDirectory);
            }
            return result;
        }

        public abstract void Write(byte[] buffer, long offset);

        public abstract int Read(byte[] buffer, long offset, int count);

        public abstract void Close();

        public abstract void Dispose();

        #endregion
    }
}
