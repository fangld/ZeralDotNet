using System;
using System.IO;
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
        private List<List<InactiveRequest>> inactiveRequests;
        
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
            amountLeft = this.totalLength;

            long hashLength = this.pieceLength * this.hashes.Count;

            if (this.totalLength > hashLength)
                throw new BitTorrentException("从Tracker服务器收到错误数据，文件长度太大");
            if (this.totalLength <= hashLength - this.pieceLength)
                throw new BitTorrentException("从Tracker服务器收到错误数据，文件长度太小");
            
            FinishedFunction = finishedFunc;
            FailedFunction = failedFunc;
            numActive = new int[hashes.Count];
            inactiveRequests = new List<List<InactiveRequest>>();

            int i;
            for(i = 0; i < hashes.Count; i++)
            {
                numActive[i] = 0;
                inactiveRequests.Insert(i, new List<InactiveRequest>());
            }

            totalInactive = 0;
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
                if (statusFunc != null)
                    statusFunc("正在检查存在的文件", -1, -1, 0, -1);
                for (i = 0; i < hashes.Count; i++)
                {
                    //Console.WriteLine(hashes.Count);
                    //Console.WriteLine(i);

                    CheckSingle(i, true);
                    if (flag.IsSet)
                        return;
                    if (statusFunc != null)
                        statusFunc(null, -1, -1, ((double)(i + 1) / (double)hashes.Count), -1);
                }

                Console.WriteLine("SW的剩余长度:{0}", amountLeft);
            }


            else
            {
                for (i = 0; i < hashes.Count; i++)
                    CheckSingle(i, false);
            }

            doneChecking = true;
        }

        public bool DoIHaveAnything()
        {
            return amountLeft < totalLength;
        }

        public bool IsEverythingPending()
        {
            return totalInactive == 0;
        }

        public bool[] GetHaveList()
        {
            return have;
        }

        public bool DoIHave(int index)
        {
            return have[index];
        }

        public bool DoIHaveRequests(int index)
        {
            return inactiveRequests[index].Count != 0;
        }

        public InactiveRequest NewRequest(int index)
        {
            int i;

            numActive[index]++;
            totalInactive--;
            List<InactiveRequest> requests = inactiveRequests[index];
            InactiveRequest request = requests[0];

            for (i = 1; i < requests.Count; i++)
            {
                if ((requests[i]).CompareTo(request) < 0)
                    request = requests[i];
            }
            requests.Remove(request);
            return request;
        }

        public void PieceCameIn(int index, long begin, byte[] piece)
        {
            try
            {
                WritePiece(index, begin, piece);
            }
            catch (IOException ioEx)
            {
                failedFunction("IO错误" + ioEx.Message);
            }
        }

        private void WritePiece(int index, long begin, byte[] piece)
        {
            storage.Write(index * pieceLength + begin, piece);
            numActive[index]--;
            if (inactiveRequests[index].Count == 0 && numActive[index] == 0)
            {
                CheckSingle(index, true);
            }
        }

        public byte[] GetPiece(int index, int begin, int length)
        {
            try
            {
                return ReadPiece(index, begin, length);
            }
            catch (IOException ioEx)
            {
                failedFunction("IO错误" + ioEx.Message);
                return null;
            }
        }

        private byte[] ReadPiece(int index, int begin, int length)
        {
            if (!have[index])
            {
                return null;
            }
            long low = pieceLength * index + begin;
            if (low + length > Math.Min(totalLength, pieceLength * (index + 1)))
            {
                return null;
            }
            return storage.Read(low, length);
        }

        public void RequestLost(int index, InactiveRequest request)
        {
            List<InactiveRequest> requestList = inactiveRequests[index];
            requestList.Add(request);
            requestList.Sort();
            numActive[index]--;
            totalInactive++;
        }

        private void CheckSingle(int index, bool check)
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
                if (!checkHashes || Globals.IsSHA1Equal(this.getSHAHash(storage.Read(low, length)), (byte[])hashes[index]))
                {
                    have[index] = true;
                    amountLeft -= length;
                    if (amountLeft == 0)
                    {
                        if (finishedFunction != null)
                            finishedFunction();
                        Console.WriteLine("AmountLeft(0):{0}", this.amountLeft);
                    }
                    return;
                }
            }

            Console.WriteLine("AmountLeft:{0}", this.amountLeft);

            List<InactiveRequest> requestList = inactiveRequests[index];

            int i = 0;
            while (i + requestSize < length)
            {
                InactiveRequest item = new InactiveRequest(i, requestSize);
                requestList.Add(item);
                totalInactive++;
                i += requestSize;
            }

            InactiveRequest lastItem = new InactiveRequest(i, length - i);
            requestList.Add(lastItem);
            totalInactive++;
        }

        private byte[] getSHAHash(byte[] piece)
        {
            SHA1Managed shaM = new SHA1Managed();
            return shaM.ComputeHash(piece);
        }
    }
}
