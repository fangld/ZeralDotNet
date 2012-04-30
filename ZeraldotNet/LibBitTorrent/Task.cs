using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ZeraldotNet.LibBitTorrent.BEncoding;
using ZeraldotNet.LibBitTorrent.Messages;
using ZeraldotNet.LibBitTorrent.Storages;
using ZeraldotNet.LibBitTorrent.Trackers;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// The information of download and upload
    /// </summary>
    public class Task
    {
        #region Fields

        private bool[] _booleans;
        private Storage _storage;

        #endregion

        #region Properties

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string TorrentFileName { get; set; }

        public string SaveAsDirectory { get; set; }

        public MetaInfo MetaInfo { get; set; }

        public long Uploaded { get; set; }

        public long Downloaded { get; set; }

        public long Remaining { get; set; }

        #endregion

        #region Methods

        public async void Start()
        {
            MetaInfo = MetaInfo.Parse(TorrentFileName);
            _storage = new Storage(MetaInfo, SaveAsDirectory);

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
                if (localAddressString != peer.Host)
                {
                    peer.HandshakeMessageReceived += new EventHandler<HandshakeMessage>(peer_HandshakeMessageReceived);
                    peer.KeepAliveMessageReceived += new EventHandler<KeepAliveMessage>(peer_KeepAliveMessageReceived);
                    peer.ChokeMessageReceived += new EventHandler<ChokeMessage>(peer_ChokeMessageReceived);
                    peer.UnchokeMessageReceived += new EventHandler<UnchokeMessage>(peer_UnchokeMessageReceived);
                    peer.InterestedMessageReceived += new EventHandler<InterestedMessage>(peer_InterestedMessageReceived);
                    peer.NotInterestedMessageReceived += new EventHandler<NotInterestedMessage>(peer_NotInterestedMessageReceived);
                    peer.HaveMessageReceived += new EventHandler<HaveMessage>(peer_HaveMessageReceived);
                    peer.BitfieldMessageReceived += new EventHandler<BitfieldMessage>(peer_BitfieldMessageReceived);
                    peer.RequestMessageReceived += new EventHandler<RequestMessage>(peer_RequestMessageReceived);                    
                    peer.PieceMessageReceived += new EventHandler<PieceMessage>(peer_PieceMessageReceived);
                    peer.CancelMessageReceived += new EventHandler<CancelMessage>(peer_CancelMessageReceived);
                    peer.InfoHash = request.InfoHash;
                    peer.LocalPeerId = Setting.GetPeerId();
                    //Console.WriteLine("Remote ip address:{0}:{1}", peer.Host, peer.Port);
                    peer.Connect();
                    peer.SendHandshakeMessage(MetaInfo.InfoHash, Setting.GetPeerId());
                    peer.SendUnchokeMessage();
                    peer.SendInterestedMessage();
                    peer.ReceiveAsnyc();
                    peer.SendRequestMessage(0, 0, Setting.BlockSize);
                    //peer.SendRequestMessage(1, 0, Setting.BlockSize);
                }
            }
        }

        void peer_CancelMessageReceived(object sender, CancelMessage e)
        {
            Console.WriteLine("{0}:{1}", sender, e);
        }

        void peer_RequestMessageReceived(object sender, RequestMessage e)
        {
            Peer peer = (Peer) sender;
        }

        void peer_BitfieldMessageReceived(object sender, BitfieldMessage e)
        {
            Console.WriteLine(e);
        }

        void peer_HaveMessageReceived(object sender, HaveMessage e)
        {
            Console.WriteLine(e);
        }

        void peer_HandshakeMessageReceived(object sender, HandshakeMessage e)
        {
            Console.WriteLine(e);
        }

        void peer_KeepAliveMessageReceived(object sender, KeepAliveMessage e)
        {
            Console.WriteLine(e);
        }

        void peer_ChokeMessageReceived(object sender, ChokeMessage e)
        {
            Peer peer = (Peer)(sender);
            peer.PeerChoking = true;
        }

        void peer_UnchokeMessageReceived(object sender, UnchokeMessage e)
        {
            Peer peer = (Peer)(sender);
            peer.PeerChoking = false;
        }

        void peer_InterestedMessageReceived(object sender, InterestedMessage e)
        {
            Peer peer = (Peer)(sender);
            peer.PeerInterested = true;
        }

        void peer_NotInterestedMessageReceived(object sender, NotInterestedMessage e)
        {
            Peer peer = (Peer)(sender);
            peer.PeerInterested = false;
        }

        void peer_PieceMessageReceived(object sender, PieceMessage e)
        {
            if (e.Index == 0)
            {
                Peer peer = (Peer)(sender);
                peer.SendHaveMessage(0);
                peer.SendRequestMessage(1, 0, Setting.BlockSize);
            }
            Console.WriteLine("{0}:{1}", sender, e);
        }

        #endregion
    }
}
