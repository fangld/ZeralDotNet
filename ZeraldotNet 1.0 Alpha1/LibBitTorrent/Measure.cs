using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 下载时参数类
    /// </summary>
    public class Measure
    {
        /// <summary>
        /// 最大更新速率周期
        /// </summary>
        private double maxRatePeriod;

        /// <summary>
        /// 访问和设置最大更新速率周期
        /// </summary>
        public double MaxRatePeriod
        {
            get { return this.maxRatePeriod; }
            set { this.maxRatePeriod = value; }
        }

        /// <summary>
        /// 计算速率的起始时间
        /// </summary>
        private DateTime rateSince;

        /// <summary>
        /// 计算速率的结束时间
        /// </summary>
        private DateTime rateLast;

        /// <summary>
        /// 上传或者下载速率
        /// </summary>
        private double rate;

        /// <summary>
        /// 访问更新过的速率
        /// </summary>
        public double UpdatedRate
        {
            get 
            {
                this.UpdateRate(0);
                return this.rate; 
            }
        }

        /// <summary>
        /// 访问没有更新的速率
        /// </summary>
        public double NonUpdatedRate
        {
            get { return this.rate; }
        }

        /// <summary>
        /// 这次下载的数据长度
        /// </summary>
        private long totalLength;

        /// <summary>
        /// 访问这次下载的数据长度
        /// </summary>
        public long TotalLength
        {
            get { return this.totalLength; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxRatePeriod"></param>
        public Measure(double maxRatePeriod) 
            : this(maxRatePeriod, -1) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="maxRatePeriod"></param>
        /// <param name="fudge"></param>
        public Measure(double maxRatePeriod, double fudge)
        {
            MaxRatePeriod = maxRatePeriod;
            this.rateSince = DateTime.Now.AddSeconds(-fudge);
            this.rateLast = rateSince;
            this.rate = 0.0;
            this.totalLength = 0L;
        }

        /// <summary>
        /// 更新速率
        /// </summary>
        /// <param name="amount">增加的长度</param>
        public void UpdateRate(long amount)
        {
            totalLength += amount;
            DateTime now = DateTime.Now;

            //刚下载或者上传的长度加上这个周期内下载的长度
            double length = rate * rateLast.Subtract(rateSince).TotalSeconds + amount;

            //刚这个周期内所用的时间
            double time = now.Subtract(rateSince).TotalSeconds;

            //更新速率
            rate = length / time;
            rateLast = now;

            //如果到了更新周期，自动更新刚保存速率的起始时间
            if (rateSince < now.AddSeconds(-maxRatePeriod))
            {
                rateSince = now.AddSeconds(-maxRatePeriod);
            }
        }

        /// <summary>
        /// 返回新速率比原速率所增加的时间
        /// </summary>
        /// <param name="newRate">新的速率</param>
        /// <returns>返回新速率比原速率所增加的时间</returns>
        public double TimeUntilRate(double newRate)
        {
            //如果原速率比新速率小，则返回0
            if (rate <= newRate)
            {
                return 0;
            }

            //这个周期的时间段
            double span = DateTime.Now.Subtract(rateSince).TotalSeconds;

            //返回新速率比原速率所增加的时间
            return ((rate * span) / newRate) - span;
        }
    }
}
