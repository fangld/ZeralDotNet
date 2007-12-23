using System;
using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent;
using ZeraldotNet.LibBitTorrent.PiecePickers;

namespace ZeraldotNet.TestLibBitTorrent.TestNormalDownloader
{
    public class DummyPiecePicker : IPiecePicker
    {
        List<int> stuff;
        List<string> events;

        public DummyPiecePicker(int num, List<string> events)
        {
            stuff = new List<int>();
            int i;
            for (i = 0; i < num; i++)
            {
                stuff.Add(i);
            }
            this.events = events;
        }

        #region IPiecePicker Members

        public void Complete(int index)
        {
            stuff.Remove(index);
            events.Add("Complete");
        }

        public void GotHave(int index)
        {
            events.Add("Get have");
        }

        public void LostHave(int index)
        {
            events.Add("Lose have");            
        }

        public int Next(WantDelegate haveFunction)
        {
            return 0;
        }

        public int PiecesNumber
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void Requested(int index)
        {
            events.Add("Requested");
        }

        #endregion
    }
}