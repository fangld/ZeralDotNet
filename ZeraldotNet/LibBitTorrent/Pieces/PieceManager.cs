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

        private List<Piece> _undownloadedPieceList;

        private List<Piece> _requestedPieceList;

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
                    return _undownloadedPieceList.Count > 0;
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

            _undownloadedPieceList = new List<Piece>();
            for (int i = 0; i < piecesNumber; i++)
            {
                if (!booleans[i])
                {
                    _undownloadedPieceList.Add(_allPieceList[i]);
                }
            }

            _requestedPieceList = new List<Piece>();

            _synchronizedObject = new object();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the next index of pieces
        /// </summary>
        /// <pparam name="number">the required number</pparam>
        public int[] GetNextIndex(int number)
        {            
            lock (_synchronizedObject)
            {
                List<int> result = new List<int>(number);
                List<int> minExistingNumberPieceList = new List<int>();

                bool existNextIndex = false;
                
                while (_undownloadedPieceList.Count > 0 && result.Count != number)
                {
                    int minExistingNumber = int.MaxValue;
                    for (int i = 0; i < _undownloadedPieceList.Count; i++)
                    {
                        if (_undownloadedPieceList[i].ExistingNumber > 0 && _undownloadedPieceList[i].ExistingNumber < minExistingNumber)
                        {
                            minExistingNumberPieceList.Clear();
                            minExistingNumberPieceList.Add(_undownloadedPieceList[i].Index);
                            minExistingNumber = _undownloadedPieceList[i].ExistingNumber;
                            continue;
                        }

                        if (_undownloadedPieceList[i].ExistingNumber == minExistingNumber)
                        {
                            minExistingNumberPieceList.Add(_undownloadedPieceList[i].Index);
                        }
                    }

                    int remainingCount = number - result.Count;

                    if (remainingCount >= minExistingNumberPieceList.Count)
                    {
                        result.AddRange(minExistingNumberPieceList);
                    }
                    else
                    {
                        for (int i = 0; i < remainingCount; i++)
                        {
                            int randomIndex = Globals.Random.Next(minExistingNumberPieceList.Count);
                            result.Add(minExistingNumberPieceList[randomIndex]);
                            minExistingNumberPieceList.RemoveAt(randomIndex);
                        }
                    }
                    minExistingNumberPieceList.Clear();
                }

                return result.ToArray();
            }
        }

        public void SetDownload(int index)
        {
            lock (_synchronizedObject)
            {
                Piece removingPiece = _allPieceList[index];
                _undownloadedPieceList.Remove(removingPiece);
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
