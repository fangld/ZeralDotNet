using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Reject request message
    /// </summary>
    public class RejectRequestMessage : RequestMessage
    {
        #region Properties

        /// <summary>
        /// The type of message
        /// </summary>
        public override MessageType Type
        {
            get { return MessageType.RejectRequest; }
        }

        #endregion

        #region Constructors

        public RejectRequestMessage()
        {
        }

        public RejectRequestMessage(int index, int begin, int length)
            : base(index, begin, length)
        {
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            string result = string.Format("Reject request {0}:{1}->{2}", Index, Begin, Length);
            return result;
        }

        #endregion
    }
}