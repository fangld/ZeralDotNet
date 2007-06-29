using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Storages
{
    /// <summary>
    /// 文件结构,包含文件名称和长度
    /// </summary>
    public class BitFile
    {
        #region Private Field

        /// <summary>
        /// 文件名称
        /// </summary>
        private string fileName;

        /// <summary>
        /// 文件长度
        /// </summary>
        private long length;

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
        /// 访问和设置文件长度
        /// </summary>
        public long Length
        {
            get { return length; }
            set { length = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <param name="lengthBytes">文件长度</param>
        public BitFile(string fileName, long length)
        {
            this.fileName = fileName;
            this.length = length;
        }

        #endregion
    }
}
