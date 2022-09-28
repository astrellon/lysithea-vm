using System;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class VMRunner : MonoBehaviour
    {
        #region Fields
        public delegate void CompleteHandler(VMRunner runner);

        public event CompleteHandler OnComplete;

        public VirtualMachine VM { get; private set; }
        public bool Running;

        public float WaitUntil = -1.0f;
        public float VMStepTiming = -1.0f;

        public bool IsWaiting => this.WaitUntil > 0.0f;
        #endregion

        #region Unity Methods
        void Update()
        {
            if (this.VM != null && this.Running)
            {
                var runOnce = false;
                while (this.VM.Running && !this.VM.Paused)
                {
                    if (this.IsWaiting)
                    {
                        this.WaitUntil -= Time.deltaTime;
                        if (runOnce || this.WaitUntil > 0.0f)
                        {
                            break;
                        }
                        else
                        {
                            this.WaitUntil = this.VMStepTiming;
                        }
                    }
                    else
                    {
                        this.WaitUntil = this.VMStepTiming;
                    }

                    runOnce = true;

                    this.VM.Step();

                }

                if (!this.VM.Running)
                {
                    this.Running = false;
                    this.OnComplete?.Invoke(this);
                }
            }
        }
        #endregion

        #region Methods
        public void Init(int stackSize, Action<string, VirtualMachine> runHandler)
        {
            this.VM = new VirtualMachine(stackSize, runHandler);
        }

        public void Wait(TimeSpan timespan)
        {
            this.WaitUntil = (float)timespan.TotalSeconds;
        }
        #endregion
    }
}