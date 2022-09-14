Simple Stack Virtual Machine
=

This repository contains code for a simple stack based virtual machine. It is expected that it in embedded in another program which will provide the real functionality. This machine is very simple and very general, it contains only the code necessary to push and pop from the main stack, jump to labels, jump with condition, call labels like functions and return from a call. The final command is to run an external command through a handler.

This is built it contains no math functions, string manipulation or anything, but the values the stack support is:
- Null: An empty value, not really intended to be pushed around, but if a command does not have a value (because the operator is taking a value from the stack) then the code line value will be null.
- Strings: Based off the standard string type from the host programming language. But it is generally based off the C# model which means immutable and UTF-8.
- Boolean: True/False
- Number: 64 bit double.
- Object/Dictionary/Map: Key value pair with string keys and the value being another valid VM value.
- List/Array: A list of VM values.
- Any: This is somewhat implementation dependant, but an extra value that lets you push any kind of value onto the stack, however if a valid bool, string, or number is pushed as a number it is not expected that comparing

Code:
-

Currently the code is written in JSON. This skips the need for specialised grammar or lexer, plus it gives the ability to load complex objects and arrays 'for free'.

```json
[
    {
        "name": "Main",
        "data": [
            ["Push", 5],
            ["Push", 12],
            ["Push", "add"],
            "Run",
            ["Push", "print"],
            "Run"
        ]
    }
]
```

This will push the `5`, `12` and `"add"` to the stack, run the command at the top of the stack (`"add"`), then push `"print"` to the stack and call run again. As for what `"add"` and `"print"` will do it up to environment that the virtual machine is running in. Ideally however the final result would print to a console the number `17`.

Here is an example of a run command handler for the above program in C#:
```csharp
private static void OnRunCommand(IValue command, VirtualMachine vm)
{
    var commandName = command.ToString();
    if (commandName == "add")
    {
        var num1 = vm.PopStack<NumberValue>();
        var num2 = vm.PopStack<NumberValue>();
        vm.PushStack((NumberValue)(num1.Value + num2.Value));
        return;
    }
    if (commandName == "print")
    {
        var total = vm.PopStack();
        Console.WriteLine($"Print: {total.ToString()}");
    }
}
```

The program will output

```
Print: 17
```

Labels:
-

Labels are used to let you jump around the code, optionally based on some condition.

```json
[
    {
        "name": "Main",
        "data": [
            ["Push", 0],

            ":Start",

            ["Push", "inc"],
            "Run",
            ["Push", "isDone"],
            "Run",

            ["JumpFalse", ":Start"],

            ["Push", "done"],
            "Run"
        ]
    }
]
```

Run Command Shorthand:
-

The previous examples show that calling the run command is a bit cumbersome, along with pushing values that are likely to be used by a run command.

So any first argument in a line that isn't an operator ("Push", "Run", "Jump", "JumpTrue", "JumpFalse", "Call", "Return") or a label (must start with a colon :) will assume to be a run command.

Additionally extra lines after any command are assumed to be pushed to the stack except with the final argument being tied to the command line itself.

For example, each add and then print section are equivalent:

```json
[
    {
        "name": "Main",
        "data": [
            ["Push", 5],
            ["Push", 12],
            ["Push", "add"],
            "Run",              // Takes the command to run from the stop of the stack,
                                // which means it could be any kind of value (not just a string).
            ["Push", "print"],
            "Run",              // Prints the top: 17


            ["Push", 5],
            ["Push", 12],
            ["Run", "add"],     // Same as above but the value is tied to the code line itself (not just a string).
            ["Run", "print"],   // Prints the top: 17


            ["Push", 5, 12],
            "add",              // Takes the top two stack items and pushes the added result back to the stack
            "print",             // Prints the top: 17


            ["add", 5, 12],     // As add is not a known operator it is assumed that it should be a run command with the code line value "add"
            "print"             // Prints the top: 17
        ]
    }
]
```

A more complex example about a dialogue tree can be found under `dotnet/DialogueTreeProcess.cs`.