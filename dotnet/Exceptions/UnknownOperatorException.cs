using System;
using System.Linq;

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