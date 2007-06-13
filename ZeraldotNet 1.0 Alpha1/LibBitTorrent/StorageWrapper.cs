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
        ///  用来检查片断的完整性的函数
        /// </summary>
        private DataFlunkedDelegate dataFlunkedFunction;

        /// <summary>
        /// 访问和设置用来检查片断的完整性的函数
        /// </summary>
        public DataFlunkedDelegate DataFlunkedFunction
        {
            get { return this.dataFlunkedFunction; }
            set { this.dataFlunkedFunction = value; }
        }

        /// <summary>
        /// 在下载完成的时候设置的事件
        /// </summary>
        private FinishedDelegate finishedFunction;

        /// <summary>
        /// 访问和设置在下载完成的时候设置的事件
        /// </summary>
        public FinishedDelegate FinishedFunction
        {
            get { return this.finishedFunction; }
            set { this.finishedFunction = value; }
        }

        /// <summary>
        /// 在下载失败的时候设置的事件
        /// </summary>
        private FailedDelegate failedFunction;

        /// <summary>
        /// 访问和设置在下载失败的时候设置的事件
        /// </summary>
        public FailedDelegate FailedFunction
        {
            get { return this.failedFunction; }
            set { this.failedFunction = value; }
        }

        /// <summary>
        /// 是否已检查片断
        /// </summary>
        private bool checkHashes;

        /// <summary>
        /// 访问和设置是否已检查片断
        /// </summary>
        public bool CheckHashes
        {
            get { return this.checkHashes; }
            set { this.checkHashes = value; }
        }

        /// <summary>
        /// Storage对象
        /// </summary>
        private Storage storage;
        
        /// <summary>
        /// 访问和设置Storage对象
        /// </summary>
        public Storage Storage
        {
            get { return this.storage; }
            set { this.storage = value; }
        }

        /// <summary>
        /// 子片断长度
        /// </summary>
        private int requestSize;

        /// <summary>
        /// 访问和设置子片断长度
        /// </summary>
        public int RequestSize
        {
            get { return this.requestSize; }
            set { this.requestSize = value; }
        }

        /// <summary>
        /// 文件片断摘要信息
        /// </summary>
        private List<byte[]> hashes;

        /// <summary>
        /// 访问文件片断摘要信息
        /// </summary>
        /// <param name="index">文件片断索引</param>
        /// <returns>文件片断摘要信息</returns>
        public byte[] GetHashes(int index)
        {
            return this.hashes[index];
        }

        /// <summary>
        /// 设置文件片断摘要信息
        /// </summary>
        /// <param name="hash"></param>
        /// <param name="index"></param>
        public void SetHashed(byte[] hash, int index)
        {
            hashes[index] = hash;
        }

        /// <summary>
        /// 片断长度
        /// </summary>
        private int pieceLength;

        /// <summary>
        /// 访问和设置片断长度
        /// </summary>
        public int PieceLength
        {
            get { return this.pieceLength; }
            set { this.pieceLength = value; }
        }
        
        /// <summary>
        /// 文件总长度
        /// </summary>
        private long totalLength;

        /// <summary>
        /// 访问和设置文件总长度
        /// </summary>
        public long TotalLength
        {
            get { return this.totalLength; }
            set { this.totalLength = value; }
        }

        /// <summary>
        /// 未下载完的文件大小
        /// </summary>
        private long leftLength;

        /// <summary>
        /// 访问和设置未下载完的文件大小
        /// </summary>
        public long LeftLength
        {
            get { return this.leftLength; }
            set { this.leftLength = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        private int[] numActive;

        /// <summary>
        /// inactiveRequests的值全部被初始化为1，这表示每个片断都需要发送request。
        /// 后面在对磁盘文件检查之后，那些已经获得的片断，在inactiveRequests中对应的是null，表示不需要再为这些片断发送request了。
        /// </summary>
        private List<List<InactiveRequest>> inactiveRequests;
        
        private int totalInactive;

        public int TotalInactive
        {
            get { return this.totalInactive; }
            set { this.totalInactive = value; }
        }

        private bool[] have;

        /// <summary>
        /// 是否已被检查
        /// </summary>
        private bool isChecked;

        /// <summary>
        /// 访问和设置是否已被检查
        /// </summary>
        public bool IsChecked
        {
            get { return this.isChecked; }
            set { this.isChecked = value; }
        }

        private static SHA1Managed shaM;

        static StorageWrapper()
        {
            shaM = new SHA1Managed();
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="storage">storage对象</param>
        /// <param name="requestSize">子片断长度</param>
        /// <param name="hashes">文件片断摘要信息</param>
        /// <param name="pieceLength">片断长度</param>
        /// <param name="finishedFunc">在下载完成的时候设置的事件</param>
        /// <param name="failedFunc">在下载失败的时候设置的事件</param>
        /// <param name="statusFunc"></param>
        /// <param name="flag"></param>
        /// <param name="checkHashes"></param>
        /// <param name="dataFlunkedFunc">用来检查片断的完整性的函数</param>
        public StorageWrapper(Storage storage, int requestSize, List<byte[]> hashes, int pieceLength,FinishedDelegate finishedFunc, 
            FailedDelegate failedFunc, StatusDelegate statusFunc, Flag flag, bool checkHashes, DataFlunkedDelegate dataFlunkedFunc)
        {
            //shaM = new SHA1Managed();
            CheckHashes = checkHashes;
            Storage = storage;
            RequestSize = requestSize;
            this.hashes = hashes;
            PieceLength = pieceLength;
            DataFlunkedFunction = dataFlunkedFunc;
            TotalLength = storage.TotalLength;
            leftLength = this.totalLength;

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
            
            //该片是否检查了完整性
            isChecked = false;

            if (storage.IsExisted)
            {
                if (statusFunc != null)
                    statusFunc("正在检查存在的文件", -1, -1, 0, -1);
                for (i = 0; i < hashes.Count; i++)
                {
                    CheckSingle(i, true);
                    if (flag.IsSet)
                        return;
                    if (statusFunc != null)
                        statusFunc(null, -1, -1, ((double)(i + 1) / (double)hashes.Count), -1);
                }
            }

            else
            {
                for (i = 0; i < hashes.Count; i++)
                    CheckSingle(i, false);
            }

            isChecked = true;
        }

        public bool DoIHaveAnything()
        {
            return leftLength < totalLength;
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
                failedFunction("IO错误:" + ioEx.Message);
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
                failedFunction("IO错误:" + ioEx.Message);
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

        /// <summary>
        /// 如果向某个peer发送的获取“子片断”的请求丢失了，那么调用此函数
        /// </summary>
        /// <param name="index">子片断索引号</param>
        /// <param name="request"></param>
        public void RequestLost(int index, InactiveRequest request)
        {
            requestList.Add(inactiveRequests[index]);
            requestList.Sort();
            numActive[index]--;
            totalInactive++;
        }

        private void CheckSingle(int index, bool check)
        {
            long low = this.pieceLength * index;
            long high = (index == hashes.Count - 1 ? totalLength : low + this.pieceLength);

            int length = (int)(high - low);

            if (check)
            {
                if (!checkHashes || Globals.IsSHA1Equal(this.getSHAHash(storage.Read(low, length)), hashes[index]))
                {
                    have[index] = true;
                    leftLength -= length;
                    if (leftLength == 0)
                    {
                        if (finishedFunction != null)
                            finishedFunction();
                    }
                    return;
                }
            }
            
            List<InactiveRequest> requestList = inactiveRequests[index];

            int i;
            for (i = 0; i + requestSize < length; i += requestSize)
            {
                InactiveRequest item = new InactiveRequest(i, requestSize);
                requestList.Add(item);
                totalInactive++;
            }

            InactiveRequest lastItem = new InactiveRequest(i, length - i);
            requestList.Add(lastItem);
            totalInactive++;
        }

        private byte[] getSHAHash(byte[] piece)
        {
            return shaM.ComputeHash(piece);
        }
    }
}