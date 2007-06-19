using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class SingleDownload : ISingleDownload
    {
        private Downloader downloader;
        private Connection connection;
        private bool choked;
        private bool interested;
        private List<ActiveRequest> requests;
        private Measure measure;
        private bool[] have;
        private DateTime last;

        public SingleDownload(Downloader downloader, Connection connection)
        {
            this.downloader = downloader;
            this.connection = connection;
            this.choked = true;
            this.interested = false;
            this.requests = new List<ActiveRequest>();
            this.measure = new Measure(downloader.m
        }
    }
}
