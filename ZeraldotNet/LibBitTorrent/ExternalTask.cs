using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 外部任务类
    /// </summary>
    public class ExternalTask
    {
        /// <summary>
        /// 任务函数
        /// </summary>
        private TaskDelegate taskFunction;

        /// <summary>
        /// 访问和设置任务函数
        /// </summary>
        public TaskDelegate TaskFunction
        {
            get { return this.taskFunction; }
            set { this.taskFunction = value; }
        }

        /// <summary>
        /// 延迟时间
        /// </summary>
        private double delay;

        /// <summary>
        /// 访问和设置延迟时间
        /// </summary>
        public double Delay
        {
            get { return this.delay; }
            set { this.delay = value; }
        }

        /// <summary>
        /// 任务名
        /// </summary>
        private string taskName;

        /// <summary>
        /// 访问和设置任务名
        /// </summary>
        public string TaskName
        {
            get { return this.taskName; }
            set { this.taskName = value; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskFunction"></param>
        /// <param name="delay"></param>
        /// <param name="taskName"></param>
        public ExternalTask(TaskDelegate taskFunciton, double delay, string taskName)
        {
            TaskFunction = taskFunciton;
            Delay = delay;
            TaskName = taskName;
        }
    }
}
