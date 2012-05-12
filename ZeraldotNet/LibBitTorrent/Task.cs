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
        //private int _lastPieceLength;

        private Storage _storage;
        private PieceManager _pieceManager;
        private int _maxRequestBlockNumber;
        private int _currentRequestBlockNumber;
        private bool[][] _remaingBlockList;
        private int _lastPieceLength;
        private int _pieceLength;

        private int sendRequestedNumber;

        private int recievePieceNumber;

//        private int _blockSize;

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

        public async void Start()
        {
            MetaInfo = MetaInfo.Parse(TorrentFileName);
            _storage = new Storage(MetaInfo, SaveAsDirectory);

            Tracker tracker = new Tracker();
            tracker.Url = MetaInfo.Announce;

            _booleans = new bool[MetaInfo.PieceListCount];
            Array.Clear(_booleans, 0, _booleans.Length);
            _pieceManager = new PieceManager(_booleans);

            SingleFileMetaInfo singleFileMetaInfo = MetaInfo as SingleFileMetaInfo;
            _pieceLength = MetaInfo.PieceLength;
            int eachBlockCount = MetaInfo.PieceLength/Setting.BlockSize;
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

            _maxRequestBlockNumber = 10;
            _currentRequestBlockNumber = 0;
            sendRequestedNumber = 0;
            recievePieceNumber = 0;

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
            IPAddress[] localAddressArray = Dns.GetHostAddresses(hostName);

            string[] localAddressStringArray = new string[localAddressArray.Length];
            Parallel.For(0, localAddressArray.Length, i => localAddressStringArray[i] = localAddressArray[i].ToString());

            foreach (Peer peer in response.Peers)
            {
                if (Array.TrueForAll(localAddressStringArray, s => s != peer.Host))
                {
                    peer.Connected += peer_Connected;
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
                    peer.InfoHash = request.InfoHash;
                    peer.LocalPeerId = Setting.GetPeerId();
                    peer.InitialBooleans(MetaInfo.PieceListCount);
                    peer.ConnectAsync();
                }
            }
        }

        void peer_Connected(object sender, EventArgs e)
        {
            Peer peer = (Peer) sender;
            Console.WriteLine("{0}:Connected", sender);
            peer.SendHandshakeMessage(MetaInfo.InfoHash, Setting.GetPeerId());
            peer.SendUnchokeMessage();
            peer.SendInterestedMessage();
            peer.ReceiveAsnyc();
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
            RequestPieces(peer, _maxRequestBlockNumber);
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
            int rcvLength = e.GetBlock().Length;
            int rcvIndex = e.Index;
            int blockOffset = rcvBegin/Setting.BlockSize;
            _remaingBlockList[rcvIndex][blockOffset] = true;
            if (Array.TrueForAll(_remaingBlockList[rcvIndex], b=>b))
            {
                Peer peer = (Peer)sender;
                if (Check(rcvIndex))
                {
                    _pieceManager.SetDownloaded(e.Index);
                    peer.SendHaveMessage(e.Index);
                    RequestPieces(peer, 1);
                }
                else
                {
                    Array.Clear(_remaingBlockList[rcvIndex], 0, _remaingBlockList[rcvIndex].Length);
                }
            }
        }

        private bool Check(int index)
        {
            int pieceLength = index != MetaInfo.PieceListCount - 1 ? MetaInfo.PieceLength : _lastPieceLength;
            byte[] piece = new byte[pieceLength];
            long offset = MetaInfo.PieceLength*index;
            int hashOffset = 20*index;
            _storage.Read(piece, offset, pieceLength);
            byte[] pieceHash  =Globals.Sha1.ComputeHash(piece);
            byte[] orgHash = MetaInfo.GetPiece(index);
            for (int i = 0; i < pieceHash.Length; i++)
            {
                if (pieceHash[i] != orgHash[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void RequestPieces(Peer peer, int requestPieceNumber)
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
                    RequestNextPiece(peer, nextPieceArray[i].Index);
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

        private void RequestNextPiece(Peer peer, int index)
        {
            if (index != MetaInfo.PieceListCount - 1)
            {
                int offset = 0;
                for (int j = 0; j < _remaingBlockList[index].Length; j++)
                {
                    peer.SendRequestMessageAsync(index, offset, Setting.BlockSize);
                    offset += Setting.BlockSize;
                }
            }
            else
            {
                int offset = 0;
                for (int j = 0; j < _remaingBlockList[index].Length - 1; j++)
                {
                    peer.SendRequestMessageAsync(index, offset, Setting.BlockSize);
                    offset += Setting.BlockSize;
                }
                int lastBlockLength = _lastPieceLength - offset;
                peer.SendRequestMessageAsync(index, offset, lastBlockLength);
            }
        }

        #endregion
    }
}
