using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Summary description for StorageWrapper.
    /// </summary>
    public class StorageWrapper
    {
        /// <summary>
        ///  一个函数，用来检查片断的完整性
        /// </summary>
        private DataFlunkedDelegate dataFlunkedFunction;

        public DataFlunkedDelegate DataFlunkedFunction
        {
            get { return this.dataFlunkedFunction; }
            set { this.dataFlunkedFunction = value; }
        }

        /// <summary>
        /// 一个事件，在下载完成的时候设置
        /// </summary>
        private FinishedDelegate finishedFunction;

        public FinishedDelegate FinishedFunction
        {
            get { return this.finishedFunction; }
            set { this.finishedFunction = value; }
        }

        /// <summary>
        /// 一个事件，在下载失败的时候设置
        /// </summary>
        private FailedDelegate failedFunction;

        public FailedDelegate FailedFunction
        {
            get { return this.failedFunction; }
            set { this.failedFunction = value; }
        }

        private bool checkHashes;

        public bool CheckHashes
        {
            get { return this.checkHashes; }
            set { this.checkHashes = value; }
        }

        /// <summary>
        /// Storage 对象
        /// </summary>
        private Storage storage;

        public Storage Storage
        {
            get { return this.storage; }
            set { this.storage = value; }
        }

        /// <summary>
        /// 子片断大小
        /// </summary>
        private int requestSize;

        public int RequestSize
        {
            get { return this.requestSize; }
            set { this.requestSize = value; }
        }

        /// <summary>
        /// 文件片断摘要信息
        /// </summary>
        private List<byte[]> hashes;

        public byte[] GetHashes(int index)
        {
            return this.hashes[index];
        }

        public void SetHashed(byte[] hash, int index)
        {
            hashes[index] = hash;
        }

        /// <summary>
        /// 片断大小
        /// </summary>
        private int pieceLength;

        public int PieceLength
        {
            get { return this.pieceLength; }
            set { this.pieceLength = value; }
        }
        
        /// <summary>
        /// 文件总大小
        /// </summary>
        private long totalLength;

        public long TotalLength
        {
            get { return this.totalLength; }
            set { this.totalLength = value; }
        }

        /// <summary>
        /// 未下载完的文件大小
        /// </summary>
        private long amountLeft;

        public long AmountLeft
        {
            get { return this.amountLeft; }
            set { this.amountLeft = value; }
        }

        private int[] numActive;

        /// <summary>
        /// inactive_requests 的值全部被初始化为1，这表示每个片断都需要发送request。
        /// 后面在对磁盘文件检查之后，那些已经获得的片断，在inactiveRequests中对应的是 None，
        /// 表示不需要再为这些片断发送 request了。
        /// </summary>
        private ArrayList inactiveRequests;
        
        private int totalInactive;

        public int TotalInactive
        {
            get { return this.totalInactive; }
            set { this.totalInactive = value; }
        }

        private bool[] have;

        private bool doneChecking;

        public bool DoneChecking
        {
            get { return this.doneChecking; }
            set { this.doneChecking = value; }
        }


        public StorageWrapper(Storage storage, int requestSize, List<byte[]> hashes, int pieceLength,
            FinishedDelegate finishedFunc, FailedDelegate failedFunc, StatusDelegate statusFunc, Flag flag, bool checkHashes, 
            DataFlunkedDelegate dataFlunkedFunc)
        {
            CheckHashes = checkHashes;
            Storage = storage;
            RequestSize = requestSize;
            this.hashes = hashes;
            PieceLength = pieceLength;
            DataFlunkedFunction = dataFlunkedFunc;
            TotalLength = storage.TotalLength;
            AmountLeft = this.totalLength;
            FinishedFunction = finishedFunc;
            FailedFunction = failedFunc;
            inactiveRequests = new ArrayList(hashes.Count);
            numActive = new int[hashes.Count];

            long hashLength = this.pieceLength * this.hashes.Count;

            if (this.totalLength > hashLength)
                throw new BitTorrentException("从Tracker服务器收到错误数据，文件长度太大");
            if (this.totalLength <= hashLength - this.pieceLength)
                throw new BitTorrentException("从Tracker服务器收到错误数据，文件长度太小");

            int i;
            for(i = 0; i < inactiveRequests.Count; i++)
            {
                numActive[i] = 0;
                inactiveRequests.Insert(i, new ArrayList());
            }

            this.totalInactive = 0;
            have = new bool[hashes.Count];

            if (hashes.Count == 0)
            {
                if (finishedFunc != null)
                    finishedFunc();
                return;
            }

            doneChecking = false;

            if (storage.IsExisted)
            {
                statusFunc("正在检查存在的文件", -1, -1, 0, -1);
                for (i = 0; i < hashes.Count; i++)
                {
                    ch
                }
            }

        }

        public void CheckSingle(int index, bool check)
        {
            long low = this.pieceLength * index;
            long high = low + this.pieceLength;

            if (index == hashes.Count - 1)
            {
                high = totalLength;
            }

            int length = (int)(high - low);

            if (check)
            {
                if (!checkHashes || Globals.IsSHA1Equal(this.getSHAHash(storage.Read(low, high)), (byte[])hashes[index]))
                {
                    have[index] = true;
                    amountLeft -= length;
                    if (amountLeft == 0)
                    {
                        finishedFunction();
                    }
                    return;
                }
            }

            ArrayList al = (ArrayList)inactiveRequests[index];
        }

        private byte[] getSHAHash(byte[] piece)
        {
            SHA1Managed shaM = new SHA1Managed();
            return shaM.ComputeHash(piece);
        }

        public bool IsEverythingPending()
        {
            return (totalInactive == 0);
        }
    }
}
