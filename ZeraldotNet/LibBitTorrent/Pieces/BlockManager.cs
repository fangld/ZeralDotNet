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
    public class BlockManager
    {
        #region Fields

        private Piece[] _pieceArray;

        #endregion

        #region Constructors

        public BlockManager(long fileLength, int pieceLength, int pieceCount)
        {
            int eachBlockCount = pieceLength / Setting.BlockSize;
            long fullPieceLength = (long)(pieceCount - 1) * (long)pieceLength;
            int lastPieceLength = (int)(fileLength - fullPieceLength);
            int lastBlockCount = lastPieceLength / Setting.BlockSize + 1;
            int lastBlockLength = lastPieceLength - Setting.BlockSize*lastBlockCount;

            _pieceArray = new Piece[pieceCount];
            Parallel.For(0, pieceCount - 1, i =>
                {
                    _pieceArray[i] = new Piece(i, 0, eachBlockCount, Setting.BlockSize);

                });

            _pieceArray[pieceCount - 1] = new Piece(pieceCount - 1, 0, lastBlockCount, Setting.BlockSize,
                                                    lastBlockLength);
        }

        #endregion


    }
}
