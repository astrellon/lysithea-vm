using System;
using System.Collections.Generic;

namespace SimpleStackVM
{
    public class OperatorException : VirtualMachineException
    {
        #region Constructor
        public OperatorException(IReadOnlyList<string> virtualMachineStackTrace, string message) : base(virtualMachineStackTrace, message)
        {

        }
        #endregion
    }
}