namespace ZeraldotNet.LibBitTorrent
{
    /// <summary>
    /// 外部任务类
    /// </summary>
    public class ExternalTask
    {
        #region Fields

        /// <summary>
        /// 任务函数
        /// </summary>
        private TaskDelegate taskFunction;

        /// <summary>
        /// 延迟时间
        /// </summary>
        private double delay;

        /// <summary>
        /// 任务名
        /// </summary>
        private string taskName;

        #endregion 

        #region Properties

        /// <summary>
        /// 访问和设置任务函数
        /// </summary>
        public TaskDelegate TaskFunction
        {
            get { return this.taskFunction; }
            set { this.taskFunction = value; }
        }

        /// <summary>
        /// 访问和设置延迟时间
        /// </summary>
        public double Delay
        {
            get { return this.delay; }
            set { this.delay = value; }
        }

        /// <summary>
        /// 访问和设置任务名
        /// </summary>
        public string TaskName
        {
            get { return this.taskName; }
            set { this.taskName = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="taskFunction">任务函数</param>
        /// <param name="delay">延迟时间</param>
        /// <param name="taskName">任务名</param>
        public ExternalTask(TaskDelegate taskFunction, double delay, string taskName)
        {
            this.taskFunction = taskFunction;
            this.delay = delay;
            this.taskName = taskName;
        }

        #endregion
    }
}
