using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeraldotNet.LibBitTorrent.BEncoding;

namespace ZeraldotNet.LibBitTorrent.Storages
{
    public class SingleFileStorage : Storage
    {
        #region Fields

        private FileStream _fileStream;

        #endregion

        #region Constructors

        public SingleFileStorage(MetaInfo metaInfo, string saveAsDirectory)
        {
            SingleFileMetaInfo singleFileMetaInfo = metaInfo as SingleFileMetaInfo;

            if (!Directory.Exists(saveAsDirectory))
            {
                Directory.CreateDirectory(saveAsDirectory);
            }

            string path = string.Format(@"{0}\{1}", saveAsDirectory, singleFileMetaInfo.Name);
            _fileStream = File.Open(path, FileMode.OpenOrCreate);
            _fileStream.SetLength(singleFileMetaInfo.Length);
        }

        #endregion

        #region Methods

        public override void Write(byte[] buffer, long offset)
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

        public override int Read(byte[] buffer, long offset, int count)
        {
            int result = 0;
            try
            {
                lock (_fileStream)
                {
                    _fileStream.Seek(offset, SeekOrigin.Begin);
                    result = _fileStream.Read(buffer, 0, count);
                }
            }
            catch (ObjectDisposedException)
            {
                //Nothing to be done.
            }
            return result;
        }

        public override void Close()
        {
            lock (_fileStream)
            {
                _fileStream.Close();
            }
        }

        public override void Dispose()
        {
            lock (_fileStream)
            {
                _fileStream.Dispose();
            }
        }

        #endregion
    }
}
