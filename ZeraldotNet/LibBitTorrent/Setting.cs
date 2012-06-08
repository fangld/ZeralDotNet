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

        public static int BufferSize = 16384;

        public static int BufferPoolCapacity = 65536;

        public static int BlockSize = 16384;

        public static int MaxInterval = 600;

        public static int NumWant = 50;

        public static int Port = 6881;

        public static bool Compact = true;

        /// <summary>
        /// The next connected interval after the tracker is fail.(ms)
        /// </summary>
        public static double TrackerFailInterval = 6000000D;

        /// <summary>
        /// The peer alive time
        /// </summary>
        public static double PeerAliveInterval = 120000D;

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
