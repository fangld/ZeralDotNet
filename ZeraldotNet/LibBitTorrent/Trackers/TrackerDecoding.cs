using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ZeraldotNet.LibBitTorrent.BEncoding;

namespace ZeraldotNet.LibBitTorrent.Trackers
{
    /// <summary>
    /// Tracker服务器响应数据解析
    /// </summary>
    public static class TrackerDecoding
    {
        /// <summary>
        /// 解析函数
        /// </summary>
        /// <param name="node">Tracker服务器响应数据</param>
        /// <returns>返回Tracker服务器响应类</returns>
        public static AnnounceResponse Decode(BEncodedNode node)
        {
            AnnounceResponse result = new AnnounceResponse();
            
            DictNode dictNode = node as DictNode;

            result.FailureReason = ((BytesNode)dictNode["failure reason"]).StringText;
            result.WarningMessage = ((BytesNode)dictNode["warning message"]).StringText;
            result.Interval = (int)((IntNode) dictNode["interval"]).Value;
            result.MinInterval = (int)((IntNode)dictNode["min interval"]).Value;
            result.TrackerId = ((BytesNode)dictNode["tracker id"]).StringText;
            result.Peers = GetPeers(dictNode);
            result.Interval = (int)((IntNode) dictNode["interval"]).Value;

            return result;
        }

        /// <summary>
        /// 获取节点信息
        /// </summary>
        /// <param name="node">Tracker服务器响应数据</param>
        /// <returns>返回信息节点</returns>
        private static List<Peer> GetPeers(DictNode node)
        {
            List<Peer> peers = new List<Peer>();

            BEncodedNode peersNode = node["peers"];
            if (peersNode is BytesNode)
            {
                var peersBytes = ((BytesNode)peersNode).ByteArray;
                for (int i = 0; i < peersBytes.Length; i += 6)
                {
                    string host = string.Format("{0}.{1}.{2}.{3}", peersBytes[i], peersBytes[i + 1], peersBytes[i + 2],
                                                peersBytes[i + 3]);
                    int port = (peersBytes[i + 4] << 8) + peersBytes[i + 5];
                    Peer peer = new Peer
                                    {
                                        Host = host,
                                        Port = port
                                    };
                    peers.Add(peer);
                    Console.WriteLine("{0}:{1}", host, port);
                }
            }

            else if (peersNode is ListNode)
            {
                var listNode = peersNode as ListNode;
                for (int i = 0; i < listNode.Count; i++)
                {
                    DictNode dictNode = listNode[i] as DictNode;
                    string peerId = ((BytesNode)dictNode["peer id"]).StringText;
                    string ip = ((BytesNode)dictNode["ip"]).StringText;
                    int port = (int)((IntNode)dictNode["port"]).Value;
                    Peer peer = new Peer
                    {
                        Host = ip,
                        Port = port
                    };
                    peers.Add(peer);
                    Console.WriteLine("{0}-{1}:{2}", peerId, ip, port);
                }
            }

            return peers;
        }
    }
}
