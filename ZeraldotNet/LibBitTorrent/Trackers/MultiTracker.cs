using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeraldotNet.LibBitTorrent.BEncoding;

namespace ZeraldotNet.LibBitTorrent.Trackers
{
    public class MultiTracker
    {
        #region Fields

        private AnnounceRequest _request;

        private List<LinkedList<Tracker>> _trackers;

        #endregion

        #region Constructors

        public MultiTracker(AnnounceRequest request)
        {
            _request = request;
        }

        #endregion

        #region Methods

        public void Parse(MetaInfo metaInfo)
        {
            _trackers = new List<LinkedList<Tracker>>(metaInfo.AnnounceArrayListCount);
            for (int i = 0; i < metaInfo.AnnounceArrayListCount; i++)
            {
                LinkedList<Tracker> trackerList = new LinkedList<Tracker>();
                IList<string> announceList = metaInfo.GetAnnounceList(i);
                for (int j = 0; j < announceList.Count; j++)
                {
                    Tracker tracker = new Tracker(_request);
                    tracker.Url = announceList[j];
                    trackerList.AddLast(tracker);
                }
                _trackers.Add(trackerList);
            }
        }

        public void Announce()
        {
            for (int i = 0; i < _trackers.Count; i++)
            {
                
            }
        }

        #endregion
    }
}
