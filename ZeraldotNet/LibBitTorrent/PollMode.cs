using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Poll的状态
    /// </summary>
    [Flags]
    public enum PollMode
    {
        /// <summary>
        /// 可读
        /// </summary>
        PollIn = 0x0001,

        /// <summary>
        /// 可写
        /// </summary>
        PollOut = 0x0002,

        /// <summary>
        /// 读写
        /// </summary>
        PollInOut = 0x0003,

        /// <summary>
        /// 错误
        /// </summary>
        PollError = 0x0008,

        /// <summary>
        /// 挂起
        /// </summary>
        PollHangUp = 0x0010
    }
}
