using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeraldotNet.LibBitTorrent.BEncoding;

namespace ZeraldotNet.LibBitTorrent.Storages
{
    using Task = System.Threading.Tasks.Task;

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

        public void  Write(byte[] buffer, long offset)
        {
            _fileStream.Seek(offset, SeekOrigin.Begin);
            _fileStream.Write(buffer, 0, buffer.Length);
        }

        public int Read(byte[] buffer, long offset, int length)
        {
            _fileStream.Seek(offset, SeekOrigin.Begin);
            int result = _fileStream.Read(buffer, 0, length);
            return result;
        }

        public void FlushAsync()
        {
            _fileStream.FlushAsync();
        }

        public void Close()
        {
            _fileStream.Close();
        }

        #endregion
    }
}
