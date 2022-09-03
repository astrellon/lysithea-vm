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
        #endregion

        #region Unity Methods
        void Update()
        {
            if (this.VM != null && this.Running)
            {
                while (this.VM.IsRunning && !this.VM.IsPaused)
                {
                    if (this.WaitUntil > 0.0f)
                    {
                        this.WaitUntil -= Time.deltaTime;
                        if (this.WaitUntil > 0.0f)
                        {
                            break;
                        }
                    }
                    else
                    {
                        this.WaitUntil = this.VMStepTiming;
                    }

                    this.VM.Step();
                    Debug.Log(string.Join("\n", this.VM.CreateStackTrace()));

                }

                if (!this.VM.IsRunning)
                {
                    this.Running = false;
                    this.OnComplete?.Invoke(this);
                }
            }
        }
        #endregion

        #region Methods
        public void Init(int stackSize, VirtualMachine.RunCommandHandler runHandler)
        {
            this.VM = new VirtualMachine(stackSize, runHandler);
        }
        #endregion
    }
}