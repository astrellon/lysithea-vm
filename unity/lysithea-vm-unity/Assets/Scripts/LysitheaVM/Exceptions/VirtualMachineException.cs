using System;
using System.Collections.Generic;

namespace LysitheaVM
{
    public class VirtualMachineException : Exception
    {
        #region Fields
        public readonly VirtualMachine VM;
        public readonly IReadOnlyList<string> VirtualMachineStackTrace;
        #endregion

        #region Constructor
        public VirtualMachineException(VirtualMachine vm, IReadOnlyList<string> virtualMachineStackTrace, string message) : base(message)
        {
            this.VM = vm;
            this.VirtualMachineStackTrace = virtualMachineStackTrace;
        }
        #endregion
    }
}