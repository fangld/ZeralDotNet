using System;

namespace ZeraldotNet.Utility.DataStructures
{
    /// <summary>
    ///  最小堆
    /// </summary>
    /// <typeparam name="T">最小堆中元素的类型</typeparam>
    public class MinHeap<T> where T : IComparable<T>
    {
        #region Fields

        /// <summary>
        /// The capacity that is the size of array
        /// </summary>
        private int _capacity;

        /// <summary>
        /// The array that saves all elements
        /// </summary>
        T[] _array;

        #endregion

        #region Properties

        /// <summary>
        /// 访问该堆包含的元素个数
        /// </summary>
        public int Count { get; private set; }

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
            Count = 0;
            _capacity = capacity;
            _array = new T[capacity];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Build the heap
        /// </summary>
        public void BuildHeap()
        {
            int index;
            for (index = (Count - 1) >> 1; index >= 0; index--)
            {
                MinHeapify(index);
            }
        }

        /// <summary>
        /// Add the item
        /// </summary>
        /// <param name="item">The item to be added</param>
        public void Add(T item)
        {
            Count++;
            if (Count > _capacity)
            {
                _capacity <<= 1;
                Array.Resize(ref _array, _capacity);
            }
            _array[Count - 1] = item;
            int index = Count - 1;

            int parentIndex = ((index - 1) >> 1);

            while (index > 0 && _array[parentIndex].CompareTo(_array[index]) > 0)
            {
                T swapValue = _array[index];
                _array[index] = _array[parentIndex];
                _array[parentIndex] = swapValue;
                index = parentIndex;
                parentIndex = ((index - 1) >> 1);
            }
        }

        /// <summary>
        /// Peek the min element that do not remove it from the min heap
        /// </summary>
        /// <returns>The min element</returns>
        public T Peek()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Min heap is empty.");
            }

            return _array[0];
        }

        /// <summary>
        /// Extract the min element that do not remove it from the min heap
        /// </summary>
        /// <returns>The min element</returns>
        public T ExtractMin()
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("Min heap is empty.");
            }
            T result = _array[0];
            _array[0] = _array[Count - 1];
            Count--;
            MinHeapify(0);
            return result;
        }

        /// <summary>
        /// Min heapify
        /// </summary>
        /// <param name="index">调整的位置</param>
        private void MinHeapify(int index)
        {
            do
            {
                //左孩子的位置
                int left = ((index << 1) + 1);

                //右孩子的位置
                int right = left + 1;
                int minindex;

                //如果左孩子的位置小于元素个数并且左孩子节点比父亲节点大，最小元素的位置为左孩子
                if (left < Count && _array[left].CompareTo(_array[index]) < 0)
                {
                    minindex = left;
                }

                //否则最大元素的位置为父亲
                else
                {
                    minindex = index;
                }

                //如果右孩子的位置小于元素个数并且右孩子节点比父亲节点大，最小元素的位置为右孩子
                if (right < Count && _array[right].CompareTo(_array[minindex]) < 0)
                {
                    minindex = right;
                }

                //如果最大元素的位置不是父亲节点，则调整下一棵子树
                if (minindex != index)
                {
                    T temp = _array[index];
                    _array[index] = _array[minindex];
                    _array[minindex] = temp;
                    index = minindex;
                }

                else
                {
                    return;
                }

            } while (true);
        }
    }

    #endregion
}
