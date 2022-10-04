using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace SimpleStackVM
{
    public static class TestProgram
    {
        #region Methods
        public static void Main(string[] args)
        {
            var file = File.ReadAllText("../examples/perfTest.lisp");
            var tokens = VirtualMachineLispParser.Tokenize(file);
            var parsed = VirtualMachineLispParser.ReadAllTokens(tokens);

            var code = VirtualMachineLispAssembler.Parse(parsed);
        }
        #endregion
    }
}