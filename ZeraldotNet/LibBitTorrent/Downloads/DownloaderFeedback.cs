using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using ZeraldotNet.LibBitTorrent.Chokers;
using ZeraldotNet.LibBitTorrent.Connecters;

namespace ZeraldotNet.LibBitTorrent.Downloads
{
    public class DownloaderFeedback
    {
        #region Fields

        private readonly IChoker choker;
        private readonly SchedulerDelegate addTaskFunction;
        private readonly StatusDelegate statusFunction;
        private readonly MeasureRateDelegate uploadFunction;
        private readonly MeasureRateDelegate downloadFunction;
        private readonly MeasureRateDelegate remainingFunction;
        private readonly AmountDelegate leftFunction;
        private readonly long fileLength;
        private readonly Flag finishFlag;
        private readonly double interval;
        private readonly bool sp;
        List<byte[]> lastIDs;

        #endregion

        #region Constructors

        public DownloaderFeedback(IChoker choker, SchedulerDelegate addTaskFunction, StatusDelegate statusFunction, MeasureRateDelegate uploadFunction,
            MeasureRateDelegate downloadFunction, MeasureRateDelegate remainingFunction, AmountDelegate leftFunction, long fileLength, Flag finishFlag,
            double interval, bool sp)
        {
            this.choker = choker;
            this.addTaskFunction = addTaskFunction;
            this.statusFunction = statusFunction;
            this.uploadFunction = uploadFunction;
            this.downloadFunction = downloadFunction;
            this.remainingFunction = remainingFunction;
            this.leftFunction = leftFunction;
            this.fileLength = fileLength;
            this.finishFlag = finishFlag;
            this.interval = interval;
            this.sp = sp;
            this.lastIDs = new List<byte[]>();
            this.Display();
        }

        #endregion

        #region Methods

        private List<IConnection> rotate()
        {
            List<IConnection> connections = this.choker.GetConnections();
            byte[] connectionsID;
            List<IConnection> result;
            foreach (byte[] id in this.lastIDs)
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    bool isFound = false;
                    connectionsID = connections[i].ID;
                    if (connectionsID.Length == id.Length)
                    {
                        isFound = true;
                        for (int j = 0; j < connectionsID.Length; j++)
                        {
                            if (connectionsID[j] != id[j])
                            {
                                isFound = true;
                                break;
                            }
                        }
                    }

                    if (isFound)
                    {
                        result = connections.GetRange(i, connections.Count - 1);
                        result.AddRange(connections.GetRange(0, i));
                        return result;
                    }
                }
            }

            return connections;
        }

        private void Spew()
        {
            List<IConnection> connections = this.rotate();
            this.lastIDs = new List<byte[]>();
            foreach (IConnection conn in connections)
            {
                this.lastIDs.Add(conn.ID);
            }
        }

        private void Display()
        {
            this.addTaskFunction(this.Display, this.interval, "Update Display");
            if (sp)
            {
                this.Spew();
            }

            if (finishFlag.IsSet)
            {
                this.statusFunction(null, -1, uploadFunction(), -1, -1);
                return;
            }

            double timeEstimate = this.remainingFunction();
            double fractionDone = (this.fileLength - this.leftFunction()) / (double)(this.fileLength);

            Debug.WriteLine("Current : {0}", this.choker.GetConnections().Count.ToString());

            if (timeEstimate != 0)
            {
                statusFunction(null, downloadFunction(), uploadFunction(), fractionDone, timeEstimate);
            }
            else
            {
                statusFunction(null, downloadFunction(), uploadFunction(), fractionDone, -1);
            }
        }

        #endregion

    }
}
