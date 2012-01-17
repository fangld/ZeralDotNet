using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Trackers
{
    /// <summary>
    /// The response of tracker server
    /// </summary>
    public class AnnounceResponse
    {
        #region Properties

        /// <summary>
        /// The failure reason that connects tracker server
        /// </summary>
        public string FailureReason { get; set; }

        /// <summary>
        /// The warning message that connects tracker server
        /// </summary>
        public string WarningMessage { get; set; }

        /// <summary>
        /// Interval in seconds that the client should wait between sending regular requests to the tracker.
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// The minimum announce interval.
        /// </summary>
        public int MinInterval { get; set; }

        /// <summary>
        /// A string that the client should send back on its next announcements.
        /// </summary>
        public string TrackerId { get; set; }

        /// <summary>
        /// The number of peers with the entire file
        /// </summary>
        public int Complete { get; set; }

        /// <summary>
        /// The number of non-seeder peers
        /// </summary>
        public int Incomplete { get; set; }

        /// <summary>
        /// The list of peer
        /// </summary>
        public List<Peer> Peers { get; set; }

        #endregion
    }
}
