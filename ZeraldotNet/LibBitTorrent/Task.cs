using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

        private bool[] _booleans;
        private Storage _storage;
        private PieceManager _pieceManager;
        private int _maxRequestBlockNumber;
        private int _currentRequestBlockNumber;
        private bool[][] _remaingBlockList;
        private int _lastPieceLength;
        private int _pieceLength;
        private string[] _localAddressStringArray;
        private AnnounceRequest _announceRequest;
        
        private HashSet<Peer> _peerSet;

        private HashSet<Tracker> _trackerSet;
        private object _peerListSyncObj;

        private object _syncObj;

        private int sendRequestedNumber;
        private int recievePieceNumber;

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

        public void Start()
        {
            _syncObj = new object();
            MetaInfo = MetaInfo.Parse(TorrentFileName);
            _storage = new Storage(MetaInfo, SaveAsDirectory);

            

            InitialAnnounceRequest();


            _booleans = new bool[MetaInfo.PieceListCount];
            Array.Clear(_booleans, 0, _booleans.Length);
            _pieceManager = new PieceManager(_booleans);

            InitialRemainingBlockFlags();

            _maxRequestBlockNumber = 5;
            _currentRequestBlockNumber = 0;
            sendRequestedNumber = 0;
            recievePieceNumber = 0;
            _peerSet = new HashSet<Peer>();

            InitialLocalAddressStringArray();
            InitialTrackerSet();

            Parallel.ForEach(_trackerSet, tracker => tracker.Announce());
        }

        private void InitialTrackerSet()
        {
            _trackerSet = new HashSet<Tracker>();
            Tracker primaryTracker = new Tracker(MetaInfo.Announce, _announceRequest);
            primaryTracker.GotAnnounceResponse += tracker_GotAnnounceResponse;
            primaryTracker.ConnectFail += (sender, arg) => OnMessage(sender, arg.Message);
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
                        tracker.ConnectFail += (sender, arg) => OnMessage(sender, arg.Message);
                        _trackerSet.Add(tracker);
                    }
                }
            }
        }

        private void InitialAnnounceRequest()
        {
            _announceRequest = new AnnounceRequest();
            _announceRequest.InfoHash = MetaInfo.InfoHash;
            _announceRequest.PeerId = Setting.GetPeerIdString();
            _announceRequest.Compact = Setting.Compact;
            _announceRequest.Port = Setting.Port;
            _announceRequest.Uploaded = 0;
            _announceRequest.Downloaded = 0;
            _announceRequest.Event = EventMode.Started;
        }

        private void InitialLocalAddressStringArray()
        {
            string hostName = Dns.GetHostName();
            IPAddress[] localAddressArray = Dns.GetHostAddresses(hostName);

           _localAddressStringArray = new string[localAddressArray.Length];
            Parallel.For(0, localAddressArray.Length, i => _localAddressStringArray[i] = localAddressArray[i].ToString());
        }

        private void InitialRemainingBlockFlags()
        {
            SingleFileMetaInfo singleFileMetaInfo = MetaInfo as SingleFileMetaInfo;
            _pieceLength = MetaInfo.PieceLength;
            int eachBlockCount = MetaInfo.PieceLength / Setting.BlockSize;
            long fullPieceLength = (long)(MetaInfo.PieceListCount - 1) * (long)_pieceLength;
            _lastPieceLength = (int)(singleFileMetaInfo.Length - fullPieceLength);

            _remaingBlockList = new bool[_pieceLength][];
            Parallel.For(0, MetaInfo.PieceListCount, i =>
            {
                bool[] blocks = new bool[eachBlockCount];
                Array.Clear(blocks, 0, eachBlockCount);
                _remaingBlockList[i] = blocks;
            });

            int lastBlockcount = _lastPieceLength / Setting.BlockSize + 1;
            bool[] lastBlocks = new bool[lastBlockcount];
            Array.Clear(lastBlocks, 0, lastBlockcount);
            _remaingBlockList[_pieceLength - 1] = lastBlocks;
        }

        void tracker_GotAnnounceResponse(object sender, AnnounceResponse e)
        {
            lock(_peerSet)
            {
                foreach (Peer peer in e.Peers)
                {
                    if (!_peerSet.Contains(peer) && Array.TrueForAll(_localAddressStringArray, s => s != peer.Host))
                    {
                        peer.OnConnected += PeerOnConnected;
                        peer.ConnectFail += peer_ConnectFail;
                        peer.HandshakeMessageReceived += new EventHandler<HandshakeMessage>(peer_HandshakeMessageReceived);
                        peer.KeepAliveMessageReceived += new EventHandler<KeepAliveMessage>(peer_KeepAliveMessageReceived);
                        peer.ChokeMessageReceived += new EventHandler<ChokeMessage>(peer_ChokeMessageReceived);
                        peer.UnchokeMessageReceived += new EventHandler<UnchokeMessage>(peer_UnchokeMessageReceived);
                        peer.InterestedMessageReceived += new EventHandler<InterestedMessage>(peer_InterestedMessageReceived);
                        peer.NotInterestedMessageReceived +=
                            new EventHandler<NotInterestedMessage>(peer_NotInterestedMessageReceived);
                        peer.HaveMessageReceived += new EventHandler<HaveMessage>(peer_HaveMessageReceived);
                        peer.BitfieldMessageReceived += new EventHandler<BitfieldMessage>(peer_BitfieldMessageReceived);
                        peer.RequestMessageReceived += new EventHandler<RequestMessage>(peer_RequestMessageReceived);
                        peer.PieceMessageReceived += new EventHandler<PieceMessage>(peer_PieceMessageReceived);
                        peer.CancelMessageReceived += new EventHandler<CancelMessage>(peer_CancelMessageReceived);
                        peer.InfoHash = _announceRequest.InfoHash;
                        peer.LocalPeerId = Setting.GetPeerId();
                        peer.InitialBooleans(MetaInfo.PieceListCount);
                        _peerSet.Add(peer);
                        peer.ConnectAsync();
                    }
                }
            }
        }

        void PeerOnConnected(object sender, EventArgs e)
        {
            Peer peer = (Peer) sender;
            Console.WriteLine("{0}:OnConnected", sender);
            peer.SendHandshakeMessage(MetaInfo.InfoHash, Setting.GetPeerId());
            peer.SendUnchokeMessage();
            peer.SendInterestedMessage();
            peer.ReceiveAsnyc();
        }

        void peer_ConnectFail(object sender, EventArgs e)
        {
            Peer peer = (Peer)sender;
            lock (_peerSet)
            {
                peer.Disconnect();
                _peerSet.Remove(peer);
            }
        }

        void peer_CancelMessageReceived(object sender, CancelMessage e)
        {
            Console.WriteLine("{0}:Received {1}", sender, e);
        }

        void peer_RequestMessageReceived(object sender, RequestMessage e)
        {
            Console.WriteLine("{0}:Received {1}", sender, e);
        }

        void peer_BitfieldMessageReceived(object sender, BitfieldMessage e)
        {
            Peer peer = (Peer) sender;
            peer.SetBooleans(e.GetBooleans());
            _pieceManager.AddExistingNumber(e.GetBooleans());
            Console.WriteLine("{0}:Received {1}", sender, e);
        }

        void peer_HaveMessageReceived(object sender, HaveMessage e)
        {
            Console.WriteLine("{0}:Received {1}", sender, e);
            Peer peer = (Peer) sender;
            peer.SetBoolean(e.Index);
            _pieceManager.AddExistingNumber(e.Index);
        }

        void peer_HandshakeMessageReceived(object sender, HandshakeMessage e)
        {
            Console.WriteLine("{0}:Received {1}", sender, e);
        }

        void peer_KeepAliveMessageReceived(object sender, KeepAliveMessage e)
        {
            Console.WriteLine("{0}:Received {1}", sender, e);
        }

        void peer_ChokeMessageReceived(object sender, ChokeMessage e)
        {
            Console.WriteLine("{0}:Received {1}", sender, e);
            Peer peer = (Peer)(sender);
            peer.PeerChoking = true;
        }

        void peer_UnchokeMessageReceived(object sender, UnchokeMessage e)
        {
            Console.WriteLine("{0}:Received {1}", sender, e);
            Peer peer = (Peer)(sender);
            peer.PeerChoking = false;
            RequestNextPieces(peer, _maxRequestBlockNumber);
        }

        void peer_InterestedMessageReceived(object sender, InterestedMessage e)
        {
            Console.WriteLine("{0}:Received {1}", sender, e);
            Peer peer = (Peer)(sender);
            peer.PeerInterested = true;
        }

        void peer_NotInterestedMessageReceived(object sender, NotInterestedMessage e)
        {
            Console.WriteLine("{0}:Received {1}", sender, e);
            Peer peer = (Peer)(sender);
            peer.PeerInterested = false;
        }

        void peer_PieceMessageReceived(object sender, PieceMessage e)
        {
            //lock ((object)recievePieceNumber)
            //{
            //    recievePieceNumber++;
            //    Console.WriteLine("recievePieceNumber:{0}, piece:{1}", recievePieceNumber, e);
            //}
            string message = string.Format("{0}:Received {1}", sender, e);
            Debug.Assert(OnMessage != null);
            OnMessage(this, message);
            _storage.Write(e.GetBlock(), MetaInfo.PieceLength*e.Index + e.Begin);
            int rcvBegin = e.Begin;
            int rcvIndex = e.Index;
            int blockOffset = rcvBegin/Setting.BlockSize;
            lock (_remaingBlockList[rcvIndex])
            {
                _remaingBlockList[rcvIndex][blockOffset] = true;
                if (Array.TrueForAll(_remaingBlockList[rcvIndex], b => b))
                {
                    Peer peer = (Peer)sender;
                    //if received piece is corrent, request the next piece, otherwise request the crash piece again
                    if (CheckPiece(rcvIndex))
                    {
                        _pieceManager.SetDownloaded(e.Index);
                        Parallel.ForEach(_peerSet, p =>
                                                       {
                                                           if (p.IsConnected)
                                                           {
                                                               p.SendHaveMessage(e.Index);
                                                           }
                                                       });
                        //peer.SendHaveMessage(e.Index);
                        RequestNextPieces(peer, 1);
                    }
                    else
                    {
                        Array.Clear(_remaingBlockList[rcvIndex], 0, _remaingBlockList[rcvIndex].Length);
                        RequestPieceByIndex(peer, rcvIndex);
                    }
                }
            }

        }

        /// <summary>
        /// Check the correntness of piece
        /// </summary>
        /// <param name="index">The index of piece</param>
        /// <returns>If received piece is correct return true, otherwise return false</returns>
        private bool CheckPiece(int index)
        {
            int pieceLength = index != MetaInfo.PieceListCount - 1 ? MetaInfo.PieceLength : _lastPieceLength;
            byte[] piece = new byte[pieceLength];
            long offset = MetaInfo.PieceLength*index;
            _storage.Read(piece, offset, pieceLength);
            byte[] rcvPieceHash  =Globals.Sha1.ComputeHash(piece);
            byte[] metaHash = MetaInfo.GetPiece(index);
            for (int i = 0; i < rcvPieceHash.Length; i++)
            {
                if (rcvPieceHash[i] != metaHash[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Request the next pieces
        /// </summary>
        /// <param name="peer">The remote peer</param>
        /// <param name="requestPieceNumber">The number of wanted pieces</param>
        private void RequestNextPieces(Peer peer, int requestPieceNumber)
        {
            lock (_pieceManager)
            {
                if (_pieceManager.HaveNextPiece)
                {
                    Piece[] nextPieceArray = _pieceManager.GetNextIndex(requestPieceNumber);
                    for (int i = 0; i < nextPieceArray.Length; i++)
                    {
                        //lock((object)sendRequestedNumber)
                        //{
                        //    sendRequestedNumber++;
                        //    Console.WriteLine("sendRequestedNumber:{0}, Index:{1}", sendRequestedNumber, nextPieceArray[i].Index);
                        //}
                        RequestPieceByIndex(peer, nextPieceArray[i].Index);
                    }
                }
                else
                {
                    if (_pieceManager.AllDownloaded)
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
        }

        /// <summary>
        /// Request the required index piece
        /// </summary>
        /// <param name="peer">The remote peer</param>
        /// <param name="index">The index of piece</param>
        private void RequestPieceByIndex(Peer peer, int index)
        {
            lock (_remaingBlockList[index])
            {
                bool[] blocks = _remaingBlockList[index];
                int offset = 0;
                for (int i = 0; i < blocks.Length - 1; i++)
                {
                    peer.SendRequestMessageAsync(index, offset, Setting.BlockSize);
                    offset += Setting.BlockSize;
                }
                int lastBlockLength = index != MetaInfo.PieceListCount - 1 ? Setting.BlockSize : _lastPieceLength - offset;
                peer.SendRequestMessageAsync(index, offset, lastBlockLength);
            }
        }

        #endregion
    }
}