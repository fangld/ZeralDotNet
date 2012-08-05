using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeraldotNet.LibBitTorrent.Storages;

namespace ZeraldotNet.LibBitTorrent.Pieces
{
    /// <summary>
    /// The manager that handle selecting pieces and maintain the existed number of pieces
    /// </summary>
    public class PieceManager
    {
        #region Fields

        /// <summary>
        /// The list of pieces
        /// </summary>
        private Piece[] _pieceArray;

        private Storage _storage;

        #endregion

        #region Properties

        /// <summary>
        /// Return the value of existing the next piece that can be downloaded
        /// </summary>
        public bool HaveNextPiece
        {
            get
            {
                lock (_pieceArray)
                {
                    return _pieceArray.Any(p => p.ExistedNumber != 0 && !p.Downloaded);
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
                    return _pieceArray.All(piece => piece.Downloaded);
                }
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct the new piece manager
        /// </summary>
        /// <param name="booleans">The array of booleans that contains the existed piece flag</param>
        public PieceManager(bool[] booleans)
        {
            int piecesNumber = booleans.Length;
            _pieceArray = new Piece[piecesNumber];

#warning new Piece is wrong, because the blockcCount parameter is not 0. It is just for PieceManager and BlockManager will instead of PieceManager.
            Parallel.For(0, piecesNumber, i => _pieceArray[i] = new Piece(i, 0, 0, Setting.BlockSize));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the next index array of pieces
        /// </summary>
        /// <param name="booleans">the booleans of pieces that peer holds</param>
        /// <param name="number">the required number</param>
        public Piece[] GetNextPieces(bool[] booleans, int number)
        {
            lock (_pieceArray)
            {
                List<Piece> result = new List<Piece>(number);
                List<Piece> minExistedNumberList = new List<Piece>();
                Piece[] toBeDownloadArray =
                    _pieceArray.Where(
                        piece =>
                        !piece.Downloaded && !piece.Requested && piece.ExistedNumber > 0 && booleans[piece.Index]).ToArray();

                while (toBeDownloadArray.Length > 0 && result.Count != number)
                {
                    int minExistedNumber = toBeDownloadArray[0].ExistedNumber;
                    for (int i = 0; i < toBeDownloadArray.Length; i++)
                    {
                        if (toBeDownloadArray[i].ExistedNumber < minExistedNumber)
                        {
                            minExistedNumberList.Clear();
                            minExistedNumberList.Add(toBeDownloadArray[i]);
                            minExistedNumber = toBeDownloadArray[i].ExistedNumber;
                            continue;
                        }

                        if (toBeDownloadArray[i].ExistedNumber == minExistedNumber)
                        {
                            minExistedNumberList.Add(toBeDownloadArray[i]);
                        }
                    }

                    int remainingCount = number - result.Count;

                    if (remainingCount >= minExistedNumberList.Count)
                    {
                        result.AddRange(minExistedNumberList);
                        Parallel.ForEach(minExistedNumberList, piece => piece.Requested = true);
                    }
                    else
                    {
                        for (int i = 0; i < remainingCount; i++)
                        {
                            int randomIndex = Globals.Random.Next(minExistedNumberList.Count);
                            result.Add(minExistedNumberList[randomIndex]);
                            minExistedNumberList[randomIndex].Requested = true;
                            minExistedNumberList.RemoveAt(randomIndex);
                        }
                    }
                    minExistedNumberList.Clear();
                }

                return result.ToArray();
            }
        }

        public void SetDownloaded(int index)
        {
            lock (_pieceArray)
            {
                Piece piece = _pieceArray[index];
                piece.Downloaded = true;
            }
        }

        public bool GetDownloaded(int index)
        {
            bool result;
            lock (_pieceArray)
            {
                Piece piece = _pieceArray[index];
                result = piece.Downloaded;
            }
            return result;
        }

        public void AddExistedNumber(int index)
        {
            lock (_pieceArray)
            {
                _pieceArray[index].ExistedNumber++;
            }
        }

        public void AddExistedNumber(bool[] booleans)
        {
            lock (_pieceArray)
            {
                for (int i = 0; i < booleans.Length; i++)
                {
                    if (booleans[i])
                    {
                        _pieceArray[i].ExistedNumber++;
                    }
                }
            }
        }

        public void RemoveExistedNumber(bool[] booleans)
        {
            lock (_pieceArray)
            {
                for (int i = 0; i < booleans.Length; i++)
                {
                    if (booleans[i])
                    {
                        _pieceArray[i].ExistedNumber--;
                    }
                }
            }
        }

        public void RemoveRequested(int[] requestedIndexes)
        {
            lock (_pieceArray)
            {
                Parallel.For(0, requestedIndexes.Length, i => _pieceArray[requestedIndexes[i]].Requested = false);
            }
        }

        public bool[] GetBooleans()
        {
            bool[] result = new bool[_pieceArray.Length];
            lock (_pieceArray)
            {
                Parallel.For(0, result.Length, i => result[i] = _pieceArray[i].Downloaded);
            }
            return result;
        }

        #endregion
    }
}
