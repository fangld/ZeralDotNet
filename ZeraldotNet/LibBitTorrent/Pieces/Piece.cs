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
        #region Fields

        private Block[] _blockArray;

        #endregion

        #region Properties

        /// <summary>
        /// The index of piece
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The number of piece that is existed by the remote clients
        /// </summary>
        public int ExistedNumber { get; set; }

        public int BlockCount { get; set; }

        /// <summary>
        /// The flag that represents the piece whether be downloaded
        /// </summary>
        public bool Downloaded
        {
            get
            {
                return Array.TrueForAll(_blockArray, block => block.Downloaded);
            } 
            set
            {
                
                Parallel.For(0, _blockArray.Length, i => _blockArray[i].Downloaded = value);
            }
        }

        /// <summary>
        /// The flag that represents the pieces whether be correctness.
        /// </summary>
        public bool Checked { get; set; }

        /// <summary>
        /// The flag that represents the piece whether be requested
        /// </summary>
        public bool Requested { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Block this[int index] 
        {
            get { return _blockArray[index]; }
            set { _blockArray[index] = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index">The index of piece</param>
        /// <param name="existedNumber">The existed number of piece</param>
        /// <param name="blockCount">The count of blocks in this piece</param>
        /// <param name="blockLength">The length of block</param>
        public Piece(int index, int existedNumber, int blockCount, int blockLength)
        {
            Index = index;
            ExistedNumber = existedNumber;
            BlockCount = blockCount;
            Requested = false;
            _blockArray = new Block[blockCount];
            int begin = 0;
            for (int i = 0; i < blockCount; i++)
            {
                _blockArray[i] = new Block { Index = index, Begin = begin, Length = blockLength, Downloaded = false, Requested = false };
                begin += blockLength;
            }
            Downloaded = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index">The index of piece</param>
        /// <param name="existedNumber">The existed number of piece</param>
        /// <param name="blockCount">The count of blocks in this piece</param>
        /// <param name="blockLength">The length of block</param>
        /// <param name="lastBlockLength">The length of last block</param>
        public Piece(int index, int existedNumber, int blockCount, int blockLength, int lastBlockLength)
        {
            Index = index;
            ExistedNumber = existedNumber;
            BlockCount = blockCount;
            Requested = false;
            _blockArray = new Block[blockCount];
            int begin = 0;
            for (int i = 0; i < blockCount - 1; i++)
            {
                _blockArray[i] = new Block { Index = index, Begin = begin, Length = blockLength, Downloaded = false, Requested = false };
                begin += blockLength;
            }
            _blockArray[blockCount - 1] = new Block { Index = index, Begin = begin, Length = lastBlockLength, Downloaded = false, Requested = false }; ;
            Downloaded = false;
        }

        #endregion

        #region Methods

        public int CompareTo(Piece other)
        {
            return ExistedNumber.CompareTo(other.ExistedNumber);
        }

        public bool Equals(Piece other)
        {
            return Index.Equals(other.Index);
        }

        #endregion
    }
}
