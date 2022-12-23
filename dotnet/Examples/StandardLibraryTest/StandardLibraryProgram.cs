using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LysitheaVM
{
    public static class StandardLibraryProgram
    {
        #region Methods
        public static void Main(string[] args)
        {
            var assembler = new Assembler();
            StandardLibrary.AddToScope(assembler.BuiltinScope);
            assembler.BuiltinScope.CombineScope(StandardAssertLibrary.Scope);
            var script = assembler.ParseFromFile("../../../examples/testStandardLibrary.lys");

            var vm = new VirtualMachine(8);

            try
            {
                var sw = Stopwatch.StartNew();
                vm.Execute(script);
                sw.Stop();
                Console.WriteLine($"Time taken: {sw.Elapsed.TotalMilliseconds} ms");
            }
            catch (VirtualMachineException exp)
            {
                Console.WriteLine(exp.Message);
                var stackTrace = string.Join("", exp.VirtualMachineStackTrace.Select(t => $"\n- {t}"));
                Console.WriteLine($"VM Stack: {stackTrace}");
                Console.WriteLine(exp.StackTrace);
            }
        }
        #endregion
    }
}