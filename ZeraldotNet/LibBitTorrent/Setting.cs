using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public static class Setting
    {
        public static int BufferSize = 16384;

        public static int BufferPoolCapacity = 65536;//32768;

        public static int BlockSize = 16384;

        public static int MaxInterval = 600;

        public static int NumWant = 50;

        public static int Port = 6881;

        public static bool Compact = true;

        private static string _peerIdString = "-0a0900-000000000000";

        public static byte[] GetPeerId()
        {
            return Encoding.ASCII.GetBytes(_peerIdString);
        }

        public static string GetPeerIdString()
        {
            return _peerIdString;
        }
    }
}
