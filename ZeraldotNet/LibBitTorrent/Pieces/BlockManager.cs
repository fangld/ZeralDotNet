using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeraldotNet.LibBitTorrent.BEncoding;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.LibBitTorrent.Pieces
{
    /// <summary>
    /// Handle block selection, read and write
    /// </summary>
    public class BlockManager
    {
        #region Fields

        private Piece[] _pieceArray;

        private Storage _storage;

        private MetaInfo _metaInfo;

        private int _lastPieceLength;

        private int _lastPieceBlockCount;

        private int _lastBlockLength;

        #endregion

        #region Properties

        public Piece this[int index]
        {
            get { return _pieceArray[index]; }
        }

        /// <summary>
        /// Return the value of existing the next piece that can be downloaded
        /// </summary>
        public bool HaveNextPiece
        {
            get
            {
                lock (_pieceArray)
                {
                    return Array.Exists(_pieceArray, p => p.ExistedNumber != 0 && !p.AllDownloaded);
                }
            }
        }

        /// <summary>
        /// Return the value of complete downloading all pieces
        /// </summary>
        public bool Completed
        {
            get
            {
                lock (_pieceArray)
                {
                    return Array.TrueForAll(_pieceArray, p => p.Checked);
                }
            }
        }

        public bool HaveNone
        {
            get
            {
                lock (_pieceArray)
                {
                    return Array.TrueForAll(_pieceArray, p => !p.Checked);
                }
            }
        }

        public bool HaveAll
        {
            get
            {
                lock (_pieceArray)
                {
                    return Array.TrueForAll(_pieceArray, p => p.Checked);
                }
            }
        }

        #endregion

        #region Constructors

        public BlockManager(MetaInfo metaInfo, string saveAsDirectory)
        {
            _metaInfo = metaInfo;
            SingleFileMetaInfo singleFileMetaInfo = metaInfo as SingleFileMetaInfo;
            int eachPieceBlockCount = metaInfo.PieceLength/Setting.BlockLength;
            long fullPieceLength = (long) (metaInfo.PieceListCount - 1)*(long) metaInfo.PieceLength;
            _lastPieceLength = (int)(singleFileMetaInfo.Length - fullPieceLength);
            _lastPieceBlockCount = _lastPieceLength / Setting.BlockLength + 1;
            _lastBlockLength = _lastPieceLength - Setting.BlockLength*(_lastPieceBlockCount - 1);

            _pieceArray = new Piece[metaInfo.PieceListCount];
            Parallel.For(0, metaInfo.PieceListCount - 1,
                         i => { _pieceArray[i] = new Piece(i, 0, eachPieceBlockCount, Setting.BlockLength); });

            _pieceArray[metaInfo.PieceListCount - 1] = new Piece(metaInfo.PieceListCount - 1, 0, _lastPieceBlockCount,
                                                                 Setting.BlockLength, _lastBlockLength);


            _storage = new Storage(metaInfo, saveAsDirectory);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the bit of the assigned piece
        /// </summary>
        /// <param name="index">The index of the assigned piece</param>
        public void SetBit(int index)
        {
            _pieceArray[index].Checked = true;
        }

        /// <summary>
        /// Return the bit field of pieces
        /// </summary>
        /// <returns>The bit field of pieces</returns>
        public bool[] GetBitField()
        {
            bool[] result = new bool[_pieceArray.Length];
            lock (_pieceArray)
            {
                Parallel.For(0, result.Length, i => result[i] = _pieceArray[i].Checked);
            }
            return result;
        }

        /// <summary>
        /// Write block to storage
        /// </summary>
        /// <param name="buffer">the buffer of block</param>
        /// <param name="index">the index of block</param>
        /// <param name="begin">the begin of block</param>
        public void Write(byte[] buffer, int index, int begin)
        {
            long offset = _metaInfo.PieceLength*index + begin;
            _storage.Write(buffer, offset);

            int blockOffset = begin / Setting.BlockLength;
            lock (_pieceArray[index])
            {
                _pieceArray[index][blockOffset].Downloaded = true;
            }
        }

        /// <summary>
        /// Read the block from storage
        /// </summary>
        /// <param name="index">the index of block</param>
        /// <param name="begin">the begin of block</param>
        /// <param name="length">the length of block</param>
        /// <returns>Return the block</returns>
        public byte[] Read(int index, int begin, int length)
        {
            byte[] result = new byte[length];
            long offset = index * _metaInfo.PieceLength + begin;
            _storage.Read(result, offset, length);
            return result;
        }

        /// <summary>
        /// Handle the index of a have message
        /// </summary>
        /// <param name="index">the index of a have message</param>
        /// <returns>whether the piece be downloaded</returns>
        public bool ReceiveHave(int index)
        {
            bool result;
            lock (_pieceArray[index])
            {
                _pieceArray[index].ExistedNumber++;
                result = !_pieceArray[index].AllDownloaded;
            }
            return result;
        }

        /// <summary>
        /// Add the existing number of pieces and return whether other peer has at least a piece that local peer lack of.
        /// </summary>
        /// <param name="bitfield">the bitfield of other peer</param>
        /// <returns>the flag that is whether other peer has at least a piece that local peer lack of</returns>
        public bool ReceiveBitfield(bool[] bitfield)
        {
            bool result = false;
            lock (_pieceArray)
            {
                Parallel.For(0, bitfield.Length, i => { if (bitfield[i]) _pieceArray[i].ExistedNumber++; });
                for (int i = 0; i < _pieceArray.Length; i++)
                {
                    if (!_pieceArray[i].AllDownloaded && bitfield[i])
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        public void CheckPieces()
        {
            Parallel.For(0, _pieceArray.Length, i => CheckPiece(i));
        }

        /// <summary>
        /// Check the correntness of piece
        /// </summary>
        /// <param name="index">The index of piece</param>
        /// <returns>If received piece is correct return true, otherwise return false</returns>
        public bool CheckPiece(int index)
        {
            int pieceLength = index != _metaInfo.PieceListCount - 1 ? _metaInfo.PieceLength : _lastPieceLength;
            byte[] piece = new byte[pieceLength];
            long offset = _metaInfo.PieceLength * index;
            _storage.Read(piece, offset, pieceLength);
            byte[] rcvPieceHash = Globals.Sha1.ComputeHash(piece);
            byte[] metaHash = _metaInfo.GetHash(index);

            _pieceArray[index].Checked = Globals.IsHashEqual(rcvPieceHash, metaHash, 20);
            return _pieceArray[index].Checked;
        }

        /// <summary>
        /// Reset the downloaded flag of the corresponding piece
        /// </summary>
        /// <param name="index">the index of piece</param>
        public void ResetDownloaded(int index)
        {
            lock (_pieceArray[index])
            {
                _pieceArray[index].AllDownloaded = false;
            }
        }

        /// <summary>
        /// Reset the requested flag of the corresponding piece
        /// </summary>
        /// <param name="indexArray">the array of index</param>
        public void ResetRequested(int[] indexArray)
        {
            lock (_pieceArray)
            {
                Parallel.For(0, indexArray.Length, i => _pieceArray[indexArray[i]].AllRequested = false);
            }
        }

        public Block[] GetNextBlocks(bool[] bitfield, int number)
        {
            lock (_pieceArray)
            {
                IList<Block> result = new List<Block>(number);

                Piece[] candidatePieces = _pieceArray.Where(p => bitfield[p.Index] && !p.AllDownloaded && p.ExistedNumber > 0).ToArray();

                //Find partial downloaded piece
                Piece[] partialDownloadedPieces = candidatePieces.Where(p => p.PartialDownloaded).ToArray();

                //Find the least existed block
                Array.Sort(partialDownloadedPieces, (piece1, piece2) => piece1.ExistedNumber - piece2.ExistedNumber);

                for (int i = 0; i < partialDownloadedPieces.Length; i++)
                {
                    for (int j = 0; j < partialDownloadedPieces[i].BlockCount; j++)
                    {
                        if (!(partialDownloadedPieces[i][j].Downloaded || partialDownloadedPieces[i][j].Requested))
                        {
                            result.Add(partialDownloadedPieces[i][j]);
                            partialDownloadedPieces[i][j].Requested = true;
                            if (result.Count == number)
                                return result.ToArray();
                        }
                    }
                }


                if (result.Count != number)
                {
                    //Find non-downloaded piece
                    Piece[] noneDownloadedPieces = candidatePieces.Where(p => !p.PartialDownloaded).ToArray();

                    //Find the least existed block
                    Array.Sort(noneDownloadedPieces, (piece1, piece2) => piece1.ExistedNumber - piece2.ExistedNumber);

                    for (int i = 0; i < noneDownloadedPieces.Length; i++)
                    {
                        for (int j = 0; j < noneDownloadedPieces[i].BlockCount; j++)
                        {
                            if (!(noneDownloadedPieces[i][j].Downloaded || noneDownloadedPieces[i][j].Requested))
                            {
                                result.Add(noneDownloadedPieces[i][j]);
                                noneDownloadedPieces[i][j].Requested = true;
                                if (result.Count == number)
                                    return result.ToArray();

                            }
                        }
                    }
                }

                return result.ToArray();
            }
        }

        ///// <summary>
        ///// Get the next index array of pieces
        ///// </summary>
        ///// <param name="bitfield">the bitfield of pieces that peer holds</param>
        ///// <param name="number">the number of requested pieces</param>
        //public Piece[] GetNextPieces(bool[] bitfield, int number)
        //{
        //    lock (_pieceArray)
        //    {
        //        List<Piece> result = new List<Piece>(number);
        //        List<Piece> minExistedNumberList = new List<Piece>();
        //        Piece[] toBeDownloadArray =
        //            _pieceArray.Where(
        //                piece =>
        //                !piece.AllDownloaded && !piece.AllRequested && piece.ExistedNumber > 0 && bitfield[piece.Index]).ToArray();

        //        while (toBeDownloadArray.Length > 0 && result.Count != number)
        //        {
        //            int minExistedNumber = toBeDownloadArray[0].ExistedNumber;
        //            for (int i = 0; i < toBeDownloadArray.Length; i++)
        //            {
        //                if (toBeDownloadArray[i].ExistedNumber < minExistedNumber)
        //                {
        //                    minExistedNumberList.Clear();
        //                    minExistedNumberList.Add(toBeDownloadArray[i]);
        //                    minExistedNumber = toBeDownloadArray[i].ExistedNumber;
        //                    continue;
        //                }

        //                if (toBeDownloadArray[i].ExistedNumber == minExistedNumber)
        //                {
        //                    minExistedNumberList.Add(toBeDownloadArray[i]);
        //                }
        //            }

        //            int remainingCount = number - result.Count;

        //            if (remainingCount >= minExistedNumberList.Count)
        //            {
        //                result.AddRange(minExistedNumberList);
        //                Parallel.ForEach(minExistedNumberList, piece => piece.AllRequested = true);
        //            }
        //            else
        //            {
        //                for (int i = 0; i < remainingCount; i++)
        //                {
        //                    int randomIndex = Globals.RandomGenerator.Next(minExistedNumberList.Count);
        //                    result.Add(minExistedNumberList[randomIndex]);
        //                    minExistedNumberList[randomIndex].AllRequested = true;
        //                    minExistedNumberList.RemoveAt(randomIndex);
        //                }
        //            }
        //            minExistedNumberList.Clear();
        //        }

        //        return result.ToArray();
        //    }
        //}

        public void Stop()
        {
            _storage.Close();
        }

        #endregion
    }
}
