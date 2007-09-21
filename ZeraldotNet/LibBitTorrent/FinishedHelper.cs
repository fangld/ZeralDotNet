using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeraldotNet.LibBitTorrent.Storages;
using ZeraldotNet.LibBitTorrent.ReRequesters;

namespace ZeraldotNet.LibBitTorrent
{
    public class FinishedHelper
    {
        #region Fields

        private Flag finishFlag;
        private FinishedDelegate finishedFunction;
        private ReRequester reRequester;
        private Storage storage;
        private ErrorDelegate errorFunction;
        private Flag doneFlag;
        private RateMeasure rateMeasure;

        #endregion

        #region Methods

        public void Finished()
        {
            finishFlag.Set();
            try
            {
                storage.SetReadonly();
            }
            catch(Exception ex)
            {
                errorFunction("trouble setting readonly at end - " + ex.Message);
            }

            if (reRequester != null)
            {
                reRequester.Announce(1, null);
            }
            finishedFunction();
        }

        public void Failed(string reason)
        {
            doneFlag.Set();
            if (reason.Length != 0)
            {
                errorFunction(reason);
            }
        }

        public void DataFlunked(long amount)
        {
            rateMeasure.RejectDate(amount);
            errorFunction("a piece failed hash check, re-downloading it");
        }

        #endregion
    }
}
