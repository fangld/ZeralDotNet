using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 任务类
    /// </summary>
    public class Task : IComparable<Task>
    {
        /// <summary>
        /// 任务的执行函数
        /// </summary>
        private TaskDelegate taskFunction;

        /// <summary>
        /// 访问和设置任务的执行函数
        /// </summary>
        public TaskDelegate TaskFunction
        {
            get { return this.taskFunction; }
            set { this.taskFunction = value; }
        }

        /// <summary>
        /// 任务的起始时间
        /// </summary>
        private DateTime when;

        /// <summary>
        /// 访问和设置任务的起始时间
        /// </summary>
        public DateTime When
        {
            get { return this.when; }
            set { this.when = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskFunction"></param>
        /// <param name="when">任务的起始时间</param>
        public Task(TaskDelegate taskFunction, DateTime when)
        {
            this.taskFunction = taskFunction;
            this.when = when;
        }

        #region IComparable<Task> Members

        public int CompareTo(Task other)
        {
            return when.CompareTo(other.When);
        }

        #endregion
    }
}
