using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZeraldotNet.LibBitTorrent.BEncoding;
using ZeraldotNet.LibBitTorrent.Trackers;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// The task of download and upload
    /// </summary>
    public class Task
    {
        #region Properties

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string TorrentFileName { get; set; }

        public string SaveAsDirectory { get; set; }

        public MetaInfo MetaInfo { get; set; }

        #endregion

        #region Methods

        public async void Start()
        {
            MetaInfo = MetaInfo.Parse(TorrentFileName);

            Tracker tracker = new Tracker();
            tracker.Url = MetaInfo.Announce;

            AnnounceRequest request = new AnnounceRequest();
            request.InfoHash = MetaInfo.InfoHash;
            request.PeerId = Setting.GetPeerIdString();
            request.Compact = Setting.Compact;
            request.Port = Setting.Port;
            request.Uploaded = 0;
            request.Downloaded = 0;
            request.Event = EventMode.Started;

            AnnounceResponse response = await tracker.Announce(request);

            string hostName = Dns.GetHostName();
            IPAddress localAddress = Dns.GetHostAddresses(hostName)[2];
            string localAddressString = localAddress.ToString();

            foreach (Peer peer in response.Peers)
            {
                if (localAddressString == peer.Host)
                {
                    Console.WriteLine("Ip addresses are the same.");
                }

                else
                {
                    peer.InfoHash = request.InfoHash;
                    peer.PeerId = Setting.GetPeerId();
                    Console.WriteLine("Remote ip address:{0}:{1}", peer.Host, peer.Port);
                    peer.Connect();
                    peer.SendHandshakeMessage(MetaInfo.InfoHash, Setting.GetPeerId());
                    peer.SendUnchokeMessage();
                    peer.SendRequestMessage(0, 0, Setting.BlockSize);
                    peer.SendInterestedMessage();
                    peer.SendRequestMessage(0, 0, Setting.BlockSize);

                    peer.ReceiveAsnyc();
                }
            }
        }

        #endregion
    }
}
