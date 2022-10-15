Simple Stack Virtual Machine
=

This repository contains code for a simple stack based virtual machine. It is expected that it in embedded in another program which will provide the real functionality. This machine is very simple and very general, it contains only the code necessary to push and pop from the main stack, jump to labels, jump with condition, call labels like functions and return from a call. The final command is to run an external command through a handler.

This is built it contains no math functions, string manipulation or anything, but the values the stack support is:
- Null: An empty value, not really intended to be pushed around, but if a command does not have a value (because the operator is taking a value from the stack) then the code line value will be null.
- Strings: Based off the standard string type from the host programming language. But it is generally based off the C# model which means immutable and UTF-8.
- Boolean: true/false
- Number: 64 bit double.
- Object/Dictionary/Map: Key value pair with string keys and the value being another valid VM value.
- List/Array: A list of VM values.
- Function: A collection of code put into a value.
- BuiltinFunction: A reference to a builtin function that can be passed around like a value.
- Any: This is somewhat implementation dependant, but an extra value that lets you push any kind of value onto the stack, however if a valid bool, string, or number is pushed as a number it is not expected that comparing

Code:
-

Currently the code is written with a Lisp like syntax. It should not be assumed that it is Lisp or that it supports all the things that Lisp would support. Lisp was chosen for it's ease of parsing and tokenising.

```lisp
(define main (function ()
    (print (add 5 12))
))

(main)
```

This will push the `5`, `12` and `"add"` to the stack, run the command at the top of the stack (`"add"`), then push `"print"` to the stack and call run again. As for what `"add"` and `"print"` will do it up to environment that the virtual machine is running in. Ideally however the final result would print to a console the number `17`.

Here is an example of a run command handler for the above program in C#:
```csharp
private static Scope CreateScope()
{
    var result = new Scope();

    result.Set("add", vm =>
    {
        var num1 = vm.PopStack<NumberValue>();
        var num2 = vm.PopStack<NumberValue>();
        vm.PushStack((NumberValue)(num1.Value + num2.Value));
    });

    result.Set("print", vm =>
    {
        var top = vm.PopStack();
        Console.WriteLine($"Print: {top.ToString()}");
    });

    return result;
}
```

The program will output

```
Print: 17
```

Labels:
-

Labels are used to let you jump around the code, optionally based on some condition.

```lisp
(define main (function ()
    (push 0)
    (:start)

    (inc)
    (isDone)

    (jumpFalse :start)
    (done)
))

(main)
```

Variables:
-

Variables can be defined, set and retrieved again. These variables will be scoped to the function that they are created in.

```lisp
(define name "Global")
(define main (function ()
    (print "Started main")
    (print name)

    (set name "Set from scope")
    (print name)

    (define name "Created in scope")
    (print name)
    (print "End main")
))

(print name)
(main)
(print name)
```

Internally the assembler sees only the last argument as the code line argument except in the case of the `Run` command which has it's own runs (see below). So all other arguments in between are turned into `Push` commands.

Ports
-

Current ports are for C++17, C# and TypeScript. The C++17 makes use of `std::variant` and `std::optional` which gives it the more recent C++ requirement.

Author
-
Alan Lawrey 2022
