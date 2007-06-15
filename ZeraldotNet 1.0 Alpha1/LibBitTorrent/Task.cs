using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    public class Task : IComparable<Task>
    {
        private TaskDelegate taskFunction;

        public TaskDelegate TaskFunction
        {
            get { return this.taskFunction; }
            set { this.taskFunction = value; }
        }

        private DateTime when;

        public DateTime When
        {
            get { return this.When; }
            set { this.When = value; }
        }

        public Task(TaskDelegate taskFunction, DateTime when)
        {
            TaskFunction = taskFunciton;
            When = when;
        }

        #region IComparable<Task> Members

        public int CompareTo(Task other)
        {
            return when.CompareTo(other.When);
        }

        #endregion
    }
}
