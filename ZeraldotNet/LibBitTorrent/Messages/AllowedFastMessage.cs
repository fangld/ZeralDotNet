using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Allowed fast message
    /// </summary>
    public class AllowedFastMessage : HaveMessage
    {
        #region Properties

        /// <summary>
        /// The type of message
        /// </summary>
        public override MessageType Type
        {
            get { return MessageType.AllowedFast; }
        }

        #endregion

        #region Constructors

        public AllowedFastMessage()
        {}
        
        public AllowedFastMessage(int index):base(index)
        {
        }

        #endregion
    }
}
