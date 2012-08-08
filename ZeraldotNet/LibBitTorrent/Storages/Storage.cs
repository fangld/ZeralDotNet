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
    public class Storage
    {
        #region Fields

        private FileStream _fileStream;

        #endregion

        #region Constructors

        public Storage(MetaInfo metaInfo, string saveAsDirectory)
        {
            if (metaInfo is SingleFileMetaInfo)
            {
                SingleFileMetaInfo singleFileMetaInfo = metaInfo as SingleFileMetaInfo;

                if (!Directory.Exists(saveAsDirectory))
                {
                    Directory.CreateDirectory(saveAsDirectory);
                }

                string path = string.Format(@"{0}\{1}", saveAsDirectory, singleFileMetaInfo.Name);
                _fileStream = File.Open(path, FileMode.OpenOrCreate);
            }
            else
            {
                
            }
        }

        #endregion

        #region Methods

        public void Write(byte[] buffer, long offset)
        {
            try
            {
                lock (_fileStream)
                {
                    _fileStream.Seek(offset, SeekOrigin.Begin);
                    _fileStream.Write(buffer, 0, buffer.Length);
                    _fileStream.Flush();
                }
            }
            catch (ObjectDisposedException)
            {
                //Nothing to be done.
            }
        }

        public int Read(byte[] buffer, long offset, int length)
        {
            int result = 0;
            try
            {
                lock (_fileStream)
                {
                    _fileStream.Seek(offset, SeekOrigin.Begin);
                    result = _fileStream.Read(buffer, 0, length);
                }

            }
            catch (ObjectDisposedException)
            {
                //Nothing to be done.
            }
            return result;
        }

        public void Close()
        {
            lock (_fileStream)
            {
                _fileStream.Close();
            }
        }

        #endregion
    }
}
