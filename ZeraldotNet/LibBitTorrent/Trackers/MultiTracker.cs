//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using ZeraldotNet.LibBitTorrent.BEncoding;

//namespace ZeraldotNet.LibBitTorrent.Trackers
//{
//    public class MultiTracker
//    {
//        #region Fields

//        private AnnounceRequest _request;

//        private List<LinkedList<Tracker>> _trackers;

//        private int _currentIndex;

//        private LinkedListNode<Tracker> _currentTrackerNode;

//        #endregion

//        #region Constructors

//        public MultiTracker(AnnounceRequest request)
//        {
//            _request = request;
//        }

//        #endregion

//        #region Events

//        public event EventHandler<AnnounceResponse> GotAnnounceResponse;

//        #endregion

//        #region Methods

//        public void Parse(MetaInfo metaInfo)
//        {
//            _trackers = new List<LinkedList<Tracker>>(metaInfo.AnnounceArrayListCount);
//            for (int i = 0; i < metaInfo.AnnounceArrayListCount; i++)
//            {
//                LinkedList<Tracker> trackerList = new LinkedList<Tracker>();
//                IList<string> announceList = metaInfo.GetAnnounceList(i);
//                for (int j = 0; j < announceList.Count; j++)
//                {
//                    Tracker tracker = new Tracker(_request);
//                    tracker.Url = announceList[j];
//                    tracker.GotAnnounceResponse += tracker_GotAnnounceResponse;
//                    tracker.ConnectFail += tracker_ConnectFail;
//                    trackerList.AddLast(tracker);
//                }
//                _trackers.Add(trackerList);
//            }

//            _currentIndex = 0;
//            _currentTrackerNode = _trackers[0].First;
//        }

//        void tracker_ConnectFail(object sender, System.Net.WebException e)
//        {
//            throw new NotImplementedException();
//        }

//        void tracker_GotAnnounceResponse(object sender, AnnounceResponse e)
//        {
//            throw new NotImplementedException();
//        }

//        public void Announce()
//        {
//            for (int i = 0; i < _trackers.Count; i++)
//            {
//                LinkedListNode<Tracker> trackerNode = _trackers[i].First;
//                do
//                {
//                    trackerNode.Value.Announce();
//                    trackerNode = trackerNode.Next;
//                } while (trackerNode == _trackers[i].First);
//            }
//        }

//        #endregion
//    }
//}
