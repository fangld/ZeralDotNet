using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace ZeraldotNet.LibBitTorrent.Storages
{
    /// <summary>
    /// 把文件片断进一步切割为子片断，并且为这些子片断发送request消息。在获得子片断后，将数据写入磁盘。
    /// </summary>
    public class StorageWrapper : IStorageWrapper
    {
        #region Fields

        /// <summary>
        ///  用来检查片断的完整性的函数
        /// </summary>
        private DataFlunkedDelegate dataFlunkedFunction;

        /// <summary>
        /// 在下载完成的时候设置的事件
        /// </summary>
        private readonly FinishedDelegate finishedFunction;

        /// <summary>
        /// 在下载失败的时候设置的事件
        /// </summary>
        private readonly FailedDelegate failedFunction;

        /// <summary>
        /// 是否已检查片断
        /// </summary>
        private readonly bool checkHashes;

        /// <summary>
        /// Storage对象
        /// </summary>
        private readonly Storage _storage;

        /// <summary>
        /// 子片断长度
        /// </summary>
        private readonly int requestSize;

        /// <summary>
        /// 文件片断摘要信息
        /// </summary>
        private readonly List<byte[]> hashes;

        /// <summary>
        /// 片断长度
        /// </summary>
        private readonly int pieceLength;

        /// <summary>
        /// 文件总长度
        /// </summary>
        private readonly long totalLength;

        /// <summary>
        /// 未下载完的文件大小
        /// </summary>
        private long leftLength;

        /// <summary>
        /// 已经发出去的请求数
        /// </summary>
        private readonly int[] numActive;

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
        /// 磁盘上是否拥有第index个片断
        /// </summary>
        private readonly bool[] have;

        /// <summary>
        /// 是否已被检查
        /// </summary>
        private bool isChecked;

        /// <summary>
        /// 检验类(SHA1)
        /// </summary>
        private readonly static SHA1Managed shaM;

        #endregion

        #region Properties

        /// <summary>
        /// 访问和设置未下载完的文件大小
        /// </summary>
        public long LeftLength
        {
            get { return this.leftLength; }
            set { this.leftLength = value; }
        }

        #endregion

        #region Constructors

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
        /// <param name="_orginalStorage">storage对象</param>
        /// <param name="requestSize">子片断长度</param>
        /// <param name="hashes">文件片断摘要信息</param>
        /// <param name="pieceLength">片断长度</param>
        /// <param name="finishedFunc">在下载完成的时候设置的事件</param>
        /// <param name="failedFunc">在下载失败的时候设置的事件</param>
        /// <param name="statusFunc">状态函数</param>
        /// <param name="flag"></param>
        /// <param name="checkHashes"></param>
        /// <param name="dataFlunkedFunc">用来检查片断的完整性的函数</param>
        public StorageWrapper(Storage storage, int requestSize, List<byte[]> hashes, int pieceLength,FinishedDelegate finishedFunc, 
            FailedDelegate failedFunc, StatusDelegate statusFunc, Flag flag, bool checkHashes, DataFlunkedDelegate dataFlunkedFunc)
        {
            this.checkHashes = checkHashes;
            this._storage = storage;
            this.requestSize = requestSize;
            this.hashes = hashes;
            this.pieceLength = pieceLength;
            this.dataFlunkedFunction = dataFlunkedFunc;
            this.totalLength = 0;//storage.SumLength;
            this.leftLength = this.totalLength;


            //检验是否需要接收数据的长度符合标准
            CheckTotalLength();            

            this.finishedFunction = finishedFunc;
            this.failedFunction = failedFunc;
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
            if (true)//_storage.IsExisted)
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

        #endregion

        #region Methods

        public long GetLeftLength()
        {
            return this.leftLength;
        }

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
        public void SetHashes(byte[] hash, int index)
        {
            hashes[index] = hash;
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
        /// <param name="piece">子片断的数据</param>
        private void WritePiece(int index, long begin, byte[] piece)
        {
            //调用storage对象写入
            _storage.Write(piece, index * pieceLength + begin);
            
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
        /// <param name="length">子片断的长度</param>
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
        /// <param name="length">子片断的长度</param>
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
            byte[] result = new byte[length];
            _storage.Read(result, low, length);
            return result;
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
            byte[] bytes = new byte[length];
            _storage.Read(bytes, begin, length);
            if (check && (!checkHashes || Globals.IsArrayEqual(getSHAHash(bytes), hashes[index], 20)))
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
        /// <param name="length">片断的长度</param>
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
        /// <param name="length">片断的长度</param>
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
        /// <param name="piece">需要检验的数据</param>
        /// <returns>返回数据的校验和</returns>
        private static byte[] getSHAHash(byte[] piece)
        {
            return shaM.ComputeHash(piece);
        }

        #endregion
    }
}