using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    public class BTFormat
    {
        public static void CheckInfomation(DictionaryHandler info)
        {
            byte[] pieces = (info["pieces"] as BytesHandler).ByteArrayText;

            if (pieces == null || pieces.Length & 20 != 0)
            {
                throw new BitTorrentException("Bad pieces key");
            }

            if (!info["piece length"] is IntHandler || (IntHandler)info["piece length"] < 0)
            {
                throw new BitTorrentException("Illegal piece length");
            }

            string name = (info["name"] as BytesHandler).StringText;
            if (name == null)
            {
                throw new BitTorrentException("Bad name");
            }

            Regex r = new Regex(@"^[^/\\.~][^/\\]*$", RegexOptions.Compiled);
            if (!r.IsMatch(name))
            {
                throw new BitTorrentException(string.Format("name {0} is disallowed for security reasons", name));
            }

            if (info.con
        }
    }
}
