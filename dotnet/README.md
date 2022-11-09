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

## Assembler
The assembler parses the Lisp like code. Currently it is a bit limited in terms of debugging.
