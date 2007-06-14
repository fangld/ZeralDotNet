using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// Summary description for PiecePicker.
    /// </summary>
    public class PiecePicker
    {
        private int piecesNumber;

        public int PiecesNumber
        {
            get { return this.piecesNumber; }
            set { this.piecesNumber = value; }
        }

        List<List<int>> interests;
        int[] interestsNumber;
        List<int> started;
        bool gotAny;

        public PiecePicker(int piecesNumber)
        {
            PiecesNumber = piecesNumber;
            this.interests = new List<List<int>>();
            interestsNumber = new int[piecesNumber];
            started = new List<int>();
            List<int> tempInterest = new List<int>();

            int i;
            for (i = 0; i < interestsNumber.Length; i++)
            {
                interestsNumber[i] = 0;
                tempInterest.Insert(i, i);
            }
            interests.Add(tempInterest);
            this.gotAny = false;
        }

        public void GotHave(int piece)
        {
            if (interestsNumber[piece] != -1)
            {
                interests[interestsNumber[piece]].Remove(piece);
                interestsNumber[piece]++;
                if (interestsNumber[piece] == interests.Count)
                {
                    interests.Add(new List<int>());
                }
                interests[interestsNumber[piece]].Add(piece);
            }
        }

        public void LostHave(int piece)
        {
            if (interestsNumber[piece] != -1)
            {
                interests[interestsNumber[piece]].Remove(piece);
                interestsNumber[piece]--;
                interests[interestsNumber[piece]].Add(piece);
            }
        }

        public void Requested(int piece)
        {
            if (!started.Contains(piece))
            {
                started.Add(piece);
            }
        }

        public void Complete(int piece)
        {
            this.gotAny = true;
            interests[interestsNumber[piece]].Remove(piece);
            interestsNumber[piece]--;
            started.Remove(piece);
        }

        public int next(WantDelegate haveFunction)
        {
            if (gotAny)
            {
                int best = -1;
                int bestNumber = int.MaxValue;

                foreach (int i in started)
                {
                    if (haveFunction(i) && interestsNumber[i] < bestNumber)
                    {
                        best = i;
                        bestNumber = interestsNumber[i];
                    }
                }

                if (bestNumber > interests.Count)
                {
                    bestNumber = interests.Count;
                }

                for (int i = 1; i < bestNumber; i++)
                {
                    List<int> temp = interests[i];
                    Random ran = new Random();
                    List<int> t = new List<int>(temp);
                    temp.Clear();

                    while (t.Count > 0)
                    {
                        int k = ran.Next(t.Count);
                        temp.Add(t[k]);
                        t.RemoveAt(k);
                    }

                    foreach (int j in temp)
                    {
                        if (haveFunction(j))
                            return j;
                    }
                }
                return best;
            }

            else
            {
                foreach (int i in started)
                {
                    if (haveFunction(i))
                    {
                        return i;
                    }
                }

                List<int> x = new List<int>();
                for (int i = 1; i < interests.Count; i++)
                {
                    x.AddRange(interests[i]);
                }
                Random ran = new Random();
                List<int> t = new List<int>();

                while (x.Count > 0)
                {
                    int k = ran.Next(x.Count);
                    t.Add(x[k]);
                    x.RemoveAt(k);
                }

                x = t;

                foreach (int j in x)
                {
                    if (haveFunction(j))
                    {
                        return j;
                    }
                }

                return -1;
            }
        }
    }
}
