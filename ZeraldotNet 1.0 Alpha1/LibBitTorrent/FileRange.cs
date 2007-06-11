using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 文件结构,包含文件名称,起始位置,结束位置
    /// </summary>
    public struct FileRange
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName;

        /// <summary>
        /// 文件的起始位置
        /// </summary>
        public long Begin;

        /// <summary>
        /// 文件的结束位置
        /// </summary>
        public long End;
    }
}
