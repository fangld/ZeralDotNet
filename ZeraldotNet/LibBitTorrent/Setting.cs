using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Setting class
    /// </summary>
    public static class Setting
    {
        #region Fields

        /// <summary>
        /// The string of peer id
        /// </summary>
        private const string _peerIdString = "-0A0900-000000000000";

        /// <summary>
        /// The byte array of peer id
        /// </summary>
        private static byte[] _peerIdBytes;

        #endregion

        #region Properties

        /// <summary>
        /// The length of buffer of tracker
        /// </summary>
        public static int TrackerBufferLength = 16384;

        /// <summary>
        /// The capacity of buffer pool
        /// </summary>
        public static int BufferPoolCapacity = 65536;

        /// <summary>
        /// The length of block
        /// </summary>
        public static int BlockLength = 16384;

        /// <summary>
        /// The max interval of tracker
        /// </summary>
        public static int MaxInterval = 600;

        /// <summary>
        /// The number of wanted peers
        /// </summary>
        public static int NumWant = 50;

        /// <summary>
        /// The flag of support compact tracker
        /// </summary>
        public static bool Compact = true;

        /// <summary>
        /// The backlog of listener
        /// </summary>
        public static int ListenBacklog = 10;

        /// <summary>
        /// The number of allowed fast set
        /// </summary>
        public static int AllowedFastSetNumber = 10;

        /// <summary>
        /// The listenning port for accept new peer
        /// </summary>
        public static ushort PeerListenningPort = 6881;

        /// <summary>
        /// The listenning port for Dht tracker
        /// </summary>
        public static ushort DhtListenningPort = 6882;       

        /// <summary>
        /// The next connected interval after the tracker is fail.(ms)
        /// </summary>
        public static double TrackerFailInterval = 6000000D;

        /// <summary>
        /// The peer alive time
        /// </summary>
        public static double PeerAliveInterval = 1200000D;

        /// <summary>
        /// The flag that represents whether peer support extension protocol
        /// </summary>
        public static bool AllowExtension = false;

        /// <summary>
        /// The flag that represents whether peer support DHT.
        /// </summary>
        public static bool AllowDht = false;

        /// <summary>
        /// The flag that represents whether peer support peer exchange.
        /// </summary>
        public static bool AllowPeerExchange = false;

        /// <summary>
        /// The flag that represents whether peer support fast extension.
        /// </summary>
        public static bool AllowFastPeer = true;

        /// <summary>
        /// The flag that represents whether allow two peers have the same ip.
        /// </summary>
        public static bool AllowSameIp = true;

        #endregion

        #region Constructors

        /// <summary>
        /// Static constructor
        /// </summary>
        static Setting()
        {
            _peerIdBytes = Encoding.ASCII.GetBytes(_peerIdString);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the byte array of peer id
        /// </summary>
        /// <returns>the byte array of peer id</returns>
        public static byte[] GetPeerId()
        {
            return _peerIdBytes;
        }

        /// <summary>
        /// Get the string of peer id
        /// </summary>
        /// <returns>the string of peer id</returns>
        public static string GetPeerIdString()
        {
            return _peerIdString;
        }

        #endregion
    }
}
