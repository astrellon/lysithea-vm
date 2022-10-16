using System;
using System.Collections.Generic;

namespace SimpleStackVM
{
    public class UnknownOperatorException : VirtualMachineException
    {
        #region Constructor
        public UnknownOperatorException(IReadOnlyList<string> virtualMachineStackTrace, string message) : base(virtualMachineStackTrace, message)
        {

        }
        #endregion
    }
}