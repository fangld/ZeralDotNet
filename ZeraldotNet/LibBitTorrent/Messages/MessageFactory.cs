namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// 网络信息单件模式
    /// </summary>
    public class MessageFactory
    {
        #region Private Field

        /// <summary>
        /// choke网络信息
        /// </summary>
        private static readonly ChokeMessage chokeMessage;

        /// <summary>
        /// unchoke网络信息
        /// </summary>
        private static readonly UnchokeMessage unchokeMessage;

        /// <summary>
        /// interested网络信息
        /// </summary>
        private static readonly InterestedMessage interestedMessage;


        /// <summary>
        /// not interested网络信息
        /// </summary>
        private static readonly NotInterestedMessage notInterestedMessage;

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        static MessageFactory()
        {
            chokeMessage = new ChokeMessage();
            unchokeMessage = new UnchokeMessage();
            interestedMessage = new InterestedMessage();
            notInterestedMessage = new NotInterestedMessage();
        }

        #endregion

        #region Methods

        /// <summary>
        /// 返回choke网络信息
        /// </summary>
        /// <returns>返回choke网络信息</returns>
        public static ChokeMessage GetChokeMessage()
        {
            return chokeMessage;
        }

        /// <summary>
        /// 返回unchoke网络信息
        /// </summary>
        /// <returns>返回unchoke网络信息</returns>
        public static UnchokeMessage GetUnchokeMessage()
        {
            return unchokeMessage;
        }

        /// <summary>
        /// 返回interested网络信息
        /// </summary>
        /// <returns>返回interested网络信息</returns>
        public static InterestedMessage GetInterestedMessage()
        {
            return interestedMessage;
        }

        /// <summary>
        /// 返回not interested网络信息
        /// </summary>
        /// <returns>返回not interested网络信息</returns>
        public static NotInterestedMessage GetNotInterestedMessage()
        {
            return notInterestedMessage;
        }

        /// <summary>
        /// 返回have网络信息
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <returns>返回have网络信息</returns>
        public static HaveMessage GetHaveMessage(int index)
        {
            return new HaveMessage(index);
        }

        /// <summary>
        /// 返回bitfield网络信息
        /// </summary>
        /// <param name="bitfield">已经下载的文件片断</param>
        /// <returns>返回bitfield网络信息</returns>
        public static BitfieldMessage GetBitfieldMessage(bool[] bitfield)
        {
            return new BitfieldMessage(bitfield);
        }

        /// <summary>
        /// 返回request网络信息
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断的起始位置</param>
        /// <param name="length">子片断的长度</param>
        /// <returns>返回request网络信息</returns>
        public static RequestMessage GetRequestMessage(int index, int begin, int length)
        {
            return new RequestMessage(index, begin, length);
        }

        /// <summary>
        /// 返回piece网络信息
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断的起始位置</param>
        /// <param name="pieces">子片断的数据</param>
        /// <returns>返回piece网络信息</returns>
        public static PieceMessage GetPieceMessage(int index, int begin, byte[] pieces)
        {
            return new PieceMessage(index, begin, pieces);
        }

        /// <summary>
        /// 返回cancel网络信息
        /// </summary>
        /// <param name="index">片断索引号</param>
        /// <param name="begin">子片断的起始位置</param>
        /// <param name="length">子片断的长度</param>
        /// <returns>返回cancel网络信息</returns>
        public static CancelMessage GetCancelMessage(int index, int begin, int length)
        {
            return new CancelMessage(index, begin, length);
        }

        /// <summary>
        /// 返回port网络信息
        /// </summary>
        /// <param name="port">DHT监听端口</param>
        /// <returns>返回port网络信息</returns>
        public static PortMessage GetPortMessage(ushort port)
        {
            return new PortMessage(port);
        }

        /// <summary>
        /// 返回handshake网络信息
        /// </summary>
        /// <param name="downloadID"></param>
        /// <param name="peerID"></param>
        /// <returns></returns>
        public static HandshakeMessage GetHandshakeMessage(byte[] downloadID, byte[] peerID)
        {
            return new HandshakeMessage(downloadID, peerID);
        }

        #endregion
    }
}
