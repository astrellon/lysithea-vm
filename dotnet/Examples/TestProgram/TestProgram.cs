using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace SimpleStackVM.Example
{
    public static class TestProgram
    {
        private static readonly Random Rand = new Random();
        private static readonly Scope CustomScope = CreateScope();
        #region Methods
        public static void Main(string[] args)
        {
            var assembler = new VirtualMachineAssembler();
            assembler.BuiltinScope.CombineScope(StandardMathLibrary.Scope);
            assembler.BuiltinScope.CombineScope(StandardMiscLibrary.Scope);
            assembler.BuiltinScope.CombineScope(StandardOperators.Scope);
            assembler.BuiltinScope.CombineScope(CustomScope);
            var script = assembler.ParseFromText(File.ReadAllText("../../../examples/testObject.lisp"));

            var vm = new VirtualMachine(16);

            try
            {
                var sw = Stopwatch.StartNew();
                vm.Execute(script);
                sw.Stop();
                Console.WriteLine($"Time taken: {sw.Elapsed.TotalMilliseconds}ms");
            }
            catch (VirtualMachineException exp)
            {
                Console.WriteLine(exp.Message);
                var stackTrace = string.Join("", exp.VirtualMachineStackTrace.Select(t => $"\n- {t}"));
                Console.WriteLine($"VM Stack: {stackTrace}");
                Console.WriteLine(exp.StackTrace);
            }
        }

        private static Scope CreateScope()
        {
            var result = new Scope();

            result.Define("done", (vm, numArgs) =>
            {
                Console.WriteLine("Done!");
            });

            result.Define("rand", (vm, numArgs) =>
            {
                vm.PushStack(Rand.NextDouble());
            });

            result.Define("newPerson", (vm, numArgs) =>
            {
                var location = vm.PopStack<ArrayValue>();
                var age = vm.PopStack<NumberValue>();
                var name = vm.PopStack<StringValue>();
                vm.PushStack(new PersonValue(name, age, location));
            });

            result.Define("newVector", (vm, numArgs) =>
            {
                var z = vm.PopStack<NumberValue>();
                var y = vm.PopStack<NumberValue>();
                var x = vm.PopStack<NumberValue>();
                vm.PushStack(new VectorValue(x.FloatValue, y.FloatValue, z.FloatValue));
            });

            result.Define("combinePerson", (vm, numArgs) =>
            {
                var right = vm.PopStack<PersonValue>();
                var left = vm.PopStack<PersonValue>();

                var name = new StringValue($"{left.Name} - {right.Name}");
                var age = new NumberValue(left.Age.Value + right.Age.Value);
                var location = new ArrayValue(left.Address.Value.Concat(right.Address.Value).ToList());

                vm.PushStack(new PersonValue(name, age, location));
            });

            return result;
        }
        #endregion
    }
}