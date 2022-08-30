using System;
using System.Collections.Generic;

namespace SimpleStackVM
{
    public class ScopeException : VirtualMachineException
    {
        #region Constructor
        public ScopeException(IReadOnlyList<string> virtualMachineStackTrace, string message) : base(virtualMachineStackTrace, message)
        {

        }
        #endregion
    }
}