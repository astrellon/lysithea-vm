using System;
using System.Collections.Generic;

namespace SimpleStackVM
{
    public class StackException : VirtualMachineException
    {
        #region Constructor
        public StackException(IReadOnlyList<string> virtualMachineStackTrace, string message) : base(virtualMachineStackTrace, message)
        {

        }
        #endregion
    }
}