using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private List<Piece> _pieceList;

        /// <summary>
        /// The synchronized object
        /// </summary>
        private readonly object _syncObject;

        #endregion

        #region Properties

        /// <summary>
        /// Return the value of existing the next piece that can be downloaded
        /// </summary>
        public bool HaveNextPiece
        {
            get
            {
                lock (_syncObject)
                {
                    return _pieceList.Exists(p => p.ExistedNumber != 0 && !p.Downloaded);
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
                lock (_syncObject)
                {
                    return _pieceList.TrueForAll(piece => piece.Downloaded);
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
            _pieceList = new List<Piece>(piecesNumber);
            for (int i = 0; i < piecesNumber; i++)
            {
                _pieceList.Add(new Piece { Index = i, ExistedNumber = 0 });
            }

            _syncObject = new object();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the next index array of pieces
        /// </summary>
        /// <param name="peerBooleans">the booleans of pieces that peer holds</param>
        /// <param name="number">the required number</param>
        public Piece[] GetNextPieces(bool[] peerBooleans, int number)
        {
            lock (_syncObject)
            {
                List<Piece> result = new List<Piece>(number);
                List<Piece> minExistedNumberList = new List<Piece>();
                List<Piece> toBeDownloadList =
                    _pieceList.FindAll(
                        piece =>
                        !piece.Downloaded && !piece.Requested && piece.ExistedNumber > 0 && peerBooleans[piece.Index]);

                while (toBeDownloadList.Count > 0 && result.Count != number)
                {
                    int minExistedNumber = toBeDownloadList[0].ExistedNumber;
                    for (int i = 0; i < toBeDownloadList.Count; i++)
                    {
                        if (toBeDownloadList[i].ExistedNumber < minExistedNumber)
                        {
                            minExistedNumberList.Clear();
                            minExistedNumberList.Add(toBeDownloadList[i]);
                            minExistedNumber = toBeDownloadList[i].ExistedNumber;
                            continue;
                        }

                        if (toBeDownloadList[i].ExistedNumber == minExistedNumber)
                        {
                            minExistedNumberList.Add(toBeDownloadList[i]);
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
            lock (_syncObject)
            {
                Piece piece = _pieceList[index];
                piece.Downloaded = true;
            }
        }

        public bool GetDownloaded(int index)
        {
            bool result;
            lock (_syncObject)
            {
                Piece piece = _pieceList[index];
                result = piece.Downloaded;
            }
            return result;
        }

        public void AddExistedNumber(int index)
        {
            lock (_syncObject)
            {
                _pieceList[index].ExistedNumber++;
            }
        }

        public void AddExistedNumber(bool[] booleans)
        {
            lock (_syncObject)
            {
                for (int i = 0; i < booleans.Length; i++)
                {
                    if (booleans[i])
                    {
                        _pieceList[i].ExistedNumber++;
                    }
                }
            }
        }

        public void RemoveExistedNumber(bool[] booleans)
        {
            lock (_syncObject)
            {
                for (int i = 0; i < booleans.Length; i++)
                {
                    if (booleans[i])
                    {
                        _pieceList[i].ExistedNumber--;
                    }
                }
            }
        }

        public bool[] GetBooleans()
        {
            bool[] result = new bool[_pieceList.Count];
            lock (_syncObject)
            {
                Parallel.For(0, result.Length, i => result[i] = _pieceList[i].Downloaded);
            }
            return result;
        }

        #endregion
    }
}
