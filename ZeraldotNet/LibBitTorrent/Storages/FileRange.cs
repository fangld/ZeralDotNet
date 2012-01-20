namespace ZeraldotNet.LibBitTorrent.Storages
{
    /// <summary>
    /// 文件结构,包含文件名称,起始位置,结束位置
    /// </summary>
    public class FileRange
    {
        #region Properties

        /// <summary>
        /// 访问和设置文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 访问和设置文件的起始位置
        /// </summary>
        public long Begin { get; set; }

        /// <summary>
        /// 访问和设置文件的结束位置
        /// </summary>
        public long End { get; set; }

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
            FileName = fileName;
            Begin = begin;
            End = end;
        }

        #endregion
    }
}
