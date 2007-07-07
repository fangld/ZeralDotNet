using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.Messages;
using ZeraldotNet.TestLibBitTorrent.DummyMessages;

namespace ZeraldotNet.TestLibBitTorrent.TestConnecter
{
    /// <summary>
    /// 测试的连接管理类
    /// </summary>
    public class DummyConnecter
    {
        #region Private Field

        /// <summary>
        /// 测试的下载器
        /// </summary>
        private DummyDownloader downloader;

        /// <summary>
        /// 是否超出最大上传速率
        /// </summary>
        private bool rateCapped;

        /// <summary>
        /// 
        /// </summary>
        private SchedulerDelegate scheduleFunction;

        /// <summary>
        /// 用于保存封装连接类与连接类的字典
        /// </summary>
        private Dictionary<DummyEncryptedConnection, DummyConnection> connectionDictionary;

        /// <summary>
        /// 
        /// </summary>
        private PendingDelegate isEverythingPending;

        /// <summary>
        /// 下载文件的片断数量
        /// </summary>
        private int piecesNumber;

        /// <summary>
        /// 测试的阻塞器
        /// </summary>
        private DummyChoker choker;

        /// <summary>
        /// 参数类
        /// </summary>
        private Measure totalUp;

        /// <summary>
        /// 最大上传速率
        /// </summary>
        private int maxUploadRate;

        /// <summary>
        /// 是否下载完成
        /// </summary>
        private bool endgame;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问是否超出最大上传速率
        /// </summary>
        public bool RateCapped
        {
            get { return this.rateCapped; }
        }

        /// <summary>
        /// 访问连接的数量
        /// </summary>
        public int ConnectionsCount
        {
            get { return connectionDictionary.Count; }
        }

        /// <summary>
        /// 访问下载文件的片断数量
        /// </summary>
        public int PiecesNumber
        {
            get { return this.piecesNumber; }
        }

        /// <summary>
        /// 访问连接类的集合
        /// </summary>
        public ICollection<DummyConnection> Connections
        {
            get { return this.connectionDictionary.Values; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="downloader"></param>
        /// <param name="choker"></param>
        /// <param name="piecesNumber"></param>
        /// <param name="isEverythingPending"></param>
        /// <param name="totalUp"></param>
        /// <param name="maxUploadRate"></param>
        /// <param name="scheduleFunction"></param>
        public DummyConnecter(DummyDownloader downloader, DummyChoker choker, int piecesNumber, PendingDelegate isEverythingPending,
            Measure totalUp, int maxUploadRate, SchedulerDelegate scheduleFunction)
        {
            this.downloader = downloader;
            this.choker = choker;
            this.piecesNumber = piecesNumber;
            this.isEverythingPending = isEverythingPending;
            this.maxUploadRate = maxUploadRate;
            this.scheduleFunction = scheduleFunction;
            this.totalUp = totalUp;
            this.rateCapped = false;
            this.connectionDictionary = new Dictionary<DummyEncryptedConnection, DummyConnection>();
            this.endgame = false;
            this.CheckEndgame();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 更新上传速率
        /// </summary>
        /// <param name="amount">上传了的字节流数量</param>
        public void UpdateUploadRate(int amount)
        {
            totalUp.UpdateRate(amount);
            if (maxUploadRate > 0 && totalUp.NonUpdatedRate > maxUploadRate)
            {
                rateCapped = true;
                scheduleFunction(new TaskDelegate(Uncap), totalUp.TimeUntilRate(maxUploadRate), "Update Upload Rate");
            }
        }

        /// <summary>
        /// 降低上传的速率
        /// </summary>
        public void Uncap()
        {
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="encryptedConnection">待建立的封装连接类</param>
        public void MakeConnection(DummyEncryptedConnection encryptedConnection)
        {
            DummyConnection connection = new DummyConnection(encryptedConnection, this);
            connectionDictionary[encryptedConnection] = connection;
            connection.Upload = MakeUpload(connection);
            connection.Download = downloader.MakeDownload(connection);
            choker.MakeConnection(connection);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="encryptedConnection">待断开的封装连接类</param>
        public void LoseConnection(DummyEncryptedConnection encryptedConnection)
        {
            DummyConnection connection = connectionDictionary[encryptedConnection];
            DummyDownload singleDownload = connection.Download;
            connectionDictionary.Remove(encryptedConnection);
            singleDownload.Disconnect();
            choker.LoseConnection(connection);
        }

        /// <summary>
        /// 清除缓冲区中的数据，并将其写入到相应节点上
        /// </summary>
        /// <param name="encryptedConnection">待写入的封装连接类</param>
        public void FlushConnection(DummyEncryptedConnection encryptedConnection)
        {
            connectionDictionary[encryptedConnection].Upload.Flush();
        }

        /// <summary>
        /// 产生上传器
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private DummyUpload MakeUpload(DummyConnection connection)
        {
            return TestConnecter.MakeUpload(connection);
        }

        /// <summary>
        /// 检查是否下载完成
        /// </summary>
        public void CheckEndgame()
        {
            if (!endgame && isEverythingPending())
            {
                endgame = true;
            }
        }

        /// <summary>
        /// 处理从节点上获取的网络信息
        /// </summary>
        /// <param name="connection">获取网络信息的封装连接类</param>
        /// <param name="message">获取的网络信息字节流</param>
        public void GetMessage(DummyEncryptedConnection encryptedConnection, byte[] message)
        {
            DummyMessageDecoder.Decode(message, encryptedConnection, connectionDictionary[encryptedConnection], this);
        }

        #endregion
    }
}
