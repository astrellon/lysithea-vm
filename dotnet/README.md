Simple Stack Virtual Machine C#
=
The C# version of the simple stack VM. This is the original version and likely to have the best support.

Examples
-

Each example has its own `.csproj` file to prevent the `Main` methods clashing. Running each sometimes seems to need clearing the `bin` and `obj` folders as running the next one will pull up the currently built one.

Performance Test:
```sh
dotnet run --project ./perfTest.csproj
```

Dialogue Tree:
```sh
dotnet run --project ./dialogueTree.csproj
```

Run Commands Test:
```sh
dotnet run --project ./runCommands.csproj
```

The performance test definitely benefits from running in Release mode:

```sh
dotnet run -c Release --project ./perfTest.csproj
```

Assembler
-
The assembler parses the Lisp like code. Currently it is a bit limited in terms of debugging, as it tokenises using a regex which loses line number and column number.
