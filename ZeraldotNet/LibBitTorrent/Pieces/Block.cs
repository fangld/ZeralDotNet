using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent.Pieces
{
    /// <summary>
    /// 
    /// </summary>
    public class Block
    {
        #region Properties
        
        /// <summary>
        /// The index of the block in a piece
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The begin of the block in a piece
        /// </summary>
        public int Begin { get; set; }

        /// <summary>
        /// The length of the block
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// The flag that represents the piece whether be downloaded
        /// </summary>
        public bool Downloaded { get; set; }

        /// <summary>
        /// The flag that represents the piece whether be requested
        /// </summary>
        public bool Requested { get; set; }

        #endregion
    }
}
