using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 文件结构,包含文件名称和长度
    /// </summary>
    public struct BitFile
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName;

        /// <summary>
        /// 文件长度
        /// </summary>
        public long Length;
    }
}
