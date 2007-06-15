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
        private Connection connection;

        public Connection Connection
        {
            set { this.connection = value; }
        }

        private Choker choker;

        public Choker Choker
        {
            set { this.choker = value; }
        }

        private StorageWrapper storageWrapper;

        public StorageWrapper StorageWrapper
        {
            set { this.storageWrapper = value; }
        }

        private int maxSliceLength;

        public int MaxSliceLength
        {
            set { this.maxSliceLength = value; }
        }

        private double maxRatePeriod;

        public double MaxRatePeriod
        {
            set { this.maxRatePeriod = value; }
        }

        private bool choked;
        private bool interested;
        private List<ActiveRequest> buffer;

        private Measure measure;

        public Measure Measure
        {
            get { return this.measure; }
            set { this.measure = value; }
        }

        public Upload(Connection connection, Choker choker, StorageWrapper storageWrapper, int maxSliceLength,
            double maxRatePeriod, double fudge)
        {
            Connection = connection;
            Choker = choker;
            StorageWrapper = storageWrapper;
            MaxRatePeriod = maxRatePeriod;
            choked = true;
            interested = false;
            buffer = new List<ActiveRequest>();
            measure = new Measure(maxRatePeriod, fudge);

            if (storageWrapper.DoIHaveAnything())
            {
                //connection.
            }
        }
    }
}
