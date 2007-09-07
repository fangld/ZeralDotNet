using System;
using System.Linq;
using System.Collections.Generic;

namespace ZeraldotNet.LibBitTorrent.PiecePickers
{
    /// <summary>
    /// 片断选择器
    /// </summary>
    public class PiecePicker : IPiecePicker
    {
        #region Fields

        /// <summary>
        /// 片断的个数
        /// </summary>
        private int piecesNumber;

        /// <summary>
        /// 类型是list，它的每一项又是一个list。
        /// 例如在这个例子中，初始化的时候，interests = [ [0, 1, 2] ]，显然，它只有一项。
        /// </summary>
        private readonly List<List<int>> interests;

        /// <summary>
        /// 类型是list，每个片断对应一项，记录了每个片断收到的 have 消息的个数。
        /// 初始化的时候，numinterests = [0, 0, 0]。
        /// </summary>
        private readonly int[] interestsNumber;

        /// <summary>
        /// 变量started是用来实现“严格优先级”的
        /// </summary>
        private readonly List<int> started;

        /// <summary>
        /// 是否下载完
        /// </summary>
        private bool gotAny;

        /// <summary>
        /// 随机选择类
        /// </summary>
        private readonly Random ran;

        #endregion

        #region Properties

        /// <summary>
        /// 访问和设置片断的个数
        /// </summary>
        public int PiecesNumber
        {
            get { return this.piecesNumber; }
            set { this.piecesNumber = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="piecesNumber"></param>
        public PiecePicker(int piecesNumber)
        {
            this.piecesNumber = piecesNumber;
            this.interests = new List<List<int>>();
            interestsNumber = new int[piecesNumber];
            started = new List<int>();
            ran = new Random();

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

        #endregion

        #region Methods

        /// <summary>
        /// 收到一个have消息
        /// </summary>
        /// <param name="index">片断的索引号</param>
        public void GotHave(int index)
        {
            if (interestsNumber[index] != -1)
            {
                //n为第index个片断所收到的have信息数量
                //从interests[n]删除了第index个片断
                interests[interestsNumber[index]].Remove(index);

                //将第index个片断所收到的have信息数量 + 1
                interestsNumber[index]++;

                //如果当n + 1超过interests的长度时，那么在interests中添加新的have信息列表
                if (interestsNumber[index] == interests.Count)
                {
                    interests.Add(new List<int>());
                }

                //在interests[n + 1]添加第index个片断
                interests[interestsNumber[index]].Add(index);
            }
        }

        /// <summary>
        /// 丢失一个have消息
        /// </summary>
        /// <param name="index">片断的索引号</param>
        public void LostHave(int index)
        {
            if (interestsNumber[index] != -1)
            {
                //n为第index个片断所收到的have信息数量
                //从interests[n]删除了第index个片断
                interests[interestsNumber[index]].Remove(index);

                //将第index个片断所收到的have信息数量 - 1
                interestsNumber[index]--;

                //在interests[n - 1]添加第index个片断
                interests[interestsNumber[index]].Add(index);
            }
        }

        /// <summary>
        /// 为某个片断发送过requested消息，用于“严格优先级”策略
        /// </summary>
        /// <param name="index">片断的索引号</param>
        public void Requested(int index)
        {
            //如果started中不含有第index个片断，则将片断索引号添加到started。
            if (!started.Contains(index))
            {
                started.Add(index);
            }
        }

        /// <summary>
        /// 如果某个片断已经得到，那么调用这个函数
        /// </summary>
        /// <param name="index">片断的索引号</param>
        public void Complete(int index)
        {
            //已经接收了任意的片断
            this.gotAny = true;

            //在interests中删除piece索引号
            interests[interestsNumber[index]].Remove(index);

            //第index个片断所收到的have信息数量-1
            interestsNumber[index]--;

            //因为接收完毕，所以在started中删除第index个片断。
            started.Remove(index);
        }

        /// <summary>
        /// 计算下一个被选择的片断
        /// </summary>
        /// <param name="haveFunction">判断是否已经选择函数</param>
        /// <returns>返回选择片断的索引号</returns>
        public int Next(WantDelegate haveFunction)
        {
            if (gotAny)
            {
                return GotAnySelect(haveFunction);
            }

            else
            {
                return GotNothingSelect(haveFunction);
            }
        }

        /// <summary>
        /// 如果已经收到了某个片断，则首先采用最小优先选择策略，然后才采用严格优先选择策略
        /// </summary>
        /// <param name="haveFunction">判断是否已经选择函数</param>
        /// <returns>返回选择片断的索引号</returns>
        private int GotAnySelect(WantDelegate haveFunction)
        {
            //如果所有的收到have信息数量最小为1的片断都已经被选择，则返回-1
            //所以best初始化为-1
            int best = -1;

            //保存最小的have信息数量
            int bestNumber = int.MaxValue;

            //严格优先选择策略
            foreach (int index in started)
            {
                if (haveFunction(index) && interestsNumber[index] < bestNumber)
                {
                    best = index;
                    bestNumber = interestsNumber[index];
                }
            }

            //如果bestNumber > interests的数量，则bestNumber = interests.Count
            if (bestNumber > interests.Count)
            {
                bestNumber = interests.Count;
            }

            //最小优先选择策略
            List<int> randomPieces, interestPieces;
            for (int i = 1; i < bestNumber; i++)
            {
                randomPieces = interests[i];
                interestPieces = new List<int>(randomPieces);
                randomPieces.Clear();

                //随机选择策略
                while (interestPieces.Count > 0)
                {
                    int k = ran.Next(interestPieces.Count);
                    randomPieces.Add(interestPieces[k]);
                    interestPieces.RemoveAt(k);
                }

                foreach (int index in randomPieces)
                {
                    //如果未被选择，则返回选择的片断索引号
                    if (haveFunction(index))
                    {
                        return index;
                    }
                }
            }

            //如果不能够采用最小优先选择策略，则采用严格优先选择策略
            return best;
        }

        /// <summary>
        /// 如果没有收到了任何片断，则首先采用严格优先选择策略，然后才采用最小优先选择策略
        /// </summary>
        /// <param name="haveFunction">判断是否已经选择函数</param>
        /// <returns>返回选择片断的索引号</returns>
        private int GotNothingSelect(WantDelegate haveFunction)
        {
            int number;

            //严格优先选择策略
            foreach (int index in started)
            {
                //如果未被选择，则返回选择的片断索引号
                if (haveFunction(index))
                {
                    return index;
                }
            }

            List<int> leastPieces = new List<int>();

            //最小优先选择策略，但是要保证收到的have信息最小为1
            for (number = 1; number < interests.Count; number++)
            {
                leastPieces.AddRange(interests[number]);
            }

            //排除已经选择的片断索引号
            leastPieces.Except(started);

            //用于保存随机选择的结果
            List<int> randomPieces = new List<int>();

            //随机选择策略
            while (leastPieces.Count > 0)
            {
                int k = ran.Next(leastPieces.Count);
                randomPieces.Add(leastPieces[k]);
                leastPieces.RemoveAt(k);
            }

            foreach (int index in randomPieces)
            {
                //如果未被选择，则返回选择的片断索引号
                if (haveFunction(index))
                {
                    return index;
                }
            }

            //如果所有的收到have信息数量最小为1的片断都已经被选择，则返回-1
            return -1;
        }

        #endregion
    }
}
