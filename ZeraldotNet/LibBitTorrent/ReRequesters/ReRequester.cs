﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using ZeraldotNet.LibBitTorrent.BEncoding;

namespace ZeraldotNet.LibBitTorrent.ReRequesters
{
    public class ReRequester
    {
        #region Fields

        private string url;
        private int interval;
        private int last;
        private byte[] trackerid;
        private int announceInterval;
        private int minPeers;
        private int maxPeers;
        private AmountDelegate amountLeftFunction;
        private int timeout;
        private Flag finishFlag;
        private bool lastFailed;
        private HowManyDelegate howManyFunction;
        private ErrorDelegate errorFunction;
        private MeasureTotalDelegate uploadFunction;
        private MeasureTotalDelegate downloadFunction;
        private StartDelegate connectFunction;
        private SchedulerDelegate schedulerFunction;
        private SchedulerDelegate externalSchedulerFunction;

        #endregion

        #region Properties

        public bool LastFailed
        {
            get { return lastFailed; }
            set { this.lastFailed = true; }
        }

        public ErrorDelegate ErrorFunction
        {
            get { return errorFunction; }
        }

        #endregion

        #region Constructors

        public ReRequester(string url, int interval, SchedulerDelegate schedulerFunction, HowManyDelegate howManyFunction,
            int minPeers, StartDelegate connectFunction, SchedulerDelegate externalSchedulerFunction, AmountDelegate amountLeftFunction,
            MeasureTotalDelegate uploadFunction, MeasureTotalDelegate downloadFunction, int port, string ip, byte[] myid,
            byte[] infoHash, int timeout, ErrorDelegate errorFunction, int maxPeers, Flag finishFlag)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}?info_hash={1}&peer_id={2}&port={3}", url, HttpUtility.UrlEncode(infoHash),
                              HttpUtility.UrlEncode(myid), port);
            if (ip.Length != 0)
            {
                sb.AppendFormat("&ip={0}", HttpUtility.UrlEncode(ip));
            }
            this.url = sb.ToString();
            this.interval = interval;
            this.last = 0;
            this.trackerid = null;
            this.announceInterval = 1200;
            this.schedulerFunction = schedulerFunction;
            this.howManyFunction = howManyFunction;
            this.minPeers = minPeers;
            this.connectFunction = connectFunction;
            this.externalSchedulerFunction = externalSchedulerFunction;
            this.amountLeftFunction = amountLeftFunction;
            this.uploadFunction = uploadFunction;
            this.downloadFunction = downloadFunction;
            this.timeout = timeout;
            this.errorFunction = errorFunction;
            this.maxPeers = maxPeers;
            this.finishFlag = finishFlag;
            this.lastFailed = true;
            //this.schedulerFunction()
        }

        #endregion

        #region Methods

        public void ReRequest(string url, SetOnce setOnce, TaskDelegate callbackFunction)
        {
            try
            {
                WebClient webClient = new WebClient();
                byte[] response = webClient.DownloadData(url);
                if(setOnce.Set())
                {
                    SchedulerHelper schedulerHelper = new SchedulerHelper(this, response, callbackFunction);
                    this.externalSchedulerFunction(schedulerHelper.AddTask, 0.0, "Tracker Process Response");
                }
            }
            catch(Exception ex)
            {
                if(setOnce.Set())
                {
                    SchedulerHelper schedulerHelper = new SchedulerHelper(this, "Problem connecting to tracker - " + ex.Message);
                    this.externalSchedulerFunction(schedulerHelper.FailTask, 0.0, "Tracker Fail");
                    this.externalSchedulerFunction(callbackFunction, 0, "Tracker Callback");
                }
            }
        }

        public void PostRequest(byte[] data, TaskDelegate callbackFunction)
        {
            try
            {
                DictionaryHandler dict = (DictionaryHandler) BEncode.Decode(data);
                BTFormat.CheckPeers(dict);
                if (dict.ContainsKey("failure reason"))
                {
                    errorFunction("rejected by tracker - " + (dict["failure reason"] as BytestringHandler).StringText);
                }
                else
                {
                    if (dict.ContainsKey("interval"))
                    {
                        announceInterval = (int)(dict["interval"] as IntHandler).LongValue;
                    }
                    if(dict.ContainsKey("min interval"))
                    {
                        interval = (int) (dict["min interval"] as IntHandler).LongValue;
                    }
                    if (dict.ContainsKey("tracker id"))
                    {
                        trackerid = (dict["tracker id"] as BytestringHandler).ByteArray;
                    }
                    if (dict.ContainsKey("last"))
                    {
                        this.last = (dict["last"] as IntHandler).IntValue;
                    }
                }
            }
            catch
            {
                
            }
        }

        #endregion

    }
}