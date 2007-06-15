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
        /// <summary>
        /// 片断的个数
        /// </summary>
        private int piecesNumber;

        /// <summary>
        /// 访问和设置片断的个数
        /// </summary>
        public int PiecesNumber
        {
            get { return this.piecesNumber; }
            set { this.piecesNumber = value; }
        }

        /// <summary>
        /// 类型是list，它的每一项又是一个list。
        /// 例如在这个例子中，初始化的时候，interests = [ [0, 1, 2] ]，显然，它只有一项。
        /// </summary>
        List<List<int>> interests;

        /// <summary>
        /// 类型是list，每个片断对应一项，记录了每个片断收到的 have 消息的个数。
        /// 初始化的时候，numinterests = [0, 0, 0]。
        /// </summary>
        int[] interestsNumber;

        /// <summary>
        /// 变量started是用来实现“严格优先级”的
        /// </summary>
        List<int> started;

        /// <summary>
        /// 是否下载完
        /// </summary>
        bool gotAny;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="piecesNumber"></param>
        public PiecePicker(int piecesNumber)
        {
            PiecesNumber = piecesNumber;
            this.interests = new List<List<int>>();
            interestsNumber = new int[piecesNumber];
            started = new List<int>();

            //初始化时，每个片断收到的have信息为0，并且interests保存了片断收到have信息为0。
            List<int> zeroInterest = new List<int>();
            int i;
            for (i = 0; i < interestsNumber.Length; i++)
            {
                interestsNumber[i] = 0;
                zeroInterest.Insert(i, i);
            }
            interests.Add(zeroInterest);
            this.gotAny = false;
        }

        /// <summary>
        /// 收到一个have消息的处理
        /// </summary>
        /// <param name="piece">片断的索引号</param>
        public void GotHave(int piece)
        {
            if (interestsNumber[piece] != -1)
            {
                //n为第index个片断所收到的have信息数量
                //从interests[n]删除了第index个片断
                interests[interestsNumber[piece]].Remove(piece);

                //将第index个片断所收到的have信息数量 + 1
                interestsNumber[piece]++;

                //如果当n + 1超过interests的长度时，添加新的have信息列表
                if (interestsNumber[piece] == interests.Count)
                {
                    interests.Add(new List<int>());
                }

                //在interests[n + 1]添加第index个片断
                interests[interestsNumber[piece]].Add(piece);
            }
        }

        /// <summary>
        /// 丢失一个have消息？？？？？
        /// </summary>
        /// <param name="piece">片断的索引号</param>
        public void LostHave(int piece)
        {
            if (interestsNumber[piece] != -1)
            {
                //n为第index个片断所收到的have信息数量
                //从interests[n]删除了第index个片断
                interests[interestsNumber[piece]].Remove(piece);

                //将第index个片断所收到的have信息数量 - 1
                interestsNumber[piece]--;

                //在interests[n - 1]添加第index个片断
                interests[interestsNumber[piece]].Add(piece);
            }
        }

        /// <summary>
        /// 为某个片断发送过requested消息，用于“严格优先级”策略
        /// </summary>
        /// <param name="piece">片断的索引号</param>
        public void Requested(int piece)
        {
            if (!started.Contains(piece))
            {
                //把片断索引号添加到started中
                started.Add(piece);
            }
        }

        /// <summary>
        /// 如果某个片断已经得到，那么调用这个函数
        /// </summary>
        /// <param name="piece"></param>
        public void Complete(int piece)
        {
            //已经接收了任意的片断
            this.gotAny = true;

            //在interests中删除piece索引号
            interests[interestsNumber[piece]].Remove(piece);

            //
            interestsNumber[piece]--;

            //
            started.Remove(piece);
        }

        /// <summary>
        /// 计算下一个被选择的片断
        /// </summary>
        /// <param name="haveFunction"></param>
        /// <returns></returns>
        public int Next(WantDelegate haveFunction)
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
