using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Downloads;

namespace ZeraldotNet.TestLibBitTorrent.TestConnecter
{
    /// <summary>
    /// 测试下载器类
    /// </summary>
    public class DummyDownloader : Downloader
    {
        /// <summary>
        /// 记录发生的事件
        /// </summary>
        List<string> events;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="events">记录发生的事件</param>
        public DummyDownloader(List<string> events)
        {
            this.events = events;
        }

        #region IDownloader Members

        /// <summary>
        /// 建立下载器
        /// </summary>
        /// <param name="connection">待建立的连接类</param>
        /// <returns></returns>
        public override SingleDownload MakeDownload(IConnection connection)
        {
            return new DummyDownload(events);
        }

        #endregion
    }
}
