using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeraldotNet.LibBitTorrent.Pieces
{
    public class PieceManager
    {
        #region Fields

        /// <summary>
        /// The list of pieces
        /// </summary>
        private List<Piece> _allPieceList;

        /// <summary>
        /// The synchronized object
        /// </summary>
        private readonly object _synchronizedObject;

        #endregion

        #region Properties

        public bool HaveNextPiece
        {
            get
            {
                lock (_synchronizedObject)
                {
                    return _allPieceList.Exists(p => p.ExistingNumber != 0 && !p.Downloaded && !p.Downloaded);
                }
            }
        }

        public bool AllDownloaded
        {
            get
            {
                lock (_synchronizedObject)
                {
                    return _allPieceList.TrueForAll(piece => piece.Downloaded);
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
            _allPieceList = new List<Piece>(piecesNumber);
            for (int i = 0; i < piecesNumber; i++)
            {
                _allPieceList.Add(new Piece { Index = i, ExistingNumber = 0 });
            }

            _synchronizedObject = new object();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the next index of pieces
        /// </summary>
        /// <pparam name="number">the required number</pparam>
        public Piece[] GetNextIndex(int number)
        {            
            lock (_synchronizedObject)
            {
                List<Piece> result = new List<Piece>(number);
                List<Piece> minExistingNumberPieceList = new List<Piece>();
                List<Piece> toBeDownloadPieceList = _allPieceList.FindAll(piece => !piece.Downloaded && !piece.Requested && piece.ExistingNumber > 0);

                while (toBeDownloadPieceList.Count > 0 && result.Count != number)
                {
                    int minExistingNumber = toBeDownloadPieceList[0].ExistingNumber;
                    for (int i = 0; i < toBeDownloadPieceList.Count; i++)
                    {
                        if (toBeDownloadPieceList[i].ExistingNumber < minExistingNumber)
                        {
                            minExistingNumberPieceList.Clear();
                            minExistingNumberPieceList.Add(toBeDownloadPieceList[i]);
                            minExistingNumber = toBeDownloadPieceList[i].ExistingNumber;
                            continue;
                        }

                        if (toBeDownloadPieceList[i].ExistingNumber == minExistingNumber)
                        {
                            minExistingNumberPieceList.Add(toBeDownloadPieceList[i]);
                        }
                    }

                    int remainingCount = number - result.Count;

                    if (remainingCount >= minExistingNumberPieceList.Count)
                    {
                        result.AddRange(minExistingNumberPieceList);
                        Parallel.ForEach(minExistingNumberPieceList, piece => piece.Requested = true);
                    }
                    else
                    {
                        for (int i = 0; i < remainingCount; i++)
                        {
                            int randomIndex = Globals.Random.Next(minExistingNumberPieceList.Count);
                            result.Add(minExistingNumberPieceList[randomIndex]);
                            minExistingNumberPieceList[randomIndex].Requested = true;
                            minExistingNumberPieceList.RemoveAt(randomIndex);
                        }
                    }
                    minExistingNumberPieceList.Clear();
                }

                return result.ToArray();

                //do
                //{
                //    int minExistingNumber = int.MaxValue;
                //    for (int i = 0; i < _allPieceList.Count; i++)
                //    {
                //        if (_allPieceList[i].ExistingNumber > 0 && _allPieceList[i].ExistingNumber < minExistingNumber && !_allPieceList[i].Requested)
                //        {
                //            minExistingNumberPieceList.Clear();
                //            minExistingNumberPieceList.Add(_allPieceList[i]);
                //            minExistingNumber = _allPieceList[i].ExistingNumber;
                //            continue;
                //        }

                //        if (_undownloadedPieceList[i].ExistingNumber == minExistingNumber && !_undownloadedPieceList[i].Requested)
                //        {
                //            minExistingNumberPieceList.Add(_undownloadedPieceList[i]);
                //        }
                //    }
                //} while (true);

                //List<Piece> result = new List<Piece>(number);
                //List<Piece> minExistingNumberPieceList = new List<Piece>();

                //bool existNextIndex = false;

                //while (_undownloadedPieceList.Count > 0 && result.Count != number)
                //{
                //    int minExistingNumber = int.MaxValue;
                //    for (int i = 0; i < _undownloadedPieceList.Count; i++)
                //    {
                //        if (_undownloadedPieceList[i].ExistingNumber > 0 && _undownloadedPieceList[i].ExistingNumber < minExistingNumber && !_undownloadedPieceList[i].Requested)
                //        {
                //            minExistingNumberPieceList.Clear();
                //            minExistingNumberPieceList.Add(_undownloadedPieceList[i]);
                //            minExistingNumber = _undownloadedPieceList[i].ExistingNumber;
                //            continue;
                //        }

                //        if (_undownloadedPieceList[i].ExistingNumber == minExistingNumber && !_undownloadedPieceList[i].Requested)
                //        {
                //            minExistingNumberPieceList.Add(_undownloadedPieceList[i]);
                //        }
                //    }

                //    int remainingCount = number - result.Count;

                //    if (remainingCount >= minExistingNumberPieceList.Count)
                //    {
                //        result.AddRange(minExistingNumberPieceList);
                //        Parallel.ForEach<Piece>(minExistingNumberPieceList, piece => piece.Requested = true);
                //    }
                //    else
                //    {
                //        for (int i = 0; i < remainingCount; i++)
                //        {
                //            int randomIndex = Globals.Random.Next(minExistingNumberPieceList.Count);
                //            result.Add(minExistingNumberPieceList[randomIndex]);
                //            minExistingNumberPieceList[randomIndex].Requested = true;
                //            minExistingNumberPieceList.RemoveAt(randomIndex);
                //        }
                //    }
                //    minExistingNumberPieceList.Clear();
                //}

                ////result.AddRange(_requestedPieceList);
                //_requestedPieceList.AddRange(result);
                //return result.ToArray();
            }
        }

        public void SetDownloaded(int index)
        {
            lock (_synchronizedObject)
            {
                Piece piece = _allPieceList[index];
                piece.Downloaded = true;
                //_undownloadedPieceList.Remove(removingPiece);
                //removingPiece.Requested = true;
                //_requestedPieceList.Remove(removingPiece);
            }
        }

        public void AddExistingNumber(int index)
        {
            lock (_synchronizedObject)
            {
                _allPieceList[index].ExistingNumber++;
            }
        }

        public void AddExistingNumber(bool[] booleans)
        {
            lock (_synchronizedObject)
            {
                for (int i = 0; i < booleans.Length; i++)
                {
                    if (booleans[i])
                    {
                        _allPieceList[i].ExistingNumber++;
                    }
                }
            }
        }

        public void RemoveExistingNumber(bool[] booleans)
        {
            lock (_synchronizedObject)
            {
                for (int i = 0; i < booleans.Length; i++)
                {
                    if (booleans[i])
                    {
                        _allPieceList[i].ExistingNumber--;
                    }
                }
            }
        }

        #endregion
    }
}
