using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.TestLibBitTorrent.TestConnecter
{
    /// <summary>
    /// 测试下载器类
    /// </summary>
    public class DummyDownloader
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

        /// <summary>
        /// 建立下载器
        /// </summary>
        /// <param name="connection">待建立的</param>
        /// <returns></returns>
        public DummyDownload MakeDownload(DummyConnection connection)
        {
            return new DummyDownload(events);
        }
    }
}
