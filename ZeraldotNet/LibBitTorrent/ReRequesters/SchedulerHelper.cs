using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.ReRequesters
{
    public class SchedulerHelper
    {
        #region Fields

        private ReRequester reRequester;
        private byte[] message;
        private TaskDelegate callbackFunction;
        private string errorMessage;

        #endregion

        #region Constructors

        public SchedulerHelper(ReRequester reRequester, byte[] message, TaskDelegate callbackFunction)
        {
            this.reRequester = reRequester;
            this.message = message;
            this.callbackFunction = callbackFunction;
        }

        public SchedulerHelper(ReRequester reRequester, string errorMessage)
        {
            this.reRequester = reRequester;
            this.errorMessage = errorMessage;
        }

        #endregion

        #region Methods

        public void AddTask()
        {
            reRequester.LastFailed = false;
            reRequester.PostRequest(message, callbackFunction);
        }

        public void FailTask()
        {
            if (reRequester.LastFailed)
            {
                reRequester.ErrorFunction(errorMessage);
            }
            reRequester.LastFailed = true;
        }

        #endregion
    }
}
