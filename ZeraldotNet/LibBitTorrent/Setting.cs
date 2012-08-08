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

        private static byte[] _peerIdBytes;

        #endregion

        #region Properties

        public static int TrackerBufferLength = 16384;

        public static int BufferPoolCapacity = 65536;

        public static int BlockLength = 16384;

        public static int MaxInterval = 600;

        public static int NumWant = 50;

        public static bool Compact = true;

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

        private const string _peerIdString = "-0a0900-000000000000";

        #endregion

        #region Constructors

        static Setting()
        {
            _peerIdBytes = Encoding.ASCII.GetBytes(_peerIdString);
        }

        #endregion

        #region Methods

        public static byte[] GetPeerId()
        {
            return Encoding.ASCII.GetBytes(_peerIdString);
        }

        public static string GetPeerIdString()
        {
            return _peerIdString;
        }

        #endregion
    }
}
