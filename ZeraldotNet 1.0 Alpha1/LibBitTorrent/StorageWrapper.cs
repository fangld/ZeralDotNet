using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 把文件片断进一步切割为子片断，并且为这些子片断发送request消息。在获得子片断后，将数据写入磁盘。
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
        /// 已经发出去的请求数
        /// </summary>
        private int[] numActive;

        /// <summary>
        /// inactiveRequests的值全部被初始化为1，这表示每个片断都需要发送request。
        /// 后面在对磁盘文件检查之后，那些已经获得的片断，在inactiveRequests中对应的是null，表示不需要再为这些片断发送request了。
        /// </summary>
        private List<InactiveRequest>[] inactiveRequests;
        
        /// <summary>
        /// 所有没有发出request的子片断总数量
        /// </summary>
        private int totalInactive;

        /// <summary>
        /// 访问和设置所有没有发出request的子片断总数量
        /// </summary>
        public int TotalInactive
        {
            get { return this.totalInactive; }
            set { this.totalInactive = value; }
        }

        /// <summary>
        /// 磁盘上是否拥有第index个片断
        /// </summary>
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

        /// <summary>
        /// 检验类(SHA1)
        /// </summary>
        private static SHA1Managed shaM;

        /// <summary>
        /// 静态构造函数
        /// </summary>
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
            CheckHashes = checkHashes;
            Storage = storage;
            RequestSize = requestSize;
            this.hashes = hashes;
            PieceLength = pieceLength;
            DataFlunkedFunction = dataFlunkedFunc;
            TotalLength = storage.TotalLength;
            leftLength = this.totalLength;


            //检验是否需要接收数据的长度符合标准
            CheckTotalLength();            

            FinishedFunction = finishedFunc;
            FailedFunction = failedFunc;
            numActive = new int[hashes.Count];

            int hashesLength = hashes.Count;

            //初始化inactiveRequests链表
            InitialInactiveRequests(hashesLength);

            have = new bool[hashesLength];

            //如果校验和的长度为0，则结束
            if (hashesLength == 0)
            {
                if (finishedFunc != null)
                    finishedFunc();
                return;
            }
            
            //该片是否检查了完整性
            isChecked = false;

            int i;
            //如果磁盘上已经存在下载的文件，则校验磁盘上的文件
            if (storage.IsExisted)
            {
                //显示检查文件信息
                if (statusFunc != null)
                    statusFunc("正在检查存在的文件", -1, -1, 0, -1);

                for (i = 0; i < hashesLength; i++)
                {
                    CheckSingle(i, true);
                    if (flag.IsSet)
                        return;

                    //显示检查文件信息
                    if (statusFunc != null)
                        statusFunc(string.Empty, -1, -1, ((double)(i + 1) / (double)hashesLength), -1);
                }
            }

            //如果磁盘上没有文件，则新建文件
            else
            {
                for (i = 0; i < hashesLength; i++)
                {
                    CheckSingle(i, false);
                }
            }

            isChecked = true;
        }

        /// <summary>
        /// 初始化inactiveRequests链表
        /// </summary>
        /// <param name="hashesLength">链表的长度</param>
        private void InitialInactiveRequests(int hashesLength)
        {
            inactiveRequests = new List<InactiveRequest>[hashesLength];
            int index;

            
            //for (index = 0; index < numActive.Length; index++)
            //{
            //    numActive[index] = 0;
            //    inactiveRequests[index] = new List<InactiveRequest>();
            //}

             //以下代码是并行处理版本，比上面的快
            index = 0;
            while (index < numActive.Length - 1)
            {
                numActive[index] = 0;
                inactiveRequests[index++] = new List<InactiveRequest>();

                numActive[index] = 0;
                inactiveRequests[index++] = new List<InactiveRequest>();
            }

            if (index == hashesLength - 1)
            {
                numActive[index] = 0;
                inactiveRequests[index] = new List<InactiveRequest>();
            }

            totalInactive = 0;
        }

        /// <summary>
        /// 检验是否需要接收数据的长度符合标准
        /// </summary>
        private void CheckTotalLength()
        {
            long hashLength = this.pieceLength * this.hashes.Count;

            //如果所下载的文件长度比校验和的文件长度要大
            if (this.totalLength > hashLength)
                throw new BitTorrentException("从Tracker服务器收到错误数据，文件长度太大");

            //如果所下载的文件长度比校验和的文件长度要小
            if (this.totalLength <= hashLength - this.pieceLength)
                throw new BitTorrentException("从Tracker服务器收到错误数据，文件长度太小");
        }

        /// <summary>
        /// 判断是否已经获得了一些文件片断
        /// </summary>
        /// <returns>返回是否已经获得了一些文件片断</returns>
        public bool DoIHaveAnything()
        {
            return leftLength < totalLength;
        }

        /// <summary>
        /// 是否推迟
        /// </summary>
        /// <returns>返回是否推迟</returns>
        public bool IsEverythingPending()
        {
            return totalInactive == 0;
        }

        /// <summary>
        /// 获取拥有片断列表
        /// </summary>
        /// <returns>返回拥有片断列表</returns>
        public bool[] GetHaveList()
        {
            return have;
        }

        /// <summary>
        /// 是否拥有第index个片断
        /// </summary>
        /// <param name="index">片断的索引号</param>
        /// <returns>返回是否拥有第index个片断</returns>
        public bool DoIHave(int index)
        {
            return have[index];
        }

        /// <summary>
        /// 指如果指定的片断还有request没有发出，那么返回true，否则返回 false。
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <returns>如果指定的片断还有request没有发出，那么返回true，否则返回 false。</returns>
        public bool DoIHaveRequests(int index)
        {
            return inactiveRequests[index].Count != 0;
        }

        /// <summary>
        /// 为指定片断创建一个request 消息，返回的是一个二元组，例如（32k, 16k），表示“子片断”的起始位置是32k ，大小是16k。
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <returns>返回第index个片断的起始位置最小的请求信息</returns>
        public InactiveRequest NewRequest(int index)
        {
            //numActive[index]记录了已经为第index个片断发出的request数量。
            numActive[index]++;

            //totalInactive记录了尚没有发出request的子片断总的大小。
            totalInactive--;

            //从 inactiveRequest中移出最小的那个request（也就是起始位置最小）。
            List<InactiveRequest> requests = inactiveRequests[index];            
            InactiveRequest minRequest = InactiveRequest.Min(requests);
            requests.Remove(minRequest);

            return minRequest;
        }

        /// <summary>
        /// 从磁盘上写入相应的片断的子片断
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断在片断中的起始位置<</param>
        /// <param name="index">子片断的长度</param>
        public void PieceCameIn(int index, long begin, byte[] piece)
        {
            try
            {
                WritePiece(index, begin, piece);
            }

            //捕获IO访问错误
            catch (IOException ioEx)
            {
                failedFunction("IO错误:" + ioEx.Message);
            }
        }

        /// <summary>
        /// 从磁盘上写入相应的片断的子片断
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断在片断中的起始位置</param>
        /// <param name="index">子片断的长度</param>
        private void WritePiece(int index, long begin, byte[] piece)
        {
            //调用storage对象写入
            storage.Write(index * pieceLength + begin, piece);
            
            //减少第index个片断的请求数
            numActive[index]--;
          
            //如果片断已经下载完毕，则检查是否正确
            if (inactiveRequests[index].Count == 0 && numActive[index] == 0)
            {
                CheckSingle(index, true);
            }
        }

        /// <summary>
        /// 从磁盘上读取需要的片断的子片断
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断在片断中的起始位置</param>
        /// <param name="lengthBytes">子片断的长度</param>
        /// <returns></returns>
        public byte[] GetPiece(int index, int begin, int length)
        {
            try
            {
                return ReadPiece(index, begin, length);
            }
            
            //捕获IO访问错误
            catch (IOException ioEx)
            {
                failedFunction("IO错误:" + ioEx.Message);
                return null;
            }
        }

        /// <summary>
        /// 从磁盘上读取需要的片断的子片断
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断在片断中的起始位置</param>
        /// <param name="lengthBytes">子片断的长度</param>
        /// <returns></returns>
        private byte[] ReadPiece(int index, int begin, int length)
        {
            //如果没有该子片断，则返回空
            if (!have[index])
            {
                return null;
            }

            //如果子片断的长度超过总文件的长度，则返回空
            long low = pieceLength * index + begin;
            if (low + length > Math.Min(totalLength, pieceLength * (index + 1)))
            {
                return null;
            }

            //返回从磁盘中读取的子片断
            return storage.Read(low, length);
        }

        /// <summary>
        /// 如果向某个peer发送的获取“子片断”的请求丢失了，那么调用此函数
        /// </summary>
        /// <param name="index">子片断索引号</param>
        /// <param name="request">请求子片断的信息</param>
        public void RequestLost(int index, InactiveRequest request)
        {
            inactiveRequests[index].Add(request);
            inactiveRequests[index].Sort();
            numActive[index]--;
            totalInactive++;
        }

        /// <summary>
        /// 检查该第index个片断是否完整
        /// </summary>
        /// <param name="index">片断的索引号</param>
        /// <param name="check"></param>
        private void CheckSingle(int index, bool check)
        {
            long begin = this.pieceLength * index;
            //如果第index个片断是最后一个片断，片断的结束位置则是文件的总长度
            long end = (index == hashes.Count - 1) ? totalLength : begin + this.pieceLength;
            int length = (int)(end - begin);

            //如果需要检验片断的完整性
            if (check && (!checkHashes || Globals.IsSHA1Equal(this.getSHAHash(storage.Read(begin, length)), hashes[index])))
            {
                //如果片断完整或者已经被检验过是完整
                FinishPiece(index, length);
            }

            //否则，产生第index个片断的所有请求信息
            else
            {
                MakeInactiveRequest(index, length);
            }

        }

        /// <summary>
        /// 产生第index个片断的所有请求信息
        /// </summary>
        /// <param name="index">片断的索引号</param>
        /// <param name="lengthBytes">片断的长度</param>
        private void MakeInactiveRequest(int index, int length)
        {
            List<InactiveRequest> requestList = inactiveRequests[index];

            //子片断在片断的偏移位置
            int offset;
            int notLastLength = length - requestSize;

            //添加每个子片断的请求信息
            for (offset = 0; offset < notLastLength; offset += requestSize)
            {
                InactiveRequest request = new InactiveRequest(offset, requestSize);
                requestList.Add(request);
                totalInactive++;
            }

            //添加最后一个子片断的请求信息
            InactiveRequest lastRequest = new InactiveRequest(offset, length - offset);
            requestList.Add(lastRequest);
            totalInactive++;
        }

        /// <summary>
        /// 当片断检验为正确时的操作
        /// </summary>
        /// <param name="index">片断的索引号</param>
        /// <param name="lengthBytes">片断的长度</param>
        private void FinishPiece(int index, int length)
        {
            //表示已经拥有第index个片断
            have[index] = true;
            leftLength -= length;
            
            //如果剩余长度为0，则结束下载
            if (leftLength == 0)
            {
                if (finishedFunction != null)
                    finishedFunction();
            }
        }

        /// <summary>
        /// 检验函数
        /// </summary>
        /// <param name="index">需要检验的数据</param>
        /// <returns>返回数据的校验和</returns>
        private byte[] getSHAHash(byte[] piece)
        {
            return shaM.ComputeHash(piece);
        }
    }
}