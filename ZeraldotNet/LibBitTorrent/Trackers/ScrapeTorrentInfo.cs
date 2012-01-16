using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Trackers
{
    public class ScrapeTorrentInfo
    {
        /// <summary>
        /// The number of peers with the entire file
        /// </summary>
        public int Complete { get; set; }

        /// <summary>
        /// The total number of times the tracker has registered a completion 
        /// </summary>
        public int Downloaded { get; set; }

        /// <summary>
        /// The number of non-seeder peers
        /// </summary>
        public int Incomplete { get; set; }

        /// <summary>
        /// The torrent's internal name
        /// </summary>
        public string Name;
    }
}
