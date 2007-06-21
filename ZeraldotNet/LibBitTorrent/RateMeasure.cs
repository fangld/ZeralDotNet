using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate void DataDelegate(long amount);

    public class RateMeasure
    {
        private long left;

        public long Left
        {
            get { return this.left; }
        }

        private bool gotAnything;
        private bool broke;
        private DateTime start;
        private DateTime last;
        private double remaining;
        private double rate;

        public RateMeasure(long left)
        {
            this.start = DateTime.MinValue;
            this.last = DateTime.MaxValue;
            this.rate = 0;
            this.remaining = -1;
            this.left = left;
            this.broke = false;
            this.gotAnything = false;
        }

        public void DataCameIn(long amount)
        {
            if (!this.gotAnything)
            {
                this.gotAnything = true;
                start = DateTime.Now.AddSeconds(-2);
                last = start;
                left -= amount;
                return;
            }
            Update(DateTime.Now, amount);
        }

        public void RejectDate(long amount)
        {
            left += amount;
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
            left -= amount;
            try
            {
                rate = ((rate * (last.Subtract(start).TotalSeconds)) + amount) / (time.Subtract(start).TotalSeconds);
                last = time;
                remaining = left / rate;
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
    }
}
