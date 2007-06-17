using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Summary description for Uploader.
    /// </summary>
    public class Upload
    {
        /// <summary>
        /// 连接类
        /// </summary>
        private Connection connection;

        /// <summary>
        /// 设置连接类
        /// </summary>
        public Connection Connection
        {
            set { this.connection = value; }
        }

        /// <summary>
        /// 阻塞类
        /// </summary>
        private Choker choker;

        /// <summary>
        /// 设置阻塞类
        /// </summary>
        public Choker Choker
        {
            set { this.choker = value; }
        }

        /// <summary>
        /// 存储类
        /// </summary>
        private StorageWrapper storageWrapper;

        /// <summary>
        /// 设置存储类
        /// </summary>
        public StorageWrapper StorageWrapper
        {
            set { this.storageWrapper = value; }
        }

        /// <summary>
        /// 最大子片断的长度
        /// </summary>
        private int maxSliceLength;

        /// <summary>
        /// 设置最大子片断的长度
        /// </summary>
        public int MaxSliceLength
        {
            set { this.maxSliceLength = value; }
        }

        /// <summary>
        /// 最大的参数更新周期
        /// </summary>
        private double maxRatePeriod;

        /// <summary>
        /// 设置最大的参数更新周期
        /// </summary>
        public double MaxRatePeriod
        {
            set { this.maxRatePeriod = value; }
        }

        /// <summary>
        /// 是否已经阻塞
        /// </summary>
        private bool choked;

        /// <summary>
        /// 访问是否已经阻塞
        /// </summary>
        public bool Choked
        {
            get { return this.choked; }
        }

        /// <summary>
        /// 是否已经感兴趣
        /// </summary>
        private bool interested;

        /// <summary>
        /// 访问是否已经感兴趣
        /// </summary>
        public bool Interested
        {
            get { return this.interested; }
        }

        /// <summary>
        /// 激活的请求
        /// </summary>
        private List<ActiveRequest> buffer;

        /// <summary>
        /// 上传的参数
        /// </summary>
        private Measure measure;

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
        /// 构造函数
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="choker"></param>
        /// <param name="storageWrapper"></param>
        /// <param name="maxSliceLength"></param>
        /// <param name="maxRatePeriod"></param>
        /// <param name="fudge"></param>
        public Upload(Connection conn, Choker choker, StorageWrapper storageWrapper, int maxSliceLength,
            double maxRatePeriod, double fudge)
        {
            this.Connection = conn;
            this.Choker = choker;
            this.StorageWrapper = storageWrapper;
            this.MaxRatePeriod = maxRatePeriod;
            this.choked = true;
            this.interested = false;
            this.buffer = new List<ActiveRequest>();
            this.measure = new Measure(maxRatePeriod, fudge);

            if (storageWrapper.DoIHaveAnything())
            {
                conn.SendBitField(storageWrapper.GetHaveList());
            }
        }

        public void GotNotInterested()
        {
            if (interested)
            {
                interested = false;
                buffer.Clear();
                choker.NotInterested(connection);
            }
        }

        public void GotInterested()
        {
            if (!interested)
            {
                interested = true;
                choker.Interested(connection);
            }
        }

        public void Flush()
        {
            while (buffer.Count > 0 && connection.IsFlushed())
            {
                ActiveRequest request = buffer[0];
                buffer.RemoveAt(0);
                byte[] piece = storageWrapper.GetPiece(request.Index, request.Begin, request.Length);
                if (piece == null)
                {
                    connection.Close();
                    return;
                }
                measure.UpdateRate(piece.Length);
                connection.SendPiece(request.Index, request.Begin, piece);
            }            
        }

        public void GotRequest(int index, int begin, int length)
        {
            if (!interested || length > maxSliceLength)
            {
                connection.Close();
                return;
            }

            if (!this.choked)
            {
                buffer.Add(new ActiveRequest(index, begin, length));
                Flush();
            }
        }

        /// <summary>
        /// 阻塞节点连接
        /// </summary>
        public void Choke()
        {
            if (!choked)
            {
                choked = true;
                buffer.Clear();
                connection.SendChoke();
            }
        }

        /// <summary>
        /// 解除阻塞节点连接
        /// </summary>
        public void Unchoke()
        {
            if (choked)
            {
                choked = false;
                connection.SendUnchoke();
            }
        }

        public bool HasQueries()
        {
            return buffer.Count > 0;
        }
    }
}
