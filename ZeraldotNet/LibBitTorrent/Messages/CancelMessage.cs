using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Cancel message
    /// </summary>
    public class CancelMessage : RequestMessage
    {
        #region Properties

        /// <summary>
        /// The type of message
        /// </summary>
        public override MessageType Type
        {
            get { return MessageType.Cancel; }
        }

        #endregion

        #region Constructors

        public CancelMessage()
        {}

        public CancelMessage(int index, int begin, int length):base(index, begin, length)
        {}

        #endregion

        #region Methods

        public override string ToString()
        {
            string result = string.Format("Cancel {0}:{1}->{2}", Index, Begin, Length);
            return result;
        }

        #endregion
    }
}
