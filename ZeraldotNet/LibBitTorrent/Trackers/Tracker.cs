﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ZeraldotNet.LibBitTorrent.BEncoding;

namespace ZeraldotNet.LibBitTorrent.Trackers
{
    /// <summary>
    /// Event mode 
    /// </summary>
    public enum EventMode
    {
        Started,
        Stopped,
        Completed
    }

    /// <summary>
    /// Tracker
    /// </summary>
    public class Tracker : IEquatable<Tracker>
    {
        #region Fields

        private Timer _timer;

        private AnnounceRequest _request;

        #endregion

        #region Properties

        /// <summary>
        /// The url of tracker server
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The requested uri
        /// </summary>
        public string Uri { get; set; }

        #endregion

        #region Events

        public event EventHandler<AnnounceResponse> GotAnnounceResponse;

        public event EventHandler<WebException> ConnectFail;

        public event EventHandler<BitTorrentException> ReturnMessageFail;

        #endregion

        #region Constructors

        public Tracker(string url, AnnounceRequest request)
        {
            Url = url;
            _request = request;
            _timer = new Timer();
            _timer.AutoReset = false;
            _timer.Elapsed += (sender, e) => Announce();
            string infoHashUrlEncodedFormat = _request.InfoHash.ToHexString().ToUrlEncodedFormat();
            int compact = _request.Compact ? 1 : 0;
            Uri = string.Format(
                    "{0}?info_hash={1}&peer_id={2}&port={3}&uploaded={4}&downloaded={5}&left={6}&compact={7}&event={8}",
                    Url, infoHashUrlEncodedFormat, _request.PeerId, _request.Port, _request.Uploaded, _request.Downloaded,
                    _request.Left, compact, _request.Event.ToString().ToLower());
        }

        #endregion

        #region Methods

        /// <summary>
        /// Announce the tracker server
        /// </summary>
        /// <returns>Return the response of announce information</returns>
        public async void Announce()
        {
            try
            {
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(Uri);
                try
                {
                    BEncodedNode node;
                    using (WebResponse webResponse = await httpRequest.GetResponseAsync())
                    {
                        Stream stream = webResponse.GetResponseStream();
                        Debug.Assert(stream != null);

                        int count = Setting.TrackerBufferLength;

                        byte[] rcvBuf = new byte[Setting.TrackerBufferLength];
                        using (MemoryStream ms = new MemoryStream(Setting.TrackerBufferLength))
                        {
                            int readLength;
                            do
                            {
                                readLength = await stream.ReadAsync(rcvBuf, 0, count);
                                ms.Write(rcvBuf, 0, readLength);
                            } while (readLength != 0);
                            node = BEncodingFactory.Decode(ms.ToArray());
                        }
                    }

                    DictNode responseNode = node as DictNode;
                    if (responseNode != null)
                    {
                        AnnounceResponse response = Parse(responseNode);
                        if (response != null)
                        {
                            _timer.Interval = response.Interval * 1000;                         
                            GotAnnounceResponse(this, response);
                        }
                        else
                        {
                            _timer.Interval = Setting.TrackerFailInterval;
                            BitTorrentException exception = new BitTorrentException("Tracker returns fail message.");
                            ReturnMessageFail(this, exception);
                        }
                    }
                    else
                    {
                        _timer.Interval = Setting.TrackerFailInterval;
                        BitTorrentException exception = new BitTorrentException("Tracker returns fail message.");
                        ReturnMessageFail(this, exception);
                    }
                }
                catch (WebException e)
                {
                    _timer.Interval = Setting.TrackerFailInterval;
                    Debug.Assert(ConnectFail != null);
                    ConnectFail(this, e);
                }
                finally
                {
                    _timer.Start();
                }
            }
            catch (NullReferenceException)
            {
                //Nothing to be done.
            }
           
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
        /// Parse the BEncoded message
        /// </summary>
        /// <param name="node">Tracker response BEncoded node</param>
        /// <returns>return the AnnounceResponse class</returns>
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
        /// Get the list of peers
        /// </summary>
        /// <param name="node">The response node that received from tracker</param>
        /// <returns>Return the list of peers</returns>
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
                        Port = port,
                        
                    };
                    peers.Add(peer);
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
                }
            }

            return peers;
        }

        public override int GetHashCode()
        {
            return Uri.GetHashCode();
        }

        public bool Equals(Tracker other)
        {
            return Uri.Equals(other.Uri);
        }

        public void Close()
        {
            lock (this)
            {
                _timer.Stop();
            }
        }

        public void Dispose()
        {
            lock (this)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        #endregion

    }
}
