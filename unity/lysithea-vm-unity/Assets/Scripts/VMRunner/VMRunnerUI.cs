using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LysitheaVM.Unity
{
    public class VMRunnerUI : MonoBehaviour
    {
        public VMRunner VMRunner;

        public VirtualMachine VM => this.VMRunner.VM;

        public void TogglePaused()
        {
            this.VM.Paused = !this.VM.Paused;
        }
    }
}
