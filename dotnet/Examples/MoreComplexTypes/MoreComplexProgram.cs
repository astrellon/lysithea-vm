using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace LysitheaVM.Example
{
    public static class MoreComplexProgram
    {
        private static readonly Random Rand = new Random();
        private static readonly Scope CustomScope = CreateScope();
        #region Methods

        public static void Main(string[] args)
        {
            var assembler = new Assembler();
            StandardLibrary.AddToScope(assembler.BuiltinScope);
            assembler.BuiltinScope.CombineScope(CustomScope);
            var script = assembler.ParseFromFile("../../../examples/testObject.lys");

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

            result.TryConstant("done", (vm, args) =>
            {
                Console.WriteLine("Done!");
            });

            result.TryConstant("rand", (vm, args) =>
            {
                vm.PushStack(Rand.NextDouble());
            });

            result.TryConstant("newPerson", (vm, args) =>
            {
                var name = args.GetIndex<StringValue>(0);
                var age = args.GetIndex<NumberValue>(1);
                var location = args.GetIndex<ArrayValue>(2);

                var locationList = location.Value.Select(c => c.ToString()).ToList();
                vm.PushStack(new PersonValue(name.Value, age.IntValue, locationList));
            });

            result.TryConstant("newVector", (vm, args) =>
            {
                var x = args.GetIndex<NumberValue>(0);
                var y = args.GetIndex<NumberValue>(1);
                var z = args.GetIndex<NumberValue>(2);
                vm.PushStack(new VectorValue(x.FloatValue, y.FloatValue, z.FloatValue));
            });

            result.TryConstant("combinePerson", (vm, args) =>
            {
                var left = args.GetIndex<PersonValue>(0);
                var right = args.GetIndex<PersonValue>(1);

                var name = $"{left.Name} - {right.Name}";
                var age = left.Age + right.Age;
                var location = left.Address.Concat(right.Address).ToList();

                vm.PushStack(new PersonValue(name, age, location));
            });

            return result;
        }
        #endregion
    }
}