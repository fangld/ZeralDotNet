using System;
using System.Collections.Generic;
using System.Text;
using ZeraldotNet.LibBitTorrent.Connecters;
using ZeraldotNet.LibBitTorrent.Chokers;

namespace ZeraldotNet.LibBitTorrent
{
    public class DownloadFeedback
    {
        #region Fields

        private Choker choker;
        private SchedulerDelegate addTaskFunction;
        private StatusDelegate statusFunction;
        private MeasureRateDelegate uploadFunction;
        private MeasureRateDelegate downloadFunction;
        private MeasureRateDelegate remainingFunction;
        private AmountDelegate amountLeftFunction;
        private long fileLength;
        private Flag finishFlag;
        private double interval;
        private bool sp;
        private List<byte[]> lastIDs;

        #endregion

        #region Constructors

        public DownloadFeedback(Choker choker, SchedulerDelegate addTaskFuncion, StatusDelegate statusFunction, MeasureRateDelegate uploadFunction,
            MeasureRateDelegate downloadFunction, MeasureRateDelegate remainingFunction, AmountDelegate amountLeftFunction,
            long fileLength, Flag finishFlag, double interval, bool sp)
        {
            this.choker = choker;
            this.addTaskFunction = addTaskFunction;
            this.statusFunction = statusFunction;
            this.uploadFunction = uploadFunction;
            this.downloadFunction = downloadFunction;
            this.remainingFunction = remainingFunction;
            this.amountLeftFunction = amountLeftFunction;
            this.fileLength = fileLength;
            this.finishFlag = finishFlag;
            this.interval = interval;
            this.sp = sp;
            this.lastIDs = new List<byte[]>();
            this.Display();
        }

        #endregion

        #region Methdos

        private List<IConnection> Rotate()
        {
            List<IConnection> connections = this.choker.GetConnections();
            foreach (byte[] id in lastIDs)
            {
                for (int i = 0; i < connections.Count;i++)
                {
                    bool bFound = false;
                    byte[] connectionID = connections[i].ID;
                    if (connectionID.Length == id.Length)
                    {
                        bFound = true;
                        for (int j = 0; j <connectionID.Length; j++)
                        {
                            if (connectionID[j] != id[j])
                            {
                                break;
                            }
                        }
                    }

                    if(bFound)
                    {
                        List<IConnection> rotateConnections = connections.GetRange(i, connections.Count - 1);
                        rotateConnections.AddRange(connections.GetRange(0, i));
                        return rotateConnections;
                    }
                }
            }
            return connections;
        }

        public void Spew()
        {
            StringBuilder sb = new StringBuilder();
            List<IConnection> connections = this.Rotate();
            this.lastIDs = new List<byte[]>();
            foreach (IConnection connection in connections)
            {
                this.lastIDs.Add(connection.ID);
            }
        }

        public void Display()
        {
            this.addTaskFunction(this.Display, this.interval, "Update Display");
            if(sp)
            {
                this.Spew();
            }
            if(finishFlag.IsSet)
            {
                this.statusFunction(null, -1, uploadFunction(), -1, -1);
                return;
            }
            double timeEstimate = this.remainingFunction();
            double fractionDone = (this.fileLength - this.amountLeftFunction())/(double) (this.fileLength);
            if (Math.Abs(timeEstimate) > double.Epsilon)
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
