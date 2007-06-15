using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Poll的状态
    /// </summary>
    public enum PollMode
    {
        /// <summary>
        /// 可读
        /// </summary>
        PollIn = 1,

        /// <summary>
        /// 可写
        /// </summary>
        PollOut = 2,

        /// <summary>
        /// 读写
        /// </summary>
        PollInOut = 3,

        /// <summary>
        /// 错误
        /// </summary>
        PollError = 8,

        /// <summary>
        /// 挂起
        /// </summary>
        PollHangUp = 16
    }
}
