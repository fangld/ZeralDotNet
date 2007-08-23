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
            byte[] pieces = (info["pieces"] as BytesHandler).ByteArray;

            if (pieces == null || pieces.Length % 20 != 0)
            {
                throw new BitTorrentException("Bad pieces key");
            }

            if (!(info["piece length"] is IntHandler) || (info["piece length"] as IntHandler).Value < 0)
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

            if (info.ContainsKey("files") && info.ContainsKey("length"))
            {
                throw new BitTorrentException("single/multiple file mix");
            }

            if (info.ContainsKey("length"))
            {
                if (!(info["length"] is IntHandler) || (info["length"] as IntHandler).Value < 0)
                {
                    throw new BitTorrentException("Bad length");
                }
            }

            else
            {
                ListHandler filesHandler = info["files"] as ListHandler;
                if (filesHandler == null)
                {
                    throw new BitTorrentException("files not list");
                }

                foreach (Handler fileHandler in filesHandler)
                {
                    DictionaryHandler ff = fileHandler as DictionaryHandler;
                    if (ff == null)
                    {
                        throw new BitTorrentException("Bad file value");
                    }

                    if (!(ff["length"] is IntHandler) || (ff["length"] as IntHandler).Value < 0)
                    {
                        throw new BitTorrentException("Bad length");
                    }

                    ListHandler pathHandler = ff["path"] as ListHandler;
                    if (pathHandler == null || pathHandler.Count == 0)
                    {
                        throw new BitTorrentException("Bad path");
                    }

                    foreach (Handler p in pathHandler)
                    {
                        BytesHandler pp = p as BytesHandler;
                        if (pp == null)
                        {
                            throw new BitTorrentException("Bad path dir");
                        }

                        if (!r.IsMatch(pp.StringText))
                        {
                            throw new BitTorrentException(string.Format("path {0} is disallowed for security reasons", pp.StringText));
                        }
                    }
                }

                int i, j;
                for (i = 0; i < filesHandler.Count; i++)
                {
                    for (j = 0; j < i; j++)
                    {
                        if (((filesHandler[i] as DictionaryHandler)["path"] as BytesHandler).Equals((filesHandler[j] as DictionaryHandler)["path"] as BytesHandler))
                        {
                            throw new BitTorrentException("duplicate path");
                        }
                    }
                }
            }
        }

        public static void CheckMessage(DictionaryHandler message)
        {
            if (!(message["announce"] is BytesHandler))
            {
                throw new BitTorrentException("ValueError");
            }

            if (!(message["info"] is DictionaryHandler))
            {
                throw new BitTorrentException("ValueError");
            }

            CheckInfomation(message["info"] as DictionaryHandler);
        }

        public static void CheckPeers(DictionaryHandler message)
        {
            if (message.ContainsKey("failure reason"))
            {
                if (!(message["failure reason"] is BytesHandler))
                {
                    throw new BitTorrentException("ValueError");
                }
                return;
            }

            ListHandler peers = message["peers"] as ListHandler;
            if (peers == null)
            {
                throw new BitTorrentException("ValueError");
            }

            foreach (Handler p in peers)
            {
                DictionaryHandler peer = p as DictionaryHandler;
                if (peer == null)
                {
                    throw new BitTorrentException("ValueError");
                }

                if (!(peer["ip"] is BytesHandler))
                {
                    throw new BitTorrentException("ValueError");
                }

                try
                {
                    long port = (peer["port"] as IntHandler).Value;
                    if (port <= 0 || port >= 65536)
                    {
                        throw new BitTorrentException("ValueError");
                    }
                }
                catch
                {
                    throw new BitTorrentException("ValueError");
                }

                byte[] id = ((BytesHandler)peer["peer id"]).ByteArray;
                if (id == null || id.Length != 20)
                {
                    throw new BitTorrentException("ValueError");
                }
            }

            if (message.ContainsKey("interval"))
            {
                CheckIntHandler("interval", message);
            }

            if (message.ContainsKey("'min interval'"))
            {
                CheckIntHandler("'min interval'",message);
            }

            if (message.ContainsKey("num peers"))
            {
                CheckIntHandler("num peers", message);
            }

            if (message.ContainsKey("done peers"))
            {
                CheckIntHandler("done peers", message);
            }

            if (message.ContainsKey("last"))
            {
                CheckIntHandler("last", message);
            }

            if (message.ContainsKey("tracker id"))
            {
                if (!(message["tracker id"] is BytesHandler))
                {
                    throw new BitTorrentException("ValueError");
                }
            }
        }

        private static void CheckIntHandler(string key, DictionaryHandler message)
        {
            try
            {
                long value = ((IntHandler)message[key]).Value;
                if (value < 0)
                {
                    throw new BitTorrentException(string.Format("{0} error", key));
                }
            }

            catch
            {
                throw new BitTorrentException(string.Format("{0} value is above 0", key));
            }
        }
    }
}
