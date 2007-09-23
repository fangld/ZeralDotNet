using System;

namespace ZeraldotNet.LibBitTorrent
{
    public class RateMeasure
    {
        #region Fields

        private long leftTime;
        private bool gotAnything;
        private bool broke;
        private DateTime start;
        private DateTime last;
        private double remaining;
        private double rate;

        #endregion

        #region Properties

        public long LeftTime
        {
            get { return this.leftTime; }
        }

        #endregion

        #region Constructors

        public RateMeasure(long left)
        {
            this.start = DateTime.MinValue;
            this.last = DateTime.MaxValue;
            this.rate = 0;
            this.remaining = -1;
            this.leftTime = left;
            this.broke = false;
            this.gotAnything = false;
        }

        #endregion

        #region Methdos

        public long GetLeftTime()
        {
            return this.leftTime;
        }

        public void DataCameIn(long amount)
        {
            if (!this.gotAnything)
            {
                this.gotAnything = true;
                start = DateTime.Now.AddSeconds(-2);
                last = start;
                leftTime -= amount;
                return;
            }
            Update(DateTime.Now, amount);
        }

        public void RejectDate(long amount)
        {
            leftTime += amount;
        }

        public double GetTimeLeft()
        {
            if (!gotAnything)
                return -1;
            DateTime now = DateTime.Now;
            if (now.Subtract(last).TotalSeconds > 15)
                Update(now, 0);
            return remaining;
        }

        public void Update(DateTime time, long amount)
        {
            leftTime -= amount;
            try
            {
                rate = ((rate * (last.Subtract(start).TotalSeconds)) + amount) / (time.Subtract(start).TotalSeconds);
                last = time;
                remaining = leftTime / rate;
                if (start < last.AddSeconds(-remaining))
                {
                    start = last.AddSeconds(-remaining);
                }
            }
            catch (DivideByZeroException)
            {
                remaining = -1;
            }

            if (broke && last.Subtract(start).TotalSeconds < 20)
            {
                start = last.AddSeconds(-20);
            }
            if (last.Subtract(start).TotalSeconds > 20)
            {
                broke = true;
            }
        }

        #endregion
    }
}
