using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public static class TrackerParser
    {
        /// <summary>
        /// 解析函数
        /// </summary>
        /// <param name="node">Tracker服务器响应数据</param>
        /// <returns>返回Tracker服务器响应类</returns>
        public static AnnounceResponse Parse(BEncodedNode node)
        {
            AnnounceResponse result = new AnnounceResponse();
            
            DictNode responseNode = node as DictNode;
            Debug.Assert(responseNode != null);

            //Get failure reason
            if (responseNode.ContainsKey("failure reason"))
            {
                BytesNode failureReasonNode = responseNode["failure reason"] as BytesNode;
                Debug.Assert(failureReasonNode != null);
                result.FailureReason = failureReasonNode.StringText;
            }
            else
            {
                result.FailureReason = string.Empty;
            }

            //Get warning message
            if (responseNode.ContainsKey("warning message"))
            {
                BytesNode warningMessageNode = responseNode["warning message"] as BytesNode;
                Debug.Assert(warningMessageNode != null);
                result.WarningMessage = warningMessageNode.StringText;
            }
            else
            {
                result.WarningMessage = string.Empty;
            }

            //Get interval
            if (responseNode.ContainsKey("interval"))
            {
                IntNode intervalNode = responseNode["interval"] as IntNode;
                Debug.Assert(intervalNode != null);
                result.Interval = intervalNode.IntValue;
            }
            else
            {
                result.Interval = Setting.MaxInterval;
            }

            //Get min interval
            if (responseNode.ContainsKey("min interval"))
            {
                IntNode minIntervalNode = responseNode["min interval"] as IntNode;
                Debug.Assert(minIntervalNode != null);
                result.MinInterval = minIntervalNode.IntValue;
            }
            else
            {
                result.MinInterval = result.Interval;
            }

            //Get tracker id
            if (responseNode.ContainsKey("tracker id"))
            {
                BytesNode trackerIdNode = responseNode["tracker id"] as BytesNode;
                Debug.Assert(trackerIdNode != null);
                result.TrackerId = trackerIdNode.StringText;
            }
            else
            {
                result.TrackerId = string.Empty;
            }

            //Get complete
            if (responseNode.ContainsKey("complete"))
            {
                IntNode completeNode = responseNode["complete"] as IntNode;
                Debug.Assert(completeNode != null);
                result.Complete = completeNode.IntValue;
            }
            else
            {
                result.Complete = 0;
            }

            //Get incomplete
            if (responseNode.ContainsKey("incomplete"))
            {
                IntNode incompleteNode = responseNode["incomplete"] as IntNode;
                Debug.Assert(incompleteNode != null);
                result.Incomplete = incompleteNode.IntValue;
            }
            else
            {
                result.Incomplete = 0;
            }

            //Get peers
            result.Peers = GetPeers(responseNode);

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
                    int port = ((IntNode)dictNode["port"]).IntValue;
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
