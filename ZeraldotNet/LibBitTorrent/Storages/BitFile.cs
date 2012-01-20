namespace ZeraldotNet.LibBitTorrent.Storages
{
    /// <summary>
    /// 文件结构,包含文件名称和长度
    /// </summary>
    public class BitFile
    {
        #region Properties

        /// <summary>
        /// 访问和设置文件名称
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// 访问和设置文件长度
        /// </summary>
        public long Length { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileName">The file name of file</param>
        /// <param name="length">The length of file</param>
        public BitFile(string fileName, long length)
        {
            FileName = fileName;
            Length = length;
        }

        #endregion
    }
}
