using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Trackers
{
    /// <summary>
    /// The request information of scrape.
    /// </summary>
    public class ScrapeRequest
    {
        #region Fields

        private List<string> _listInfoHash;

        #endregion

        #region Constructors

        public ScrapeRequest()
        {
            _listInfoHash = new List<string>();
        }

        #endregion

        #region Methods

        public void AddInfoHash(string infoHash)
        {
            _listInfoHash.Add(infoHash);
        }

        public void AddRangeInfoHash(IEnumerable<string> infoHashs)
        {
            _listInfoHash.AddRange(infoHashs);
        }

        public void RemoveInfoHash(string infoHash)
        {
            _listInfoHash.Add(infoHash);
        }

        #endregion
    }
}
