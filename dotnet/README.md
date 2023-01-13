# C#

The C# version of the Lysithea VM. This is the original version and likely to have the best support.

## Examples
Each of these examples just run in the console and don't have any additional dependencies.

### Performance Test
A fairly limited performance test that does not make use of the standard library. Designed mostly just to test loops and builtin function calls. Also designed to be simple enough to be used as a very limited comparison to other scripting languages.

It doesn't *have* to be run in Release mode, but it does make a difference to the run time.

```sh
$ cd ./Examples/PerfTest
$ dotnet run -c Release
```

### Dialogue Tree
A larger example showing off a simple dialogue system.

```sh
$ cd ./Examples/DialogueTree
$ dotnet run
```

### Standard Library Test
Makes use of the standard library and runs a bunch of tests.

```sh
$ cd ./Examples/StandardLibraryTest
$ dotnet run
```

### More Complex Types
An example of some more complex types being used.

Shows off a *Vector* type and a *Person* type.

```sh
$ cd ./Examples/MoreComplexTypes
$ dotnet run
```

### Benchmark
Runs a few different benchmarks, this one does make use of dependencies in the form of **NLua**, **MoonSharp** and **BenchmarkDotNet**.

```sh
$ cd ./Examples/Benchmark
$ sudo dotnet run -c Release
```

# Building
You can either copy the source code from the `VM` folder as a whole and use it or modify it as you see fit. Or you can build the library and copy the binary into your project and add it as a reference.

```sh
dotnet/VM $ dotnet publish -c Release
```

Then you can take `lysitheaVM.dll`, `lysitheaVM.pdb` and `lysitheaVM.deps.json` from the `dotnet/VM/bin/net6.0/Release` folder and put them into your project.

# Code
Currently the code is written with a Lisp like syntax. It should not be assumed that it is Lisp or that it supports all the things that Lisp would support. Lisp was chosen for it's ease of parsing and tokenising.

```lisp
(function main ()
    (print "Result: " (+ 5 12))
)

(main)
```

This will push the `5` and `12` to the stack and then run the `+` operators, then push `"print"` to the stack and run the `call` opcode. As for `"print"` will do it up to environment that the virtual machine is running in. Ideally however the final result would print to a console `Result: 17`.

## Simple Program
To make use of Lysithea in a C# project you can either build the `VM` by following the instructions from the `Building` section above or by copying the contents of the `VM` folder into your project.

If you are starting from scratch, then you will need a project. This assumes you are using .Net 6.0 or newer.
```sh
$ dotnet new console
```

### Adding the code directly
To add the code directly you'll need the `VM` folder in your own project folder.
```
$ cp -r /path/to/VM ./
```
Or in the case of the `Examples/ReadmeExample` project:
```sh
$ cp -r ../../../VM ./
```

### Adding the source code as a reference
You can add the library code via a reference if you have it on disk
```sh
$ dotnet add reference /path/to/VM
```
Or in the case of the `Example/ReadmeExample` project:
```sh
$ dotnet add reference ../../../VM
```

### Adding the binary as a reference
You can add the library if it's already built as a `.dll` but you'll have to modify the `.csproj` file of your project. Add something like the following lines to your project.

```xml
<ItemGroup>
  <Reference Include="/path/to/VM/lysitheaVM.dll" />
</ItemGroup>
```
Or in the case of the `Example/ReadmeExample` project:
```xml
<ItemGroup>
  <Reference Include="../../../VM/bin/Release/net6.0/lysitheaVM.dll" />
</ItemGroup>
```

### The code itself
Now that we have a way to access the Lysithea VM we can actually write some code. Here is a very simple example:

```csharp
using LysitheaVM;

public static class Program
{
    #region Methods
    public static void Main()
    {
        var assembler = new Assembler();
        StandardLibrary.AddToScope(assembler.BuiltinScope);

        var script = assembler.ParseFromText("ReadmeExample", "(print 'Result ' (+ 5 12))");

        var vm = new VirtualMachine(8);
        vm.Execute(script);
    }
    #endregion
}
```

This should print to the console
```sh
Result: 17
```

