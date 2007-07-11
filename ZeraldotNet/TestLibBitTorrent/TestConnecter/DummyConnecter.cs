using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Encrypters;
using ZeraldotNet.LibBitTorrent.Downloads;
using ZeraldotNet.LibBitTorrent.Chokers;
using ZeraldotNet.LibBitTorrent.Uploads;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent;

namespace ZeraldotNet.TestLibBitTorrent.TestConnecter
{
    public class DummyConnecter : Connecter
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="downloader">下载器</param>
        /// <param name="choker">阻塞器</param>
        /// <param name="piecesNumber">下载文件的片断数量</param>
        /// <param name="isEverythingPending"></param>
        /// <param name="totalUp">参数类</param>
        /// <param name="maxUploadRate">最大上传速率</param>
        /// <param name="scheduleFunction"></param>
        public DummyConnecter(IDownloader downloader, IChoker choker, int piecesNumber, PendingDelegate isEverythingPending,
            Measure totalUp, int maxUploadRate, SchedulerDelegate scheduleFunction)
            : base(downloader, choker, piecesNumber, isEverythingPending, totalUp, maxUploadRate, scheduleFunction)
        { }

        protected override IUpload MakeUpload(IConnection connection)
        {
            return TestConnecter.MakeUpload(connection);
        }
    }
}
