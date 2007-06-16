using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public delegate void StatusDelegate(string message, double downloadRate, double uploadRate, double fractionDone, double timeEstimate);
    public delegate void ErrorDelegate(string message);

    public class Connecter
    {
        private bool rateCapped;

        private Measure totalUp;

        private int maxUploadRate;

        private SchedulerDelegate schedulerFunction;

        public void UpdateUploadRate(int amount)
        {

            throw new NotImplementedException();
            totalUp.UpdateRate(amount);
            if (maxUploadRate > 0 && totalUp.NonUpdatedRate > maxUploadRate)
            {
                rateCapped = true;
            }
        }
    }
}
