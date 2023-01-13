using System;
using System.Collections.Generic;
using LysitheaVM;

public static class Program
{
    #region Fields
    private static readonly Random Rand = new Random();
    private const string ScriptText = @"
    (print 'Int 0-100 ' (random.int 0 100))
    (print 'Bool ' (random.bool))
    (print 'Random Value ' (randomNumber))
    (print 'Seed ' seed)
    (print 'Seed in dynamic scope? ' (isDefined 'seed'))
    (print 'Seed in builtin scope? ' (isBuiltin 'seed'))";
    #endregion

    #region Random Methods
    private static void RandomInt(VirtualMachine vm, ArgumentsValue args)
    {
        var min = args.GetIndexInt(0);
        var max = args.GetIndexInt(1);
        vm.PushStack(Rand.Next(min, max));
    }

    private static void RandomBool(VirtualMachine vm, ArgumentsValue args)
    {
        vm.PushStack(Rand.NextDouble() > 0.5);
    }

    private static void RandomNumber(VirtualMachine vm, ArgumentsValue args)
    {
        vm.PushStack(Rand.NextDouble());
    }
    #endregion

    #region Methods
    public static void Main()
    {
        // Create an object that contains will contain functions, however this is just
        // a way to group things under a single object, kind of like a namespace, but it's only an object.
        var randomFuncs = new Dictionary<string, IValue>
        {
            {"int", new BuiltinFunctionValue(RandomInt, "random.int")},
            {"bool", new BuiltinFunctionValue(RandomBool, "random.bool")}
        };
        var randomObj = new ObjectValue(randomFuncs);

        // Create a scope to store all the values.
        var randomScope = new Scope();

        // Set the object onto the scope
        randomScope.TryDefine("random", randomObj);

        // You can also set a function directly onto the scope
        randomScope.TryDefine("randomNumber", RandomNumber);

        // You can define a simple value as well.
        randomScope.TryDefine("seed", new NumberValue(1234));

        Console.WriteLine("No Scope");
        NoScope(randomObj);

        Console.WriteLine("\nAssembler Scope");
        AssemblerScope(randomScope);

        Console.WriteLine("\nVirtual Machine Scope");
        VMScope(randomScope);
    }

    private static void NoScope(ObjectValue randomObj)
    {
        var assembler = new Assembler();
        assembler.BuiltinScope.TryDefine("random", randomObj);
        assembler.BuiltinScope.TryDefine("randomNumber", RandomNumber);
        assembler.BuiltinScope.TryDefine("seed", new NumberValue(1234));
        StandardLibrary.AddToScope(assembler.BuiltinScope);

        var vm = new VirtualMachine(8);

        var script = assembler.ParseFromText("noScopeScript", ScriptText);
        vm.Execute(script);
    }

    private static void AssemblerScope(IReadOnlyScope randomScope)
    {
        var assembler = new Assembler();
        assembler.BuiltinScope.CombineScope(randomScope);
        StandardLibrary.AddToScope(assembler.BuiltinScope);

        var vm = new VirtualMachine(8);

        var script = assembler.ParseFromText("assemblerScopeScript", ScriptText);
        vm.Execute(script);
    }

    private static void VMScope(IReadOnlyScope randomScope)
    {
        var assembler = new Assembler();
        StandardLibrary.AddToScope(assembler.BuiltinScope);

        var vm = new VirtualMachine(8);
        vm.GlobalScope.CombineScope(randomScope);

        var script = assembler.ParseFromText("assemblerScopeScript", ScriptText);
        vm.Execute(script);
    }
    #endregion
}