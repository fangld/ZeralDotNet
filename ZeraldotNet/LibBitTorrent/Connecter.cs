using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Messages;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate void StatusDelegate(string message, double downloadRate, double uploadRate, double fractionDone, double timeEstimate);
    public delegate void ErrorDelegate(string message);

    /// <summary>
    /// 连接管理类
    /// </summary>
    public class Connecter
    {
        #region Private Field

        /// <summary>
        /// 下载器
        /// </summary>
        private IDownloader downloader;

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
        private Dictionary<EncryptedConnection, Connection> connectionDictionary;

        /// <summary>
        /// 
        /// </summary>
        private PendingDelegate isEverythingPending;

        /// <summary>
        /// 下载文件的片断数量
        /// </summary>
        private int piecesNumber;

        /// <summary>
        /// 阻塞器
        /// </summary>
        private Choker choker;

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
        /// 访问下载文件的片断数量
        /// </summary>
        public int PiecesNumber
        {
            get { return this.piecesNumber; }
        }

        /// <summary>
        /// 访问连接的数量
        /// </summary>
        public int ConnectionsCount
        {
            get { return connectionDictionary.Count; }
        }

        /// <summary>
        /// 访问连接类的集合
        /// </summary>
        public ICollection<Connection> Connections
        {
            get { return this.connectionDictionary.Values; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="downloader">下载器</param>
        /// <param name="choker">阻塞器</param>
        /// <param name="piecesNumber">下载文件的片断数量</param>
        /// <param name="isEverythingPending"></param>
        /// <param name="totalUp">参数类</param>
        /// <param name="maxUploadRate">最大上传速率</param>
        /// <param name="scheduleFunction"></param>
        public Connecter(IDownloader downloader, Choker choker, int piecesNumber, PendingDelegate isEverythingPending,
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
            this.connectionDictionary = new Dictionary<EncryptedConnection, Connection>();
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

            //如果现在上传速率超国预定的最大上传速率，则进行限制
            if (maxUploadRate > 0 && totalUp.NonUpdatedRate > maxUploadRate)
            {
                rateCapped = true;
                scheduleFunction(new TaskDelegate(UnCap), totalUp.TimeUntilRate(maxUploadRate), "Update Upload Rate");
            }
        }

        /// <summary>
        /// 降低上传的速率
        /// </summary>
        public void UnCap()
        {
            rateCapped = false;
            Upload minRateUpload;
            double minRate, rate;

            //选择上传速率最小的节点进行上传
            while (!rateCapped)
            {
                //如果连接数量为0，则跳出循环
                if (connectionDictionary.Count == 0)
                {
                    break;
                }

                minRateUpload = connectionDictionary.ElementAt(0).Value.Upload;
                minRate = minRateUpload.Rate;

                //查找上传速率最小的节点
                foreach (Connection item in connectionDictionary.Values)
                {
                    //如果该节点没有被阻塞，而且缓冲区中还有数据，并且其封装连接类已经发送完数据
                    if (!item.Upload.Choked && item.Upload.HasQueries && item.EncryptedConnection.IsFlushed)
                    {
                        //获取节点的上传速率
                        rate = item.Upload.Rate;
                        
                        //如果该节点的上传速率小于最小上传速率，则更新最小
                        if (rate < minRate)
                        {
                            minRateUpload = item.Upload;
                            minRate = rate;
                        }
                    }
                }

                //调用节点的Flush函数
                minRateUpload.Flush();

                //如果上传速率大于最大上传速率，则跳出循环
                if (totalUp.NonUpdatedRate > maxUploadRate)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 建立连接
        /// </summary>
        /// <param name="encryptedConnection">待建立的封装连接类</param>
        public void MakeConnection(EncryptedConnection encryptedConnection)
        {
            Connection connection = new Connection(encryptedConnection, this);
            connectionDictionary[encryptedConnection] = connection;
            connection.Upload = MakeUpload(connection);
            connection.Download = downloader.MakeDownload(connection);
            choker.MakeConnection(connection);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="encryptedConnection">待断开的封装连接类</param>
        public void LoseConnection(EncryptedConnection encryptedConnection)
        {
            Connection connection = connectionDictionary[encryptedConnection];
            ISingleDownload singleDownload = connection.Download;
            connectionDictionary.Remove(encryptedConnection);
            singleDownload.Disconnect();
            choker.LoseConnection(connection);
        }

        /// <summary>
        /// 清除缓冲区中的数据，并将其写入到相应节点上
        /// </summary>
        /// <param name="encryptedConnection">待写入的封装连接类</param>
        public void FlushConnection(EncryptedConnection encryptedConnection)
        {
            connectionDictionary[encryptedConnection].Upload.Flush();
        }

        /// <summary>
        /// 产生上传器
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        private Upload MakeUpload(Connection connection)
        {
            return new Upload(connection, Download.Choker, Download.StorageWrapper, Download.Parameters.MaxSliceLength,
                       Download.Parameters.MaxRatePeriod, Download.Parameters.UploadRateFudge);
        }

        /// <summary>
        /// 处理从节点上获取的网络信息
        /// </summary>
        /// <param name="connection">获取网络信息的封装连接类</param>
        /// <param name="message">获取的网络信息字节流</param>
        public void GetMessage(EncryptedConnection encryptedConnection, byte[] message)
        {
            MessageDecoder.Decode(message, encryptedConnection, connectionDictionary[encryptedConnection], this);
        }

        /// <summary>
        /// 检查是否下载完成
        /// </summary>
        public void CheckEndgame()
        {
            //当所有文件已经下载完，则转入到Endgame下载模式
            if (!endgame && isEverythingPending())
            {
                endgame = true;
                downloader = new EndgameDownloader(downloader);
            }
        }

        #endregion
    }
}
