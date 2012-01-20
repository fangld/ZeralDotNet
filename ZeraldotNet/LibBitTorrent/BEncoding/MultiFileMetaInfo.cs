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

        public override MetaInfoMode Mode
        {
            get { return MetaInfoMode.MultiFile; }
        }

        #endregion

        #region Fields

        private IList<FileInfo> _fileInfoList;

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

        public IList<FileInfo> GetFileInfoList()
        {
            return _fileInfoList;
        }

        #endregion
    }
}
