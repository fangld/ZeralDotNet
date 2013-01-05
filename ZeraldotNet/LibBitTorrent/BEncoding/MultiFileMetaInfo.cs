using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    /// <summary>
    /// Meta info that is multi file mode
    /// </summary>
    public class MultiFileMetaInfo : MetaInfo
    {
        #region Properties

        /// <summary>
        /// the file path of the directory in which to store all the files. This is purely advisory. 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// the length of all files
        /// </summary>
        public override long SumLength
        {
            get { return _fileInfoList.Sum(fi => fi.Length); }
        }

        /// <summary>
        /// the mode of metainfo
        /// </summary>
        public override MetaInfoMode Mode
        {
            get { return MetaInfoMode.MultiFile; }
        }

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
