using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Trackers
{
    /// <summary>
    /// The request of tracker server
    /// </summary>
    public class AnnounceRequest
    {
        #region Properties

        /// <summary>
        /// The info hash of this torrent
        /// </summary>
        public byte[] InfoHash { get; set; }

        /// <summary>
        /// The peer id
        /// </summary>
        public string PeerId { get; set; }

        /// <summary>
        /// The port number this peer is listening on.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// The total amount uploaded.
        /// </summary>
        public long Uploaded { get; set; }

        /// <summary>
        /// The total amount downloaded.
        /// </summary>
        public long Downloaded { get; set; }

        /// <summary>
        /// The number of bytes this peer still has to download
        /// </summary>
        public long Left { get; set; }

        /// <summary>
        /// The compact mode of tracker request
        /// </summary>
        public bool Compact { get; set; }

        /// <summary>
        /// Indicates that the tracker can omit peer id field in announce-list dictionary.
        /// </summary>
        public int NoPeerId { get; set; }

        /// <summary>
        /// The event mode of trancker request.
        /// </summary>
        public EventMode Event { get; set; }

        /// <summary>
        /// The true IP address of the client.
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// The number of peers that the client would like to receive from the tracker.
        /// </summary>
        public int NumWant { get; set; }

        /// <summary>
        /// An additional client identification mechanism that is not shared with any peers. It is intended to allow a client to prove their identity should their IP address change. 
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The tracker id.
        /// </summary>
        public string TrackerId { get; set; }

        #endregion
    }
}
