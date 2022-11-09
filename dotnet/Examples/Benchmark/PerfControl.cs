using System;
using System.Linq;

namespace LysitheaVM
{
    public class PerfControl
    {
        #region Fields
        private static readonly Random Rand = new Random();
        private int counter;
        private double total;
        #endregion

        #region Constructor
        public PerfControl()
        {

        }
        #endregion

        #region Methods
        public void Run()
        {
            while (!DoIsDone())
            {
                this.DoStep();
            }
            Console.WriteLine($"Done: {this.total}");
        }
        private void DoStep()
        {
            var num1 = Rand.NextDouble();
            var num2 = Rand.NextDouble();
            this.total += num1 + num2;
        }

        private bool DoIsDone()
        {
            this.counter++;
            return this.counter >= 1_000_000;
        }
        #endregion
    }
}