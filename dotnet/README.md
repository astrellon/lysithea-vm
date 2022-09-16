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
Currently the assembler is in a separate folder as it has a dependency on the thirdparty code `SimpleJSON`. Which means that this code will work without modification is most C# environments (.Net standard, Core, Unity and probably Mono). However each environment likely has a more suited Json parser that could be used so while the assembler is functional it should also be used an example of what to do.

It should not be much work to create another version that uses either `System.Text.Json` or `Newtonsoft.Json`.

It is unlikely that the Unity `JsonUtility` will work as the code expects to work with more dynamic structures (lists of different types and dictionaries).

Disassembler
-
This like the assembler has a dependency on `SimpleJSON` and is generally there as a debug tool or example disassembler rather than a more complete one.