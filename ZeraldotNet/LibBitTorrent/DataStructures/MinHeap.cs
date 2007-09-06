using System;

namespace ZeraldotNet.LibBitTorrent.DataStructures
{
    /// <summary>
    ///  最小堆
    /// </summary>
    /// <typeparam name="T">最小堆中元素的类型</typeparam>
    public class MinHeap<T> where T:IComparable<T>
    {
        #region Fields

        /// <summary>
        /// 该堆包含的元素个数
        /// </summary>
        private int count;

        /// <summary>
        /// 最大的
        /// </summary>
        private int capacity;

        /// <summary>
        /// 保存所有元素
        /// </summary>
        T[] array;

        #endregion

        #region Properties

        /// <summary>
        /// 访问该堆包含的元素个数
        /// </summary>
        public int Count
        {
            get { return this.count; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        public MinHeap() : this(4) { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity">应该调用</param>
        public MinHeap(int capacity)
        {
            this.count = 0;
            this.capacity = capacity;
            array = new T[capacity];
        }

        #endregion

        #region Methods

        /// <summary>
        /// 建立堆
        /// </summary>
        public void BuildHead()
        {
            int position;
            for (position = (this.count - 1) >> 1; position >= 0; position--)
            {
                this.MinHeapify(position);
            }
        }

        /// <summary>
        /// 增加元素
        /// </summary>
        /// <param name="item">待增加的元素</param>
        public void Add(T item)
        {
            this.count++;
            if (this.count > this.capacity)
            {
                DoubleArray();
            }
            this.array[this.count - 1] = item;
            int position = this.count - 1;

            int parentPosition = Parent(position);

            while (position > 0 && array[parentPosition].CompareTo(array[position]) > 0)
            {
                Swap(position, parentPosition);
                position = parentPosition;
                parentPosition = Parent(position);
            }
        }

        /// <summary>
        /// 使保存的元素的数量翻倍。
        /// </summary>
        private void DoubleArray()
        {
            this.capacity <<= 1;
            T[] newArray = new T[this.capacity];
            CopyArray(this.array, newArray);
            this.array = newArray;            
        }

        /// <summary>
        /// 复制数组
        /// </summary>
        /// <param name="source">待复制的数组</param>
        /// <param name="destion">复制去的数组</param>
        private static void CopyArray(T[] source, T[] destion)
        {
            int index;
            for (index = 0; index < source.Length; index++)
            {
                destion[index] = source[index];
            }
        }

        /// <summary>
        /// 返回第一个元素，但是没有删除它
        /// </summary>
        /// <returns>返回第一个元素，但是没有删除它</returns>
        public T Peek()
        {
            if (this.count == 0)
            {
                throw new InvalidOperationException("堆为空。");
            }

            return this.array[0];
        }

        /// <summary>
        /// 返回第一个元素
        /// </summary>
        /// <returns>返回第一个元素</returns>
        public T ExtractFirst()
        {
            if (this.count == 0)
            {
                throw new InvalidOperationException("堆为空。");
            }
            T result = this.array[0];
            this.array[0] = this.array[this.count - 1];
            this.count--;
            this.MinHeapify(0);
            return result;
        }

        /// <summary>
        /// 调整堆
        /// </summary>
        /// <param name="position">调整的位置</param>
        private void MinHeapify(int position)
        {
            do
            {
                //左孩子的位置
                int left = Left(position);

                //右孩子的位置
                int right = Right(position);
                int minPosition;

                //如果左孩子的位置小于元素个数并且左孩子节点比父亲节点大，最小元素的位置为左孩子
                if (left < count && array[left].CompareTo(array[position]) < 0)
                {
                    minPosition = left;
                }

                //否则最大元素的位置为父亲
                else
                {
                    minPosition = position;
                }

                //如果右孩子的位置小于元素个数并且右孩子节点比父亲节点大，最小元素的位置为右孩子
                if (right < count && array[right].CompareTo(array[minPosition]) < 0)
                {
                    minPosition = right;
                }

                //如果最大元素的位置不是父亲节点，则调整下一棵子树
                if (minPosition != position)
                {
                    Swap(position, minPosition);
                    position = minPosition;
                }

                else
                {
                    return;
                }

            } while (true);
        }

        /// <summary>
        /// 交换操作
        /// </summary>
        /// <param name="firstPosition">第一个交换的位置</param>
        /// <param name="secondPosition">第二个交换的位置</param>
        private void Swap(int firstPosition, int secondPosition)
        {
            T temp = this.array[firstPosition];
            this.array[firstPosition] = this.array[secondPosition];
            this.array[secondPosition] = temp;
        }

        /// <summary>
        /// 求解父亲节点位置
        /// </summary>
        /// <param name="position">待求解的位置</param>
        /// <returns>返回父亲节点位置</returns>
        private static int Parent(int position)
        {
            position--;
            return position >> 1;
        }

        /// <summary>
        /// 求解左孩子节点位置
        /// </summary>
        /// <param name="position">待求解的位置</param>
        /// <returns>返回左孩子节点位置</returns>
        private static int Left(int position)
        {
            position <<= 1;
            return ++position;
        }

        /// <summary>
        /// 求解右孩子节点位置
        /// </summary>
        /// <param name="position">待求解的位置</param>
        /// <returns>返回右孩子节点位置</returns>
        private static int Right(int position)
        {
            position <<= 1;
            position++;
            return ++position;
        }

        #endregion
    }
}
