using System.Collections.Generic;
using ZeraldotNet.LibBitTorrent;

namespace ZeraldotNet.UnitTest.TestLibBitTorrent.TestChokers
{
    public class DummyScheduler
    {
        private readonly List<TaskDelegate> functions;

        private readonly List<double> delays;

        public TaskDelegate GetFunction(int index)
        {
            return this.functions[index];
        }

        public double GetDelay(int index)
        {
            return this.delays[index];
        }

        public int FunctionCount
        {
            get { return this.functions.Count; }
        }

        public void RemoveAt(int index)
        {
            this.delays.RemoveAt(index);
            this.functions.RemoveAt(index);
        }

        public DummyScheduler()
        {
            functions = new List<TaskDelegate>();
            delays = new List<double>();
        }

        public void Call(TaskDelegate function, double delay, string taskName)
        {
            functions.Add(function);
            delays.Add(delay);
        }
    }
}