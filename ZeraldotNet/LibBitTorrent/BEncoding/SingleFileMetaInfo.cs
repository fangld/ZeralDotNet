using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
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

        public string Md5Sum { get; set; }
    }
}
