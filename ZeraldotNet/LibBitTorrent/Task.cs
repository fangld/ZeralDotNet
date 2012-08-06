using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ZeraldotNet.LibBitTorrent.BEncoding;
using ZeraldotNet.LibBitTorrent.Messages;
using ZeraldotNet.LibBitTorrent.Storages;
using ZeraldotNet.LibBitTorrent.Trackers;
using ZeraldotNet.LibBitTorrent.Pieces;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// The information of download and upload
    /// </summary>
    public class Task
    {
        #region Fields
        private BlockManager _blockManager;
        private int _maxRequestPieceNumber;
        private string[] _localAddressStringArray;
        private AnnounceRequest _announceRequest;
        
        private HashSet<Peer> _peerSet;

        private HashSet<Tracker> _trackerSet;

        private int _sendRequestedNumber;
        private int _recievePieceNumber;
        private Listener _listener;

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

        public bool Finished { get; set; }

        #endregion

        #region Events

        public event EventHandler OnFinished;

        public event EventHandler<string> OnMessage;

        #endregion

        #region Methods

        /// <summary>
        /// Start the task
        /// </summary>
        public async void Start()
        {
            MetaInfo = MetaInfo.Parse(TorrentFileName);
            _blockManager = new BlockManager(MetaInfo, SaveAsDirectory);

            InitialAnnounceRequest();

            _maxRequestPieceNumber = 5;
            _sendRequestedNumber = 0;
            _recievePieceNumber = 0;
            _peerSet = new HashSet<Peer>();

            InitialLocalAddressStringArray();
            InitialListener();
            _listener.Listen();

            InitialTrackers();
            Parallel.ForEach(_trackerSet, tracker => System.Threading.Tasks.Task.Run(() => tracker.Announce()));
        }

        #region Listener Methods

        private void InitialListener()
        {
            _listener = new Listener();
            _listener.NewPeer += _listener_NewPeer;
            _listener.ListenFail += _listener_ListenFail;
        }

        void _listener_ListenFail(object sender, string e)
        {
            OnMessage(sender, e);
        }

        void _listener_NewPeer(object sender, Peer e)
        {
            lock (_peerSet)
            {
                if (AddToPeerSet(e))
                {
                    e.SendHandshakeMessageAsync(MetaInfo.InfoHash, Setting.GetPeerId());
                    bool[] booleans = _blockManager.GetBooleans();
                    e.SendBitfieldMessageAsync(booleans);
                    e.ReceiveAsnyc();
                }
            }
        }

        #endregion

        #region Trackers Methods

        private void InitialAnnounceRequest()
        {
            _announceRequest = new AnnounceRequest();
            _announceRequest.InfoHash = MetaInfo.InfoHash;
            _announceRequest.PeerId = Setting.GetPeerIdString();
            _announceRequest.Compact = Setting.Compact;
            _announceRequest.Port = Setting.ListenPort;
            _announceRequest.Uploaded = 0;
            _announceRequest.Downloaded = 0;
            _announceRequest.Event = EventMode.Started;
        }

        private void InitialTrackers()
        {
            _trackerSet = new HashSet<Tracker>();
            Tracker primaryTracker = new Tracker(MetaInfo.Announce, _announceRequest);
            primaryTracker.GotAnnounceResponse += tracker_GotAnnounceResponse;
            primaryTracker.ConnectFail += tracker_ConnectFail;
            _trackerSet.Add(primaryTracker);

            for (int i = 0; i < MetaInfo.AnnounceArrayListCount; i++)
            {
                IList<string> announceList = MetaInfo.GetAnnounceList(i);
                for (int j = 0; j < announceList.Count; j++)
                {
                    Tracker tracker = new Tracker(announceList[j], _announceRequest);
                    if (!_trackerSet.Contains(tracker))
                    {
                        tracker.GotAnnounceResponse += tracker_GotAnnounceResponse;
                        tracker.ConnectFail += tracker_ConnectFail;
                        _trackerSet.Add(tracker);
                    }
                }
            }
        }

        void tracker_GotAnnounceResponse(object sender, AnnounceResponse e)
        {
            lock (_peerSet)
            {
                foreach (Peer peer in e.Peers)
                {
                    if (AddToPeerSet(peer))
                    {
                        peer.ConnectAsync();
                    }
                }
            }
        }

        void tracker_ConnectFail(object sender, WebException e)
        {
            string message = string.Format("{0}:{1}", ((Tracker)sender).Url, e.Message);
            OnMessage(this, message);
        }

        #endregion

        private bool AddToPeerSet(Peer peer)
        {
            bool isNewPeer = !_peerSet.Contains(peer) &&
                             Array.TrueForAll(_localAddressStringArray, s => s != peer.Host);
            if (isNewPeer)
            {
                peer.OnConnected += peer_OnConnected;
                peer.ConnectFail += peer_ConnectFail;
                peer.SendFail += peer_SendFail;
                peer.ReceiveFail += peer_ReceiveFail;
                peer.TimeOut += peer_TimeOut;
                peer.HandshakeMessageReceived += peer_HandshakeMessageReceived;
                peer.KeepAliveMessageReceived += peer_KeepAliveMessageReceived;
                peer.ChokeMessageReceived += peer_ChokeMessageReceived;
                peer.UnchokeMessageReceived += peer_UnchokeMessageReceived;
                peer.InterestedMessageReceived += peer_InterestedMessageReceived;
                peer.NotInterestedMessageReceived += peer_NotInterestedMessageReceived;
                peer.HaveMessageReceived += peer_HaveMessageReceived;
                peer.BitfieldMessageReceived += peer_BitfieldMessageReceived;
                peer.RequestMessageReceived += peer_RequestMessageReceived;
                peer.PieceMessageReceived += peer_PieceMessageReceived;
                peer.CancelMessageReceived += peer_CancelMessageReceived;
                peer.HaveAllMessageReceived += peer_HaveAllMessageReceived;
                peer.HaveNoneMessageReceived += peer_HaveNoneMessageReceived;
                peer.SuggestPieceMessageReceived += peer_SuggestPieceMessageReceived;
                peer.RejectRequestMessageReceived += peer_RejectRequestMessageReceived;
                peer.AllowedFastMessageReceived += peer_AllowedFastMessageReceived;
                peer.PortMessageReceived += peer_PortMessageReceived;
                peer.InfoHash = _announceRequest.InfoHash;
                peer.PeerId = Setting.GetPeerId();
                peer.InitialBooleans(MetaInfo.PieceListCount);
                _peerSet.Add(peer);
            }
            return isNewPeer;
        }

        #region Peer Methods

        void peer_TimeOut(object sender, EventArgs e)
        {
            Peer peer = (Peer)sender;
            peer.SendKeepAliveMessageAsync();
        }

        void peer_OnConnected(object sender, EventArgs e)
        {
            string message = string.Format("{0}:OnConnected", sender);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);

            Peer peer = (Peer)sender;
            peer.SendHandshakeMessageAsync(MetaInfo.InfoHash, Setting.GetPeerId());
            peer.SendUnchokeMessageAsync();
            peer.SendInterestedMessageAsync();
            peer.ReceiveAsnyc();
        }

        void peer_ConnectFail(object sender, EventArgs e)
        {
            string message = string.Format("{0}:ConnectFail", sender);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);

            lock (_peerSet)
            {
                Peer peer = (Peer)sender;
                _peerSet.Remove(peer);
                peer.Dispose();
            }
        }

        void peer_SendFail(object sender, EventArgs e)
        {
            lock (_peerSet)
            {
                Peer peer = (Peer)sender;
                _peerSet.Remove(peer);
                _blockManager.ResetRequested(peer.GetRequestedIndexes());
                peer.Disconnect();
                peer.Dispose();
            }
        }

        void peer_HandshakeMessageReceived(object sender, HandshakeMessage e)
        {
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);

            Peer peer = (Peer)sender;
            peer.PeerId = e.PeerId;
            peer.InfoHash = e.InfoHash;
            peer.IsDht = e.IsDht;
            peer.IsPeerExchange = e.IsPeerExchange;
            peer.IsFastExtension = e.IsFastExtension;
        }

        void peer_KeepAliveMessageReceived(object sender, KeepAliveMessage e)
        {
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);
        }

        void peer_ChokeMessageReceived(object sender, ChokeMessage e)
        {
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);
        }

        void peer_UnchokeMessageReceived(object sender, UnchokeMessage e)
        {
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);
            Peer peer = (Peer)(sender);
            RequestNextPieces(peer, _maxRequestPieceNumber);
        }

        void peer_InterestedMessageReceived(object sender, InterestedMessage e)
        {
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);
            Peer peer = (Peer)(sender);
            if (peer.AmChoking)
            {
                peer.SendUnchokeMessageAsync();
            }
        }

        void peer_NotInterestedMessageReceived(object sender, NotInterestedMessage e)
        {
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);
        }

        void peer_HaveMessageReceived(object sender, HaveMessage e)
        {
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);
            Peer peer = (Peer)sender;
            peer.SetBoolean(e.Index);
            _blockManager.AddExistedNumber(e.Index);
        }

        void peer_BitfieldMessageReceived(object sender, BitfieldMessage e)
        {
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);
            Peer peer = (Peer)sender;
            peer.SetBooleans(e.GetBooleans());
            _blockManager.AddExistedNumber(e.GetBooleans());
        }

        void peer_RequestMessageReceived(object sender, RequestMessage e)
        {
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);

            Peer peer = (Peer)sender;
            if (!peer.AmChoking && peer.PeerInterested && _blockManager[e.Index].Checked)
            {
                byte[] buffer = _blockManager.Read(e.Index, e.Begin, e.Length);
                peer.SendPieceMessageAsync(e.Index, e.Begin, buffer);
            }
        }

        void peer_PieceMessageReceived(object sender, PieceMessage e)
        {
            //lock ((object)_recievePieceNumber)
            //{
            //    _recievePieceNumber++;
            //    Console.WriteLine("_recievePieceNumber:{0}, piece:{1}", _recievePieceNumber, e);
            //}
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);
            _blockManager.Write(e.GetBlock(), e.Index, e.Begin);

            Peer peer = (Peer)sender;
            //if received piece is corrent, it will request the next piece, 
            //otherwise it will request the crash piece again.
            if (_blockManager[e.Index].Downloaded)
            {
                if (_blockManager.CheckPiece(e.Index))
                {
                    peer.RemoveRequestedIndex(e.Index);
                    Parallel.ForEach(_peerSet, p =>
                        {
                            if (p.IsConnected)
                            {
                                p.SendHaveMessageAsync(e.Index);
                            }
                        });
                    RequestNextPieces(peer, 1);
                }
                else
                {
                    _blockManager.ResetDownloaded(e.Index);
                    RequestPieceByIndex(peer, e.Index);
                }
            }
        }

        void peer_CancelMessageReceived(object sender, CancelMessage e)
        {
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);
        }

        void peer_PortMessageReceived(object sender, PortMessage e)
        {
            throw new NotImplementedException();
        }

        void peer_AllowedFastMessageReceived(object sender, AllowedFastMessage e)
        {
            throw new NotImplementedException();
        }

        void peer_RejectRequestMessageReceived(object sender, RejectRequestMessage e)
        {
            throw new NotImplementedException();
        }

        void peer_SuggestPieceMessageReceived(object sender, SuggestPieceMessage e)
        {
            throw new NotImplementedException();
        }

        void peer_HaveNoneMessageReceived(object sender, HaveNoneMessage e)
        {
            throw new NotImplementedException();
        }

        void peer_HaveAllMessageReceived(object sender, HaveAllMessage e)
        {
            throw new NotImplementedException();            
        }

        void peer_ReceiveFail(object sender, EventArgs e)
        {
            lock (_peerSet)
            {
                Peer peer = (Peer)sender;
                _peerSet.Remove(peer);
                peer.Disconnect();
                _blockManager.ResetRequested(peer.GetRequestedIndexes());
                peer.Dispose();
            }
        }

        /// <summary>
        /// Request the next pieces
        /// </summary>
        /// <param name="peer">The remote peer</param>
        /// <param name="requestPieceNumber">The number of wanted pieces</param>
        private void RequestNextPieces(Peer peer, int requestPieceNumber)
        {
            if (_blockManager.HaveNextPiece)
            {
                Piece[] nextPieceArray = _blockManager.GetNextPieces(peer.GetBooleans(), requestPieceNumber);
                Parallel.For(0, nextPieceArray.Length, i => RequestPieceByIndex(peer, nextPieceArray[i].Index));
                //for (int i = 0; i < nextPieceArray.Length; i++)
                //{
                //    //lock ((object)_sendRequestedNumber)
                //    //{
                //    //    _sendRequestedNumber++;
                //    //    Console.WriteLine("_sendRequestedNumber:{0}, Index:{1}", _sendRequestedNumber, nextPieceArray[i].Index);
                //    //}
                //    RequestPieceByIndex(peer, nextPieceArray[i].Index);
                //}
            }
            else
            {
                if (_blockManager.Completed)
                {
                    if (!Finished)
                    {
                        Finished = true;
                        Debug.Assert(OnFinished != null);
                        OnFinished(this, null);
                        OnMessage(this, "Task is finished!");
                    }
                }
            }
        }

        /// <summary>
        /// Request the required index piece
        /// </summary>
        /// <param name="peer">The remote peer</param>
        /// <param name="index">The index of piece</param>
        private void RequestPieceByIndex(Peer peer, int index)
        {
            peer.AddRequestedIndex(index);
            Piece piece = _blockManager[index];
            for (int i = 0; i < piece.BlockCount; i++)
            {
                peer.SendRequestMessageAsync(index, piece[i].Begin, piece[i].Length);
            }
        }

        #endregion

        private void InitialLocalAddressStringArray()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] localAddressArray = Dns.GetHostAddresses(hostName);

           _localAddressStringArray = new string[localAddressArray.Length];
            Parallel.For(0, localAddressArray.Length, i => _localAddressStringArray[i] = localAddressArray[i].ToString());
        }

        #endregion
    }
}