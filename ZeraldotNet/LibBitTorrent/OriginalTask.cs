using System;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 原有项目的任务类
    /// </summary>
    public class OriginalTask : IComparable<OriginalTask>
    {
        #region Fields

        /// <summary>
        /// 任务的执行函数
        /// </summary>
        private TaskDelegate taskFunction;

        /// <summary>
        /// 任务的起始时间
        /// </summary>
        private DateTime when;

        #endregion

        #region Properties

        /// <summary>
        /// 访问和设置任务的执行函数
        /// </summary>
        public TaskDelegate TaskFunction
        {
            get { return this.taskFunction; }
            set { this.taskFunction = value; }
        }

        /// <summary>
        /// 访问和设置任务的起始时间
        /// </summary>
        public DateTime When
        {
            get { return this.when; }
            set { this.when = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskFunction"></param>
        /// <param name="when">任务的起始时间</param>
        public OriginalTask(TaskDelegate taskFunction, DateTime when)
        {
            this.taskFunction = taskFunction;
            this.when = when;
        }

        #endregion

        #region Overridden Method

        #region IComparable<OriginalTask> Members

        public int CompareTo(OriginalTask other)
        {
            return when.CompareTo(other.When);
        }

        #endregion

        #endregion
    }
}
