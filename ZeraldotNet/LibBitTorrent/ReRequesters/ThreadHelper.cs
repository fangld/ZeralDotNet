using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.ReRequesters
{
    public class ThreadHelper
    {
        #region Fields

        private TaskDelegate callbackFunction;
        private SetOnce setOnce;
        private ReRequester reRequester;
        private string url;

        #endregion

        #region Constructors

        public ThreadHelper(string url, ReRequester reRequester, SetOnce setOnce, TaskDelegate callbackFunction)
        {
            this.url = url;
            this.reRequester = reRequester;
            this.setOnce = setOnce;
            this.callbackFunction = callbackFunction;
        }

        #endregion

        #region Methods

        public void CheckFail()
        {
            if (setOnce.Set())
            {
                if (reRequester.LastFailed)
                {
                    reRequester.ErrorFunction("Problem connecting to tracker - timeout exceeder");
                }
                reRequester.LastFailed = true;
                if (callbackFunction != null)
                {
                    callbackFunction();
                }
            }
        }

        public void LauchProcess()
        {
            reRequester.ReRequest(url, setOnce, callbackFunction);
        }

        #endregion


    }
}
