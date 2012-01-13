using System.Text.RegularExpressions;

namespace ZeraldotNet.LibBitTorrent.BEncoding
{
    public class BTFormat
    {
        public static void CheckInfomation(DictNode info)
        {
            byte[] pieces = (info["pieces"] as BytesNode).ByteArray;

            if (pieces == null || pieces.Length % 20 != 0)
            {
                throw new BitTorrentException("Bad pieces key");
            }

            if (!(info["piece length"] is IntNode) || (info["piece length"] as IntNode).Value < 0)
            {
                throw new BitTorrentException("Illegal piece length");
            }

            string name = (info["name"] as BytesNode).StringText;
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
                if (!(info["length"] is IntNode) || (info["length"] as IntNode).Value < 0)
                {
                    throw new BitTorrentException("Bad length");
                }
            }

            else
            {
                ListNode filesHandler = info["files"] as ListNode;
                if (filesHandler == null)
                {
                    throw new BitTorrentException("files not list");
                }

                foreach (BEncodedNode fileHandler in filesHandler)
                {
                    DictNode ff = fileHandler as DictNode;
                    if (ff == null)
                    {
                        throw new BitTorrentException("Bad file value");
                    }

                    if (!(ff["length"] is IntNode) || (ff["length"] as IntNode).Value < 0)
                    {
                        throw new BitTorrentException("Bad length");
                    }

                    ListNode pathHandler = ff["path"] as ListNode;
                    if (pathHandler == null || pathHandler.Count == 0)
                    {
                        throw new BitTorrentException("Bad path");
                    }

                    foreach (BEncodedNode p in pathHandler)
                    {
                        BytesNode pp = p as BytesNode;
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
                        if (((filesHandler[i] as DictNode)["path"] as BytesNode).Equals((filesHandler[j] as DictNode)["path"] as BytesNode))
                        {
                            throw new BitTorrentException("duplicate path");
                        }
                    }
                }
            }
        }

        public static void CheckMessage(DictNode message)
        {
            if (!(message["announce"] is BytesNode))
            {
                throw new BitTorrentException("ValueError");
            }

            if (!(message["info"] is DictNode))
            {
                throw new BitTorrentException("ValueError");
            }

            CheckInfomation(message["info"] as DictNode);
        }

        public static void CheckPeers(DictNode message)
        {
            if (message.ContainsKey("failure reason"))
            {
                if (!(message["failure reason"] is BytesNode))
                {
                    throw new BitTorrentException("ValueError");
                }
                return;
            }

            ListNode peers = message["peers"] as ListNode;
            if (peers == null)
            {
                throw new BitTorrentException("ValueError");
            }

            foreach (BEncodedNode p in peers)
            {
                DictNode peer = p as DictNode;
                if (peer == null)
                {
                    throw new BitTorrentException("ValueError");
                }

                if (!(peer["ip"] is BytesNode))
                {
                    throw new BitTorrentException("ValueError");
                }

                try
                {
                    long port = (peer["port"] as IntNode).Value;
                    if (port <= 0 || port >= 65536)
                    {
                        throw new BitTorrentException("ValueError");
                    }
                }
                catch
                {
                    throw new BitTorrentException("ValueError");
                }

                byte[] id = ((BytesNode)peer["peer id"]).ByteArray;
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
                if (!(message["tracker id"] is BytesNode))
                {
                    throw new BitTorrentException("ValueError");
                }
            }
        }

        private static void CheckIntHandler(string key, DictNode message)
        {
            try
            {
                long value = ((IntNode)message[key]).Value;
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
