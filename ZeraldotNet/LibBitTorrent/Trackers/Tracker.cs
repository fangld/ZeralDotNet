using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZeraldotNet.LibBitTorrent.BEncoding;

namespace ZeraldotNet.LibBitTorrent.Trackers
{
    /// <summary>
    /// 事件模式
    /// </summary>
    public enum EventMode
    {
        Started,
        Stopped,
        Completed
    }

    /// <summary>
    /// Tracker服务器
    /// </summary>
    public class Tracker
    {
        #region Properties

        /// <summary>
        /// The url of tracker server
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The tracker server id
        /// </summary>
        public string TrackerId { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Announce the tracker server
        /// </summary>
        /// <param name="request">The request of announce information</param>
        /// <returns>Return the response of announce information</returns>
        public async Task<AnnounceResponse> Announce(AnnounceRequest request)
        {
            string infoHashUrlEncodedFormat = request.InfoHash.ToHexString().ToUrlEncodedFormat();
            int compact = request.Compact ? 1 : 0;

            string uri =
                string.Format(
                    "{0}?info_hash={1}&peer_id={2}&port={3}&uploaded={4}&downloaded={5}&left={6}&compact={7}&event={8}",
                    Url, infoHashUrlEncodedFormat, request.PeerId, request.Port, request.Uploaded, request.Downloaded,
                    request.Left, compact, request.Event.ToString().ToLower());

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(uri);
            Stream stream = httpRequest.GetResponse().GetResponseStream();
            Debug.Assert(stream != null);

            int count = Setting.BufferSize;
            int readLength;
            byte[] rcvBuf = new byte[Setting.BufferSize];
            MemoryStream ms = new MemoryStream(Setting.BufferSize);
            do
            {
                readLength = await stream.ReadAsync(rcvBuf, 0, count);
                ms.Write(rcvBuf,0, readLength);
            } while (readLength != 0);
            BEncodedNode node = BEncoder.Decode(ms.ToArray());
            DictNode responseNode = node as DictNode;
            Debug.Assert(responseNode != null);
            AnnounceResponse result = Parse(responseNode);
            return result;
        }

        /// <summary>
        /// Scrape the tracker server
        /// </summary>
        /// <param name="request">The request of announce information</param>
        /// <returns>Return the response of announce information</returns>
        public void Scrape(ScrapeRequest request)
        {
            
        }

        /// <summary>
        /// 解析函数
        /// </summary>
        /// <param name="node">Tracker服务器响应数据</param>
        /// <returns>返回Tracker服务器响应类</returns>
        private AnnounceResponse Parse(DictNode node)
        {
            AnnounceResponse result = new AnnounceResponse();

            Debug.Assert(node != null);

            //Get failure reason
            if (node.ContainsKey("failure reason"))
            {
                BytesNode failureReasonNode = node["failure reason"] as BytesNode;
                Debug.Assert(failureReasonNode != null);
                result.FailureReason = failureReasonNode.StringText;
            }
            else
            {
                result.FailureReason = string.Empty;
            }

            //Get warning message
            if (node.ContainsKey("warning message"))
            {
                BytesNode warningMessageNode = node["warning message"] as BytesNode;
                Debug.Assert(warningMessageNode != null);
                result.WarningMessage = warningMessageNode.StringText;
            }
            else
            {
                result.WarningMessage = string.Empty;
            }

            //Get interval
            if (node.ContainsKey("interval"))
            {
                IntNode intervalNode = node["interval"] as IntNode;
                Debug.Assert(intervalNode != null);
                result.Interval = intervalNode.IntValue;
            }
            else
            {
                result.Interval = Setting.MaxInterval;
            }

            //Get min interval
            if (node.ContainsKey("min interval"))
            {
                IntNode minIntervalNode = node["min interval"] as IntNode;
                Debug.Assert(minIntervalNode != null);
                result.MinInterval = minIntervalNode.IntValue;
            }
            else
            {
                result.MinInterval = result.Interval;
            }

            //Get tracker id
            if (node.ContainsKey("tracker id"))
            {
                BytesNode trackerIdNode = node["tracker id"] as BytesNode;
                Debug.Assert(trackerIdNode != null);
                result.TrackerId = trackerIdNode.StringText;
            }
            else
            {
                result.TrackerId = string.Empty;
            }

            //Get complete
            if (node.ContainsKey("complete"))
            {
                IntNode completeNode = node["complete"] as IntNode;
                Debug.Assert(completeNode != null);
                result.Complete = completeNode.IntValue;
            }
            else
            {
                result.Complete = 0;
            }

            //Get incomplete
            if (node.ContainsKey("incomplete"))
            {
                IntNode incompleteNode = node["incomplete"] as IntNode;
                Debug.Assert(incompleteNode != null);
                result.Incomplete = incompleteNode.IntValue;
            }
            else
            {
                result.Incomplete = 0;
            }

            //Get peers
            result.Peers = GetPeers(node);

            return result;
        }

        /// <summary>
        /// 获取节点信息
        /// </summary>
        /// <param name="node">Tracker服务器响应数据</param>
        /// <returns>返回信息节点</returns>
        private List<Peer> GetPeers(DictNode node)
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

        #endregion
    }
}
