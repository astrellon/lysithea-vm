using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LysitheaVM
{
    public static class StandaloneProgram
    {
        #region Methods
        public static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Requires input file.");
                return 1;
            }

            var assembler = new Assembler();
            StandardLibrary.AddToScope(assembler.BuiltinScope);
            var script = assembler.ParseFromFile(args[0]);

            var vm = new VirtualMachine(64);

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
                return 2;
            }

            return 0;
        }
        #endregion
    }
}