using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent.Pieces
{
    /// <summary>
    /// The information of piece
    /// </summary>
    public class Piece : IComparable<Piece>, IEquatable<Piece>
    {
        #region Properties

        /// <summary>
        /// The index of piece
        /// </summary>
        public int Index;

        /// <summary>
        /// The number of piece that is existed by the remote clients
        /// </summary>
        public int ExistingNumber;

        #endregion

        public int CompareTo(Piece other)
        {
            return ExistingNumber.CompareTo(other.ExistingNumber);
        }

        public bool Equals(Piece other)
        {
            return Index.Equals(other.Index) && ExistingNumber.Equals(other.ExistingNumber);
        }
    }
}
