using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    /// <summary>
    /// Meta info that is single file mode
    /// </summary>
    public class SingleFileMetaInfo : MetaInfo
    {
        /// <summary>
        /// the filename. This is purely advisory.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// length of the file in bytes 
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// the length of all files
        /// </summary>
        public override long SumLength { get { return Length; } }

        /// <summary>
        /// a 32-character hexadecimal string corresponding to the MD5 sum of the file.
        /// </summary>
        public string Md5Sum { get; set; }

        /// <summary>
        /// the mode of metainfo
        /// </summary>
        public override MetaInfoMode Mode
        {
            get { return MetaInfoMode.SingleFile; }
        }
    }
}
