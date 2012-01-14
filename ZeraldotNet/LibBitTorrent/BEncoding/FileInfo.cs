using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    public class FileInfo
    {
        #region Properties

        /// <summary>
        /// length of the file in bytes.
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// a 32-character hexadecimal string corresponding to the MD5 sum of the file.
        /// </summary>
        public string Md5Sum { get; set; }

        /// <summary>
        /// a list containing one or more string elements that together represent the path and filename.
        /// </summary>
        public string Path { get; set; }

        #endregion
    }
}
