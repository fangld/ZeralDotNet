using System;
using ZeraldotNet.LibBitTorrent.ReRequesters;
using ZeraldotNet.LibBitTorrent.Storages;

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

        #region Properties

        public Flag FinishFlag
        {
            get { return finishFlag; }
            set { finishFlag = value; }
        }

        public FinishedDelegate FinishedFunction
        {
            get { return finishedFunction; }
            set { finishedFunction = value; }
        }

        public Storage Storage
        {
            get { return storage; }
            set { storage = value; }
        }

        public ReRequester ReRequester
        {
            set { reRequester = value; }
        }

        public RateMeasure RateMeasure
        {
            set { rateMeasure = value; }
        }

        public Flag DoneFlag
        {
            get { return doneFlag; }
            set { doneFlag = value; }
        }

        public ErrorDelegate ErrorFunction
        {
            get { return errorFunction; }
            set { errorFunction = value; }
        }

        #endregion

        #region Methods

        public void Finished()
        {
            FinishFlag.Set();
            try
            {
                Storage.SetReadonly();
            }
            catch(Exception ex)
            {
                ErrorFunction("trouble setting readonly at end - " + ex.Message);
            }

            if (reRequester != null)
            {
                reRequester.Announce(1, null);
            }
            FinishedFunction();
        }

        public void Failed(string reason)
        {
            DoneFlag.Set();
            if (reason != null)
            {
                ErrorFunction(reason);
            }
        }

        public void DataFlunked(long amount)
        {
            rateMeasure.RejectDate(amount);
            ErrorFunction("a piece failed hash check, re-downloading it");
        }

        #endregion
    }
}
