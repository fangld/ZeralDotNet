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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="metaInfo">the metainfo of torrent</param>
        /// <param name="saveAsDirectory">the save as directory</param>
        public BlockManager(MetaInfo metaInfo, string saveAsDirectory)
        {
            _metaInfo = metaInfo;
            int eachPieceBlockCount = metaInfo.PieceLength/Setting.BlockLength;
            long fullPieceLength = (long) (metaInfo.PieceListCount - 1)*(long) metaInfo.PieceLength;
            _lastPieceLength = (int)(metaInfo.SumLength - fullPieceLength);
            _lastPieceBlockCount = _lastPieceLength / Setting.BlockLength + 1;
            _lastBlockLength = _lastPieceLength - Setting.BlockLength*(_lastPieceBlockCount - 1);

            _pieceArray = new Piece[metaInfo.PieceListCount];
            for (int i = 0; i < metaInfo.PieceListCount - 1; i++)
            {
                _pieceArray[i] = new Piece(i, 0, eachPieceBlockCount, Setting.BlockLength);
            }

            _pieceArray[metaInfo.PieceListCount - 1] = new Piece(metaInfo.PieceListCount - 1, 0, _lastPieceBlockCount,
                                                                 Setting.BlockLength, _lastBlockLength);

            _storage = Storage.Create(metaInfo, saveAsDirectory);
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
                for (int i = 0; i < bitfield.Length; i++)
                {
                    if (bitfield[i])
                    {
                        _pieceArray[i].ExistedNumber++;
                    }
                }

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

        /// <summary>
        /// Check the correntness of all pieces
        /// </summary>
        public void CheckPieces()
        {
            for (int i = 0; i < _pieceArray.Length; i++)
            {
                CheckPiece(i);
            }
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
            long offset = _metaInfo.PieceLength*index;
            _storage.Read(piece, offset, pieceLength);
            byte[] rcvPieceHash = Globals.GetSha1Hash(piece); //Globals.Sha1.ComputeHash(piece);
            byte[] metaHash = _metaInfo.GetHashValues(index);

            _pieceArray[index].Checked = Globals.IsHashEqual(rcvPieceHash, metaHash, 20);

            //for (int i = 0; i < pieceLength; i++)
            //{
            //    if (piece[i] != _storage.Buffer[i + offset])
            //    {
            //        Console.WriteLine("{0}: {1}, {2}", i, piece[i], _storage.Buffer[i + offset]);
            //    }
            //}

            //if (!_pieceArray[index].Checked)
            //{
            //    byte[] newHash = Globals.GetSha1Hash(piece);//Globals.Sha1.ComputeHash(piece);
            //    byte[] newHash1 = Globals.GetSha1Hash(piece);//Globals.Sha1.ComputeHash(piece);

            //    for (int i = 0; i < 20; i++)
            //    {
            //        Console.WriteLine("metaHash:{0}, rcvPieceHash:{1}, newHash:{2}, newHash1:{3}", metaHash[i], rcvPieceHash[i], newHash[i], newHash1[i]);
            //    }
            //    _storage.MoveHashFail(index);
            //}

            //if (_pieceArray[index].Checked)
            //{
            //    if (_storage.HashFailBuffers.ContainsKey(index))
            //    {
            //        byte[] newHash = Globals.GetSha1Hash(_storage.HashFailBuffers[index].Buffer);//Globals.Sha1.ComputeHash(_storage.HashFailBuffers[index].Buffer);
            //        for (int i = 0; i < 20; i++)
            //        {
            //            Console.WriteLine("metaHash:{0}, rcvPieceHash:{1}, failHash:{2}, newHash:{3}", metaHash[i], rcvPieceHash[i], _storage.HashFailBuffers[index].Hash[i], newHash[i]);
            //        }

            //        for (int i = 0; i < pieceLength; i++)
            //        {
            //            if (piece[i] != _storage.HashFailBuffers[index].Buffer[i])
            //            {
            //                Console.WriteLine("{0}: {1}, {2}", i, piece[i], _storage.HashFailBuffers[index].Buffer[i]);
            //            }
            //        }
            //    }
            //}

            //if (_storage.HashFailBuffers.ContainsKey(index))
            //{
            //    for (int i = 0; i < pieceLength; i++)
            //    {
            //        if (piece[i] != _storage.HashFailBuffers[index].Buffer[i])
            //        {
            //            Console.WriteLine("{0}: {1}, {2}", i, piece[i], _storage.HashFailBuffers[index].Buffer[i]);
            //        }
            //    }
            //}

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
                _pieceArray[index].ResetDownloaded();
            }
        }

        ///// <summary>
        ///// Reset the requested flag of the corresponding piece
        ///// </summary>
        ///// <param name="indexArray">the array of index</param>
        //public void ResetRequested(int[] indexArray)
        //{
        //    lock (_pieceArray)
        //    {
        //        for (int i = 0; i < indexArray.Length; i++)
        //        {
        //            _pieceArray[indexArray[i]].ResetRequested();
        //        }
        //    }
        //}

        public void ResetRequestedByBlocks(Block[] blocks)
        {
            lock (_pieceArray)
            {
                for (int i = 0; i < blocks.Length; i++)
                {
                    blocks[i].Requested = false;
                }
            }
        }

        /// <summary>
        /// Reset the requested flag of the index-th piece
        /// </summary>
        /// <param name="index">the index of piece</param>
        public void ResetRequestedByIndex(int index)
        {
            lock (_pieceArray)
            {
                _pieceArray[index].ResetRequested();
            }
        }

        /// <summary>
        /// Get the next blocks
        /// </summary>
        /// <param name="bitfield">The bitfield peer holds</param>
        /// <param name="count">The requested count</param>
        /// <returns>Return the next blocks</returns>
        public Block[] GetNextBlocks(bool[] bitfield, int count)
        {
            lock (_pieceArray)
            {
                List<Block> result = new List<Block>(count);

                List<Piece> partialDownloadedPieces = new List<Piece>();
                List<Piece> nonDownloadedPieces = new List<Piece>();
                
                //Discriminate partial downloaded piece and none downloaded piece
                foreach (Piece p in _pieceArray)
                {
                    if (bitfield[p.Index] && !p.AllDownloaded)
                    {
                        if (p.PartialDownloaded)
                        {
                            partialDownloadedPieces.Add(p);
                        }
                        else
                        {
                            nonDownloadedPieces.Add(p);
                        }
                    }
                }

                //Find the minimal existed blocks in partial downloaded pieces
                int partialCount = SelectBlocks(partialDownloadedPieces, result, 0, count, b => !b.Downloaded && !b.Requested);

                if (partialCount != count)
                {
                    //Find the minimal existed blocks in non-downloaded pieces
                    SelectBlocks(nonDownloadedPieces, result, partialCount, count - partialCount, b => !b.Requested);
                }

                return result.ToArray();
            }
        }

        public int SelectBlocks(List<Piece> pieces, List<Block> blocks, int selectedCount, int count, Predicate<Block> match)
        {
            pieces.Sort((piece1, piece2) => piece1.ExistedNumber - piece2.ExistedNumber);

            foreach (Piece piece in pieces)
            {
                foreach (Block block in piece)
                {
                    if (match(block))
                    {
                        blocks.Add(block);
                        block.Requested = true;
                        selectedCount++;
                        if (selectedCount == count)
                        {
                            return count;
                        }
                    }
                }
            }
            return selectedCount;
        }

        public void Stop()
        {
            _storage.Close();
        }

        #endregion
    }
}
