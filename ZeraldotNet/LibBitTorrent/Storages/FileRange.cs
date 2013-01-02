namespace ZeraldotNet.LibBitTorrent.Storages
{
    /// <summary>
    /// The range in all files of file
    /// </summary>
    public class FileRange
    {
        #region Properties

        /// <summary>
        /// a list containing one or more string elements that together represent the path and filename.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// begin offset of the file in all files
        /// </summary>
        public long Begin { get; set; }

        /// <summary>
        /// end offset of the file in all files
        /// </summary>
        public long End { get; set; }

        #endregion
    }
}
