namespace ZeraldotNet.LibBitTorrent
{
    public class Parameters
    {
        private readonly int maxUploads = 4;

        public int MaxUploads
        {
            get { return this.maxUploads; }
        }

        private readonly double keepAliveInterval = 120.0;

        public double KeepAliveInterval
        {
            get { return this.keepAliveInterval; }
        }

        //(32K)
        private readonly int downloadSliceSize = 32768;

        public int DownloadSliceSize
        {
            get { return downloadSliceSize; }
        }

        private int requestBackLog = 5;

        public int RequestBackLog
        {
            get { return requestBackLog; }
            set { requestBackLog = value; }
        }

        //(2 ^ 23)
        private int maxMessageLength = 8388608;

        public int MaxMessageLength
        {
            get { return maxMessageLength; }
            set { maxMessageLength = value; }
        }

        private string ip = string.Empty;

        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }

        private ushort minPort = 6881;

        public ushort MinPort
        {
            get { return minPort; }
            set { minPort = value; }
        }

        private ushort maxPort = 6999;

        public ushort MaxPort
        {
            get { return maxPort; }
            set { maxPort = value; }
        }

        private string responseFile = string.Empty;

        public string ResponseFile
        {
            get { return responseFile; }
            set { responseFile = value; }
        }

        private string url = string.Empty;

        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        private string saves = string.Empty;

        public string Saves
        {
            get { return saves; }
            set { saves = value; }
        }

        private double timeout = 300.0;

        public double Timeout
        {
            get { return timeout; }
            set { timeout = value; }
        }

        private double timeoutCheckInterval = 60.0;

        public double TimeoutCheckInterval
        {
            get { return timeoutCheckInterval; }
            set { timeoutCheckInterval = value; }
        }

        //(2 ^ 17)
        private int maxSliceLength = 131072;

        public int MaxSliceLength
        {
            get { return maxSliceLength; }
            set { maxSliceLength = value; }
        }

        private double maxRatePeriod = 20.0;

        public double MaxRatePeriod
        {
            get { return maxRatePeriod; }
            set { maxRatePeriod = value; }
        }

        private string bind = string.Empty;

        public string Bind
        {
            get { return bind; }
            set { bind = value; }
        }

        private double uploadRateFudge = 5.0;

        public double UploadRateFudge
        {
            get { return uploadRateFudge; }
            set { uploadRateFudge = value; }
        }

        private double displayInterval = 1.0;

        public double DisplayInterval
        {
            get { return displayInterval; }
            set { displayInterval = value; }
        }

        private int rerequestInterval = 300;

        public int RerequestInterval
        {
            get { return rerequestInterval; }
            set { rerequestInterval = value; }
        }

        private int minPeers = 20;

        public int MinPeers
        {
            get { return minPeers; }
            set { minPeers = value; }
        }

        private int httpTimeout = 60;

        public int HttpTimeout
        {
            get { return httpTimeout; }
            set { httpTimeout = value; }
        }

        private int maxInitiate = 40;

        public int MaxInitiate
        {
            get { return maxInitiate; }
            set { maxInitiate = value; }
        }

        private bool checkHashes = true;

        public bool CheckHashes
        {
            get { return checkHashes; }
            set { checkHashes = value; }
        }

        private int maxUploadRate = 0;

        public int MaxUploadRate
        {
            get { return maxUploadRate; }
            set { maxUploadRate = value; }
        }

        private double allocatePause = 3.0;

        public double AllocatePause
        {
            get { return allocatePause; }
            set { allocatePause = value; }
        }

        private double snubTime = 60.0;

        public double SnubTime
        {
            get { return snubTime; }
            set { snubTime = value; }
        }

        private bool spew = false;

        public bool Spew
        {
            get { return spew; }
            set { spew = value; }
        }

    }
}
