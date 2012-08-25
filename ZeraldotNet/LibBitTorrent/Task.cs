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

        private LinkedList<Peer> _peerList;

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

        #region Constructors

        public Task()
        {
            _trackerSet = new HashSet<Tracker>();
            _listener = new Listener();
            _peerList =new LinkedList<Peer>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Start the task
        /// </summary>
        public async void Start(string torrentFileName, string saveAsDirectory)
        {
            TorrentFileName = torrentFileName;
            SaveAsDirectory = saveAsDirectory;
            MetaInfo = MetaInfo.Parse(TorrentFileName);
            _blockManager = new BlockManager(MetaInfo, SaveAsDirectory);

            InitialAnnounceRequest();

            _maxRequestPieceNumber = 5;
            _sendRequestedNumber = 0;
            _recievePieceNumber = 0;

            InitialLocalAddressStringArray();
            InitialListener();
            _listener.Listen();

            InitialTrackers();
            Parallel.ForEach(_trackerSet, tracker => System.Threading.Tasks.Task.Run(() => tracker.Announce()));
        }

        public async void Stop()
        {
            Parallel.ForEach(_trackerSet, tracker =>
                {
                    tracker.Close();
                    tracker.Dispose();
                });

            Parallel.ForEach(_peerList, peer =>
                {
                    peer.Disconnect();
                    peer.Dispose();
                });

            _listener.Stop();

            if (_blockManager != null)
            {
                _blockManager.Stop();
            }
        }

        public void SetBit(int index)
        {
            _blockManager.SetBit(index);
        }

        public void CheckPieces()
        {
            _blockManager.CheckPieces();
        }

        #region Listener Methods

        private void InitialListener()
        {
            _listener.NewPeer += _listener_NewPeer;
            _listener.ListenFail += _listener_ListenFail;
        }

        void _listener_ListenFail(object sender, string e)
        {
            OnMessage(sender, e);
        }

        void _listener_NewPeer(object sender, Peer e)
        {
            lock (_peerList)
            {
                if (AddToPeerList(e))
                {
                    string message = string.Format("{0} is coming", e);
                    OnMessage(this, message);
                    e.SendHandshakeMessageAsync(MetaInfo.InfoHash, Setting.GetPeerId());
                    e.ReceiveAsnyc();
                }
                else
                {
                    e.Disconnect();
                    e.Dispose();
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
            _announceRequest.Port = Setting.PeerListenningPort;
            _announceRequest.Uploaded = 0;
            _announceRequest.Downloaded = 0;
            _announceRequest.Event = EventMode.Started;
        }

        private void InitialTrackers()
        {
            Tracker primaryTracker = new Tracker(MetaInfo.Announce, _announceRequest);
            primaryTracker.GotAnnounceResponse += tracker_GotAnnounceResponse;
            primaryTracker.ConnectFail += tracker_ConnectFail;
            primaryTracker.ReturnMessageFail += tracker_ReturnMessageFail;
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

        void tracker_ReturnMessageFail(object sender, BitTorrentException e)
        {
            string message = string.Format("{0}:{1}", ((Tracker)sender).Url, e.Message);
            OnMessage(this, message);
        }

        void tracker_GotAnnounceResponse(object sender, AnnounceResponse e)
        {
            lock (_peerList)
            {
                foreach (Peer peer in e.Peers)
                {
                    if (AddToPeerList(peer))
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

        private bool AddToPeerList(Peer peer)
        {
            bool sameIp = Setting.AllowSameIp 
                              ? _peerList.Any(p => p.Host == peer.Host && p.Port == peer.Port)
                              : _peerList.Any(p => p.Host == peer.Host);
            bool localIp = Array.TrueForAll(_localAddressStringArray, s => s != peer.Host);

            bool toBeAdd = !sameIp && localIp;

            if (toBeAdd)
            {
                peer.OnConnected += peer_OnConnected;
                peer.ConnectFail += peer_ConnectFail;
                peer.SendFail += peer_SendFail;
                peer.ReceiveFail += peer_ReceiveFail;
                peer.TimeOut += peer_TimeOut;
                peer.MessageSending += peer_MessageSending;
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
                peer.InitialBooleans(MetaInfo.PieceListCount);
                _peerList.AddLast(peer);
            }
            return toBeAdd;
        }

        void peer_MessageSending(object sender, Message e)
        {
            string message = string.Format("{0}:Sending {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);
        }

        #region Peer Methods

        void peer_TimeOut(object sender, EventArgs e)
        {
            Peer peer = (Peer)sender;
            peer.SendKeepAliveMessageAsync();
        }

        void peer_OnConnected(object sender, EventArgs e)
        {
            string message = string.Format("{0} is connected", sender);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);


            Peer peer = (Peer)sender;
            peer.SendHandshakeMessageAsync(MetaInfo.InfoHash, Setting.GetPeerId());
            peer.ReceiveAsnyc();
        }

        void peer_ConnectFail(object sender, SocketException e)
        {
            string message = string.Format("{0}:Connect fail, {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);

            lock (_peerList)
            {
                Peer peer = (Peer)sender;
                _peerList.Remove(peer);
                peer.Dispose();
            }
        }

        void peer_SendFail(object sender, EventArgs e)
        {
            string message = string.Format("{0}:Send fail", sender);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);

            lock (_peerList)
            {
                Peer peer = (Peer)sender;
                _peerList.Remove(peer);
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

            Peer peer = (Peer) sender;

            if (_blockManager.HaveNone)
            {
                if(Setting.AllowFastPeer && peer.SupportFastPeer)
                {
                    peer.SendHaveNoneMessageAsync();
                }
            }
            else if (_blockManager.HaveAll && Setting.AllowFastPeer && peer.SupportFastPeer)
            {
                peer.SendHaveAllMessageAsync();
            }
            else
            {
                bool[] booleans = _blockManager.GetBitField();
                peer.SendBitfieldMessageAsync(booleans);
            }
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
            RequestNextBlocks(peer, _maxRequestPieceNumber);
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
            bool isInterested = _blockManager.ReceiveHave(e.Index);
            if (isInterested && !peer.AmInterested)
            {
                peer.SendInterestedMessageAsync();
            }
        }

        void peer_BitfieldMessageReceived(object sender, BitfieldMessage e)
        {
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);

            Peer peer = (Peer) sender;
            bool isInterested = _blockManager.ReceiveBitfield(e.GetBitfield());
            if (isInterested && !peer.AmInterested)
            {
                peer.SendInterestedMessageAsync();
            }
            else if (!isInterested && peer.AmInterested)
            {
                peer.SendNotInterestedMessageAsync();
            }
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
            if (_blockManager[e.Index].AllDownloaded)
            {
                if (_blockManager.CheckPiece(e.Index))
                {
                    peer.RemoveRequestedIndex(e.Index);
                    Parallel.ForEach(_peerList, p =>
                        {
                            if (p.IsHandshaked)
                            {
                                p.SendHaveMessageAsync(e.Index);
                            }
                        });
                    
                }
                else
                {
                    _blockManager.ResetDownloaded(e.Index);
                    //RequestPieceByIndex(peer, e.Index);
                }
            }
            RequestNextBlocks(peer, 1);
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
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);
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
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);
        }

        void peer_HaveAllMessageReceived(object sender, HaveAllMessage e)
        {
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);          
        }

        void peer_ReceiveFail(object sender, EventArgs e)
        {
            string message = string.Format("{0}:Receive fail", sender);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);

            lock (_peerList)
            {
                Peer peer = (Peer) sender;
                _peerList.Remove(peer);
                _blockManager.ResetRequested(peer.GetRequestedIndexes());
                peer.Disconnect();
                peer.Dispose();
            }
        }

        /// <summary>
        /// Request the next pieces
        /// </summary>
        /// <param name="peer">The remote peer</param>
        /// <param name="requestPieceNumber">The number of wanted pieces</param>
        private void RequestNextBlocks(Peer peer, int requestPieceNumber)
        {
            if (_blockManager.HaveNextPiece)
            {
                Block[] nexBlockArray = _blockManager.GetNextBlocks(peer.GetBitfield(), requestPieceNumber);

                for (int i = 0; i < nexBlockArray.Length; i++)
                {
                    Block block = nexBlockArray[i];
                    peer.SendRequestMessageAsync(block.Index, block.Begin, block.Length);
                }

                //Piece[] nextPieceArray = _blockManager.GetNextPieces(peer.GetBitfield(), requestPieceNumber);
                //Parallel.For(0, nextPieceArray.Length, i => RequestPieceByIndex(peer, nextPieceArray[i].Index));
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
                if (_blockManager.HaveAll)
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

        ///// <summary>
        ///// Request the required index piece
        ///// </summary>
        ///// <param name="peer">The remote peer</param>
        ///// <param name="index">The index of piece</param>
        //private void RequestPieceByIndex(Peer peer, int index)
        //{
        //    peer.AddRequestedIndex(index);
        //    Piece piece = _blockManager[index];
        //    for (int i = 0; i < piece.BlockCount; i++)
        //    {
        //        peer.SendRequestMessageAsync(index, piece[i].Begin, piece[i].Length);
        //    }
        //}

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