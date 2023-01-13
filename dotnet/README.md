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