## Adding Error Handling
Let's say you do want to do some error handling.

There's 2 types of exceptions:

**AssemblerException**: When parsing the tokens do things make reasonable sense, have you given the right number of arguments to certain keywords (if/unless), have you attempted to redefine a constant, is there an unexpected bracket, etc.

This will contain a `Trace` location as to where in the script this happened.

**VirtualMachineException**: These are run time errors that weren't able to be caught by the assembler.

This will be things like functions not being found, labels not found, etc.

This will contain a `VirtualMachineStackTrace` as a list of string locations at to where this occurred in the script based on function calls.

```csharp
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

    private const string UnexpectedBracketCode = @"
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
        /* Outputs
        Div: 6
        Runtime Error: Unable to get variable: num3
        at [global] in RuntimeErrorExampleRuntimeErrorExample:5:28
        4:     (print 'Div: ' (/ num1 num2))
        5:     (print 'Mul: ' (* num1 num3))
                                    ^----^

        Oh no!
        */

        if (!TryExecute(vm, "UnexpectedBracketExample", UnexpectedBracketCode))
        {
            Console.WriteLine("Oh no!");
        }

        /* Outputs
        Assembler Error: 2:17 -> 3:0: ): 2:17 -> 3:0: ): Unexpected )
        UnexpectedBracketExample:3:18
        2:     (define num1 18)
        3:     (define num2 3))
                        ^---^
        4:     (print 'Div: ' (/ num1 num2))

        Oh no!
        */

        if (!TryExecute(vm, "AssemblerErrorExample", AssemblerErrorCode))
        {
            Console.WriteLine("Oh no!");
        }

        /* Outputs
        Assembler Error: 2:12 -> 2:17: num1: Attempting to Define a constant: num1
        AssemblerErrorExample:3:13
        2:     (const num1 18)
        3:     (define num1 'Redefined')
                    ^----^
        4:     (define num2 3)

        Oh no!
        */
    }

    private static bool TryExecute(VirtualMachine vm, string sourceName, string text)
    {
        try
        {
            var script = assembler.ParseFromText(sourceName, text);
            vm.Execute(script);
            return true;
        }
        catch (AssemblerException exp)
        {
            Console.WriteLine($"Assembler Error: {exp.Message}\n{exp.Trace}");
        }
        catch (VirtualMachineException exp)
        {
            Console.WriteLine($"Runtime Error: {exp.Message}\n{string.Join("\n", exp.VirtualMachineStackTrace)}");
        }
        catch (Exception exp)
        {
            Console.WriteLine($"Unexpected Error: {exp.Message}\n{exp.StackTrace}");
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
```

## Adding Extra Functionality
So by default the VM only supports basic arithmetic (`+`, `-`, `*`, `/`) and comparisons (`<`, `>`, `>=`, `<=`, `==`, `!=`). The standard library gives you some more functionality around manipulating some of the builtin types, like strings, lists and maps but that still doesn't let you do much.

So in general you'll be adding extra functionality to actually do something for your specific program.

There's two main ways of doing this, adding extra functions and values to the assembler and adding extra functions and values to the virtual machine.

The main difference being that when done at the assembler stage it lets it inline calls to those functions increasing performance and it means that even if the virtual machine in unaware of those function it won't need to, unless you want to dynamically find those functions.

Additionally this extra functionality can be grouped together into a `scope` which can then be combined with the assembler or virtual machines scope. Or you can define individual values.

### Example

In this example the `isDefined` function checks if there is a variable with that name found anywhere in the accessible scope; either current scope or parent scope up to global *but* not the builtin scope to the script!

There is also an `isBuiltin` function which checks if there is a builtin constant with that name. This **only** works for global constants, as function level constants are not stored anywhere outside of the inlining.

Here is a somewhat lengthy example of different ways of adding extra functionality. Property error handling has been left out for brevity.

