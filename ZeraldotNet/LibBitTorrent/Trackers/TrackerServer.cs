using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
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
    public class TrackerServer
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
        public AnnounceResponse Announce(AnnounceRequest request)
        {
            string uri =
                string.Format(
                    "{0}?info_hash={1}&peer_id={2}&port={3}&uploaded={4}&downloaded={5}&left={6}&compact={7}&no_peer_id={8}&event={9}",
                    Url, request.InfoHash, request.PeerId, request.Port, request.Uploaded, request.Downloaded,
                    request.Left, request.Compact, request.NoPeerId, request.Event.ToString().ToLower());
            Console.WriteLine(uri);
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(uri);
            Stream stream = httpRequest.GetResponse().GetResponseStream();

            int count = Setting.BufferSize;
            int readLength;
            byte[] bytes = new byte[Setting.BufferSize];
            MemoryStream ms = new MemoryStream(Setting.BufferSize);
            do
            {
                readLength = stream.Read(bytes, 0, count);
                ms.Write(bytes,0, readLength);
                //for (int i = 0; i < readLength; i++)
                //{
                //    Console.WriteLine("{0}:char:{1}, byte:{2}", i, (char)bytes[i], bytes[i]);
                //}
            } while (readLength != 0);
            var node = BEncoder.Decode(ms.ToArray());
            var result = TrackerDecoding.Decode(node);
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

        #endregion
    }
}
