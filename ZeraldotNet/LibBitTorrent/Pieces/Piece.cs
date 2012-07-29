﻿using System;
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
        public int ExistedNumber;

        /// <summary>
        /// The flag that represents the piece whether be downloaded
        /// </summary>
        public bool Downloaded;

        /// <summary>
        /// The flag that represents the piece whether be requested
        /// </summary>
        public bool Requested;

        #endregion

        public int CompareTo(Piece other)
        {
            return ExistedNumber.CompareTo(other.ExistedNumber);
        }

        public bool Equals(Piece other)
        {
            return Index.Equals(other.Index);// && ExistedNumber.Equals(other.ExistedNumber) && Downloaded.Equals(other.Downloaded) && Requested.Equals(other.Requested);
        }
    }
}
