using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleStackVM
{
    public class VirtualMachineException : Exception
    {
        #region Fields
        public readonly IReadOnlyList<string> VirtualMachineStackTrace;
        #endregion

        #region Constructor
        public VirtualMachineException(IReadOnlyList<string> virtualMachineStackTrace, string message) : base(message)
        {
            this.VirtualMachineStackTrace = virtualMachineStackTrace;
        }
        #endregion
    }
}