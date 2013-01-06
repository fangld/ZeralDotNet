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
        #region Fields

        /// <summary>
        /// 
        /// </summary>
        private FileInfo[] _fileInfoArray;

        #endregion

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
            get { return _fileInfoArray.Sum(fi => fi.Length); }
        }

        /// <summary>
        /// the mode of metainfo
        /// </summary>
        public override MetaInfoMode Mode
        {
            get { return MetaInfoMode.MultiFile; }
        }

        #endregion

        #region Constructors

        public MultiFileMetaInfo()
        {
            Name = string.Empty;
        }

        #endregion

        #region Methods

        public void SetFileInfoArray(FileInfo[] fileInfoArray)
        {
            _fileInfoArray = fileInfoArray;
        }

        public FileInfo[] GetFileInfoArray()
        {
            return _fileInfoArray;
        }

        #endregion
    }
}
