using System;
using LysitheaVM;

public static class Program
{
    #region Fields
    private static readonly Assembler assembler = CreateAssembler();

    private const string RuntimeErrorCode = @"
    (define num1 18)
    (define num2 3)
    (print 'Div: ' (/ num1 num2))
    (print 'Mul: ' (* num1 num3))";

    private const string ParserErrorCode = @"
    (define num1 18)
    (define num2 3))
    (print 'Div: ' (/ num1 num2))
    (print 'Mul: ' (* num1 num3)) ";

    private const string AssemblerErrorCode = @"
    (const num1 18)
    (define num1 'Redefined')
    (define num2 3)
    (print 'Div: ' (/ num1 num2))
    (print 'Mul: ' (* num1 num3))";

    #endregion

    #region Methods
    public static void Main()
    {
        var vm = new VirtualMachine(8);

        if (!TryExecute(vm, "RuntimeErrorExample", RuntimeErrorCode))
        {
            Console.WriteLine("Oh no!");
        }

        if (!TryExecute(vm, "ParserErrorExample", ParserErrorCode))
        {
            Console.WriteLine("Oh no!");
        }

        if (!TryExecute(vm, "AssemblerErrorExample", AssemblerErrorCode))
        {
            Console.WriteLine("Oh no!");
        }
    }

    private static bool TryExecute(VirtualMachine vm, string sourceName, string text)
    {
        try
        {
            var script = assembler.ParseFromText(sourceName, text);
            vm.Execute(script);
            return true;
        }
        catch (ParserException exp)
        {
            Console.WriteLine($"Parser Error: {exp.Message}\n{exp.Trace}");
        }
        catch (AssemblerException exp)
        {
            Console.WriteLine($"Assembler Error: {exp.Message}\n{exp.Trace}");
        }
        catch (VirtualMachineException exp)
        {
            Console.WriteLine($"Runtime Error: {exp.Message}\n{string.Join("\n", exp.VirtualMachineStackTrace)}");
        }

        return false;
    }

    private static Assembler CreateAssembler()
    {
        var assembler = new Assembler();
        StandardLibrary.AddToScope(assembler.BuiltinScope);
        return assembler;
    }
    #endregion
}