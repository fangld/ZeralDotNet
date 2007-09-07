using System;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// BitTorrent专用异常类
    /// </summary>
    public class BitTorrentException : Exception
    {
        /// <summary>
        /// 初始化一个的BitTorrentException类
        /// </summary>
        public BitTorrentException()
        {
        }

        /// <summary>
        /// 初始化一个的BitTorrentException类,并且有一个特定的错误信息
        /// </summary>
        /// <param name="message">错误信息.</param>
        public BitTorrentException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// 初始化一个的BitTorrentException类,并且有一个特定的错误信息与一个内部的异常  
        /// </summary>
        /// <param name="message">错误信息</param>
        /// <param name="innerException">内部异常</param>
        public BitTorrentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// 初始化一个的BitTorrentException类,并且有一个串行化数据 
        /// </summary>
        /// <param name="info">串行化数据保存了异常的信息</param>
        /// <param name="context">上下文包括了源与目标的信息</param>
        /// <exception cref="System.ArgumentNullException">参数info为空</exception>
        /// <exception cref="System.Runtime.Serialization.SerializationException">这个类为空,或者System.Exception.HResult为零</exception>
        public BitTorrentException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
