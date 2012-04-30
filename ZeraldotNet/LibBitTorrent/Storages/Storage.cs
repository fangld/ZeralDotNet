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

        private List<FileStream> _fileStreamList;

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
                FileStream fs = File.Open(path, FileMode.OpenOrCreate);

                _fileStreamList = new List<FileStream>();
                _fileStreamList.Add(fs);
            }
            else
            {
                
            }
            
        }

        #endregion

        #region Methods

        public Task WriteAsync(byte[] buffer, int offset, int count)
        {
            return _fileStreamList[0].WriteAsync(buffer, 0, count);
        }

        public Task<int> Read(byte[] buffer, int offset, int count)
        {
            return _fileStreamList[0].ReadAsync(buffer, 0, count);
        }

        public void FlushAsync()
        {
            for(int i = 0; i < _fileStreamList.Count; i++)
            {
                _fileStreamList[i].FlushAsync();
            }
        }

        public void Close()
        {
            for (int i = 0; i < _fileStreamList.Count; i++)
            {
                _fileStreamList[i].Close();
            }
        }

        #endregion
    }
}
