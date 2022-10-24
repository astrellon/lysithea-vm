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
            using var file = File.OpenRead("../../../examples/testObject.lisp");
            using var reader = new StreamReader(file);
            var result = VirtualMachineStreamParser.ReadAllTokens(reader);
        }

        public static void MainOld(string[] args)
        {
            var assembler = new VirtualMachineAssembler();
            StandardLibrary.AddToScope(assembler.BuiltinScope);
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

            result.Define("done", (vm, args) =>
            {
                Console.WriteLine("Done!");
            });

            result.Define("rand", (vm, args) =>
            {
                vm.PushStack(Rand.NextDouble());
            });

            result.Define("newPerson", (vm, args) =>
            {
                var name = args.Get<StringValue>(0);
                var age = args.Get<NumberValue>(1);
                var location = args.Get<ArrayValue>(2);
                vm.PushStack(new PersonValue(name, age, location));
            });

            result.Define("newVector", (vm, args) =>
            {
                var x = args.Get<NumberValue>(0);
                var y = args.Get<NumberValue>(1);
                var z = args.Get<NumberValue>(2);
                vm.PushStack(new VectorValue(x.FloatValue, y.FloatValue, z.FloatValue));
            });

            result.Define("combinePerson", (vm, args) =>
            {
                var left = args.Get<PersonValue>(0);
                var right = args.Get<PersonValue>(1);

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