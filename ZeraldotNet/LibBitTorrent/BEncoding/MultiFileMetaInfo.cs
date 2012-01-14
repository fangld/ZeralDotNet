using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    public class MultiFileMetaInfo : MetaInfo
    {
        #region Properties

        /// <summary>
        /// the file path of the directory in which to store all the files. This is purely advisory. 
        /// </summary>
        public string Name { get; set; }

        #endregion

        #region Fields

        private List<FileInfo> _fileInfoList;

        #endregion

        #region Constructors

        public MultiFileMetaInfo()
        {
            _fileInfoList = new List<FileInfo>();
            Name = string.Empty;
        }

        #endregion

        #region Methods

        public void AddFileInfo(FileInfo fileInfo)
        {
            _fileInfoList.Add(fileInfo);
        }

        public List<FileInfo> GetFileInfoList()
        {
            return _fileInfoList;
        }

        #endregion
    }
}
