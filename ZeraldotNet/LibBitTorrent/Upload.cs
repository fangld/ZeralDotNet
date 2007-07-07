using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Summary description for Uploader.
    /// </summary>
    public class Upload
    {
        #region Private Field

        /// <summary>
        /// 连接类
        /// </summary>
        private Connection connection;

        /// <summary>
        /// 阻塞类
        /// </summary>
        private Choker choker;

        /// <summary>
        /// 存储类
        /// </summary>
        private StorageWrapper storageWrapper;

        /// <summary>
        /// 最大子片断的长度
        /// </summary>
        private int maxSliceLength;

        /// <summary>
        /// 最大的参数更新周期
        /// </summary>
        private double maxRatePeriod;

        /// <summary>
        /// 是否已经阻塞
        /// </summary>
        private bool choked;

        /// <summary>
        /// 是否已经感兴趣
        /// </summary>
        private bool interested;

        /// <summary>
        /// 激活的请求
        /// </summary>
        private LinkedList<ActiveRequest> buffer;

        /// <summary>
        /// 上传的参数
        /// </summary>
        private Measure measure;

        #endregion

        #region Public Properties

        /// <summary>
        /// 访问是否已经阻塞
        /// </summary>
        public bool Choked
        {
            get { return this.choked; }
        }

        /// <summary>
        /// 访问是否已经感兴趣
        /// </summary>
        public bool Interested
        {
            get { return this.interested; }
        }

        /// <summary>
        /// 访问和设置上传的参数
        /// </summary>
        public Measure Measure
        {
            get { return this.measure; }
            set { this.measure = value; }
        }

        /// <summary>
        /// 访问上传的速率
        /// </summary>
        public double Rate
        {
            get { return this.measure.UpdatedRate; }
        }

        /// <summary>
        /// 返回缓冲区的数据是否大于0
        /// </summary>
        public bool HasQueries
        {
            get { return buffer.Count > 0; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connection">连接类</param>
        /// <param name="choker">阻塞类</param>
        /// <param name="storageWrapper">磁盘封装类</param>
        /// <param name="maxSliceLength">最大子片断长度</param>
        /// <param name="maxRatePeriod">最大速率更新时间</param>
        /// <param name="fudge"></param>
        public Upload(Connection connection, Choker choker, StorageWrapper storageWrapper, int maxSliceLength,
            double maxRatePeriod, double fudge)
        {
            this.connection = connection;
            this.choker = choker;
            this.storageWrapper = storageWrapper;
            this.maxSliceLength = maxSliceLength;
            this.maxRatePeriod = maxRatePeriod;
            this.choked = true;
            this.interested = false;
            this.buffer = new LinkedList<ActiveRequest>();
            this.measure = new Measure(maxRatePeriod, fudge);

            //如果已经获取的某些片断，则向所有节点发送BitField信息
            if (storageWrapper.DoIHaveAnything())
            {
                connection.SendBitField(storageWrapper.GetHaveList());
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 收到Not Interested网络信息
        /// </summary>
        public void GetNotInterested()
        {
            //如果该节点是Interested节点
            if (interested)
            {
                interested = false;
                buffer.Clear();
                choker.NotInterested(connection);
            }
        }

        /// <summary>
        /// 收到Interested网络信息
        /// </summary>
        public void GetInterested()
        {
            //如果该节点是Not Interested节点
            if (!interested)
            {
                interested = true;
                choker.Interested(connection);
            }
        }

        /// <summary>
        /// 收到Request网络信息
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">片断起始位置</param>
        /// <param name="length">片断长度</param>
        public void GetRequest(int index, int begin, int length)
        {
            //如果该节点是not interested节点或者请求的子片断长度大于最大子片断长度，则关闭连接
            if (!interested || length > maxSliceLength)
            {
                connection.Close();
                return;
            }

            //如果该节点没有被阻塞，则添加请求信息到缓冲区
            if (!choked)
            {
                buffer.AddLast(new ActiveRequest(index, begin, length));
                Flush();
            }
        }

        /// <summary>
        /// 收到Cancel网络信息
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">片断起始位置</param>
        /// <param name="length">片断长度</param>
        public void GetCancel(int index, int begin, int length)
        {
            buffer.Remove(new ActiveRequest(index, begin, length));
        }

        /// <summary>
        /// choke该节点
        /// </summary>
        public void Choke()
        {
            //如果该节点没有被choke，则choke该节点
            if (!choked)
            {
                choked = true;
                buffer.Clear();
                connection.SendChoke();
            }
        }

        /// <summary>
        /// unchoke该节点
        /// </summary>
        public void Unchoke()
        {
            //如果该节点被choke，则unchoke该节点
            if (choked)
            {
                choked = false;
                connection.SendUnchoke();
            }
        }

        /// <summary>
        /// 清除buffer中的所有数据，并且将其发送到相应节点。
        /// </summary>
        public void Flush()
        {
            byte[] piece;
            ActiveRequest request;

            while (buffer.Count > 0 && connection.IsFlushed)
            {
                request = buffer.First.Value;
                buffer.RemoveFirst();
                
                //从磁盘上获取对应request的字节流
                piece = storageWrapper.GetPiece(request.Index, request.Begin, request.Length);

                //如果获取的字节流为空，则关闭连接
                if (piece == null)
                {
                    connection.Close();
                    return;
                }

                //更新参数类的上传速率
                measure.UpdateRate(piece.Length);

                //发送字节流到相应节点
                connection.SendPiece(request.Index, request.Begin, piece);
            }
        }

        #endregion
    }
}
