using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Summary description for Measure.
    /// </summary>
    public class Measure
    {
        private double maxRatePeriod;

        public double MaxRatePeriod
        {
            get { return this.maxRatePeriod; }
            set { this.maxRatePeriod = value; }
        }

        private DateTime rateSince;

        private DateTime last;

        private double rate;

        public double UpdatedRate
        {
            get 
            {
                this.UpdateRate(0);
                return this.rate; 
            }
        }

        public double NonUpdateRate
        {
            get { return this.rate; }
        }

        private long total;

        public long Total
        {
            get { return this.total; }
        }

        public Measure(double maxRatePeriod) 
            : this(maxRatePeriod, -1) { }


        public Measure(double maxRatePeriod, double fudge)
        {
            MaxRatePeriod = maxRatePeriod;
            this.rateSince = DateTime.Now.AddSeconds(-fudge);
            this.last = rateSince;
            this.rate = 0.0;
            this.total = 0L;
        }

        public void UpdateRate(long amount)
        {
            total += amount;
            DateTime now = DateTime.Now;
            rate = (rate * last.Subtract(rateSince).TotalSeconds + amount) / now.Subtract(rateSince).TotalSeconds;
            last = now;
            if (rateSince < now.AddSeconds(-maxRatePeriod))
            {
                rateSince = now.AddSeconds(-maxRatePeriod);
            }
        }

        public double TimeUntilRate(double newRate)
        {
            if (rate <= newRate)
            {
                return 0;
            }
            double span = DateTime.Now.Subtract(rateSince).TotalSeconds;
            return ((rate * span) / newRate) - span;
        }
    }
}