```csharp
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
    (print 'Defined ' (isDefined seed))
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

        /* Output
        No Scope
        Int 0-100 56
        Bool true
        Random Value 0.629341375957468
        Seed 1234
        Seed in dynamic scope? false
        Seed in builtin scope? true
        */

        Console.WriteLine("\nAssembler Scope");
        AssemblerScope(randomScope);

        /* Output
        Assembler Scope
        Int 0-100 40
        Bool true
        Random Value 0.3844478389897984
        Seed 1234
        Seed in dynamic scope? false
        Seed in builtin scope? true
        */

        Console.WriteLine("\nVirtual Machine Scope");
        VMScope(randomScope);

        /* Output
        Virtual Machine Scope
        Int 0-100 2
        Bool true
        Random Value 0.04099879684714747
        Seed 1234
        Seed in dynamic scope? true
        Seed in builtin scope? false
        */
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
```

### Detailed Comparison of Scope Usage
Here is how the code looks like after being assembled with the `NoScope` and `AssemblerScope`, basically if the assembler is able to directly find the values it needs.

As you can see there are fewer operators needed as there is less looking up of values at run time.

Both the standard library call to `print` is inlined as well as the calls to the `random` object and it's values `random.int` and `random.bool` and the value of `seed` is inlined as well.

**Note!** This does mean that anything known at assembler time is baked in and changing those values at run time will not be seen! You cannot change the value of `seed` for example if it is known at assembler time. To do that sort of thing you can simply leave it out of the assembler's `BuiltinScope`.

Both these tables leave out the operators for calling `isDefined` and `isBuiltin` for brevity and they are the same between both tables.

| Operator | Argument | Current Stack |
| -------- | -------- | --- |
| `push` | "Int 0-100 " | "Int 0-100 " |
| `push` | 0 | "Int 0-100 ", 0 |
| `push` | 100 | "Int 0-100", 0, 100 |
| `callDirect` | [random.int, 2] | "Int 0-100 ", 64 (some random number) |
| `callDirect` | [print, 2] | *empty* |
| `push` | "Bool " | "Bool " |
| `callDirect` | [random.bool, 0] | "Bool ", false (some random bool) |
| `callDirect` | [print, 2] | *empty* |
| `push` | "Random Value " | "Random Value " |
| `callDirect` | [randomNumber, 0] | "Random Value ", 0.1234 (some random number) |
| `callDirect` | [print, 2] | *empty* |
| `push` | "Seed " | "Seed " |
| `push` | 1234 | "Seed ", 1234 |
| `callDirect` | [print, 2] | *empty* |

Here is what the `VMScope` example code looks like, it contains `get` and `getProperty` calls to find the `random` object and then find function before it can call it. This does not affect the assemblers ability to inline calls to the `print` function from the standard library.

**Note!** This does give you the ability to load up functions dynamically, change values at run time and the script doesn't need to be assembled again. This mostly has an impact on performance which may or may not be an issue for your situation.

| Operator | Argument | Current Stack |
| -------- | -------- | --- |
| `push` | "Int 0-100 " | "Int 0-100 " |
| `push` | 0 | "Int 0-100 ", 0 |
| `push` | 100 | "Int 0-100 ", 0, 100 |
| `get` | random | "Int 0-100 ", 0, 100, {object: random} |
| `getProperty` | int | "Int 0-100 ", 0, 100, {function: random.int} |
| `call` | 2 | "Int 0-100 ", 32 (some random number) |
| `callDirect` | [print, 2] | *empty* |
| `push` | "Bool " | "Bool " |
| `get` | random | "Bool ", {object: random} |
| `getProperty` | bool | "Bool ", {function: random.bool} |
| `call` | 0 | "Bool ", true (a random bool) |
| `callDirect` | [print, 2] | *empty* |
| `push` | Random Value | "Random Value " |
| `get` | randomNumber | "Random Value ", {function: randomNumber} |
| `call` | 0 | "Random Value ", 0.543 (some random number) |
| `callDirect` | [print, 2] | *empty* |
| `push` | "Seed " | "Seed " |
| `get` | seed | "Seed", 1234 |
| `callDirect` | [print, 2] | *empty* |

## More details
For more details, see the main [repositories documentation](https://github.com/astrellon/lysithea-vm).

## License
MIT

## Author
Alan Lawrey 2023