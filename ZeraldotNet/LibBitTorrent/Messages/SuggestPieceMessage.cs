using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent.Messages
{
    /// <summary>
    /// Suggest piece message
    /// </summary>
    public class SuggestPieceMessage : HaveMessage
    {
        #region Properties

        /// <summary>
        /// The type of message
        /// </summary>
        public override MessageType Type
        {
            get { return MessageType.SuggestPiece; }
        }

        #endregion

        #region Constructors

        public SuggestPieceMessage()
        {}

        public SuggestPieceMessage(int index):base(index)
        {}

        #endregion

        #region Methods

        public override string ToString()
        {
            string result = string.Format("Suggest piece {0}", Index);
            return result;
        }

        #endregion
    }
}
