using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Storages
{
    /// <summary>
    /// 文件结构,包含文件名称,起始位置,结束位置
    /// </summary>
    public class FileRange
    {
        #region Private Field

        /// <summary>
        /// 文件名称
        /// </summary>
        private string fileName;

        /// <summary>
        /// 文件的起始位置
        /// </summary>
        private long begin;

        /// <summary>
        /// 文件的结束位置
        /// </summary>
        private long end;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问和设置文件名称
        /// </summary>
        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        /// <summary>
        /// 访问和设置文件的起始位置
        /// </summary>
        public long Begin
        {
            get { return begin; }
            set { begin = value; }
        }

        /// <summary>
        /// 访问和设置文件的结束位置
        /// </summary>
        public long End
        {
            get { return end; }
            set { end = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="begin">文件的起始位置</param>
        /// <param name="end">文件的结束位置</param>
        public FileRange(string fileName, long begin, long end)
        {
            this.fileName = fileName;
            this.begin = begin;
            this.end = end;
        }

        #endregion
    }
}
