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

        #region Constructors

        /// <summary>
        /// Construct the new piece manager
        /// </summary>
        /// <param name="piecesNumber">The number of pieces</param>
        public PieceManager(int piecesNumber)
        {
            _allPieceList = new List<Piece>(piecesNumber);
            for (int i = 0; i < piecesNumber; i++)
            {
                _allPieceList.Add(new Piece { Index = i, ExistingNumber = 0 });
            }

            _undownloadedPieceList = new List<Piece>();

            _requestedPieceList = new List<Piece>();

            _synchronizedObject = new object();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the next index of pieces
        /// </summary>
        public int GetNextIndex()
        {
            int result = -1;
            lock (_synchronizedObject)
            {
                if (_undownloadedPieceList.Count > 0)
                {
                    int minExistingNumber = int.MaxValue;
                    List<int> minExistingNumberPieceList = new List<int>();
                    for (int i = 0; i < _undownloadedPieceList.Count; i++)
                    {
                        if (_undownloadedPieceList[i].ExistingNumber > 0 && _undownloadedPieceList[i].ExistingNumber < minExistingNumber)
                        {
                            minExistingNumberPieceList.Clear();
                            minExistingNumberPieceList.Add(_undownloadedPieceList[i].Index);
                            continue;
                        }

                        if (_undownloadedPieceList[i].ExistingNumber == minExistingNumber)
                        {
                            minExistingNumberPieceList.Add(_undownloadedPieceList[i].Index);
                        }
                    }

                    if (minExistingNumberPieceList.Count > 0)
                    {
                        int randomIndex = Globals.Random.Next();
                        result = minExistingNumberPieceList[randomIndex];
                    }
                }
            }
            return result;
        }

        //public void SetDownload(int index)
        //{
        //    lock (_synchronizedObject)
        //    {
        //    }
        //}

        public void AddExistingNumber(int index)
        {
            lock (_synchronizedObject)
            {
                _allPieceList[i].ExistingNumber++;
            }
        }

        public void AddExistingNumber(bool[] booleans)
        {
            lock (_synchronizedObject)
            {
                for (int i = 0; i < booleans.Count; i++)
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
                for (int i = 0; i < booleans.Count; i++)
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
