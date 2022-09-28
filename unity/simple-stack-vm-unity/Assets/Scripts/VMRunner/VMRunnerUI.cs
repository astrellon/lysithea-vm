using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleStackVM.Unity
{
    public class VMRunnerUI : MonoBehaviour
    {
        public VMRunner VMRunner;

        public VirtualMachine VM => this.VMRunner.VM;
    }
}
