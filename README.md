# Simple Stack Virtual Machine

This repository contains code for a simple stack based virtual machine. It is expected that it in embedded in another program which will provide the real functionality. This machine is very simple and very general, it contains only the code necessary to push and pop from the main stack, jump to labels, jump with condition, call labels like functions and return from a call. It does come with a standard library which a few of the internal calls make use of, however the standard library does not need to be made available to any given script in order to run.

A list of the builtin types:
- **Null**: An empty value, not really intended to be pushed around, but if a command does not have a value (because the operator is taking a value from the stack) then the code line value will be null.
- **Strings**: Based off the standard string type from the host programming language.
- **Boolean**: true/false
- **Number**: 64 bit double.
- **Object/Dictionary/Map**: Key value pair with string keys and the value being another value.
- **List/Array**: A list of values.
- **Function**: A collection of code put into a value, it also contains the labels for jumps and input parameters.
- **BuiltinFunction**: A reference to a builtin function that can be passed around like a value.

All values should be considered *immutable*! This means that values can be shared between Virtual Machine instances without any issue.

All of these are built on top of several base interfaces:

- **IValue**: Base interface for all value types, the only things that a value needs is a way to compare with another value, a way to turn it into a string and what is it's type name.
- **IArrayValue**: For any array type values this only requires knowing a list of all values and a way to access single values.
- **IObjectValue**: For any object type values this only requires knowing a list of accessible keys and a way to look up individual key value.
- **IFunctionValue**: For any callable function like value it just needs a way to be invoked.

**Note**: These are just the general building blocks and interfaces, as shown by the differences between the different ports, the C++ does not adhere to the same interface.

The C++ interface contains a basic **value** which contains:
- **double**: The value for a number.
- **type**: An enum that indicates the type (null, number, true, false, complex).
- **std::shared_ptr<complex_type>**: A pointer to a more complex type that contains more information, such strings, arrays and objects.

The end result does mean there's always some wasted memory either in the double or the shared pointer, however instead of using a union or std::variant this simplified approach means that it remains safe and fast.

# Code

Currently the code is written with a Lisp like syntax. It should not be assumed that it is Lisp or that it supports all the things that Lisp would support. Lisp was chosen for it's ease of parsing and tokenising.

```lisp
(define main (function ()
    (print "Result: " (add 5 12))
))

(main)
```

This will push the `5`, `12` and `"add"` to the stack, run the command at the top of the stack (`"add"`), then push `"print"` to the stack and call run again. As for what `"add"` and `"print"` will do it up to environment that the virtual machine is running in. Ideally however the final result would print to a console `Result: 17`.

Here is an example of a run command handler for the above program in C#:
```csharp
private static Scope CreateScope()
{
    var result = new Scope();

    result.Define("add", (vm, args) =>
    {
        var num1 = args.GetIndex<NumberValue>(0);
        var num2 = args.GetIndex<NumberValue>(1);
        vm.PushStack(num1.Value + num2.Value);
    });

    result.Define("print", (vm, args) =>
    {
        Console.Write("Print: ");
        Console.WriteLine(string.Join("", args.Value));
    });

    return result;
}
```

The program will output

```
Result: 17
```

## Labels

Labels are used to let you jump around the code, optionally based on some condition. This example assume that some of the standard library is included for the `<` operator and `print` function.

```lisp
(define main (function ()
    (set x 0)
    (:start)

    (inc x)
    (if (< x 10))
        (
            (print "Less than 10: " x)
            (jump :start)
        )
    )
    (done)
))

(main)
```

## Variables

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

# Keywords

There's not many keywords that are built in and each are used by the assembler to turn into a set of operators that are used by the virtual machine.

### `define`
Creates a new variable in the current scope. Currently it is allowed that variables can be redefined, as such define will never throw an error.

### `set`
Updates a variable, if that variable does not exist in the current scope it will check the parent scope. Will throw an error if the variable has not been defined.
### `if`
### `unless`
### `function`
### `loop`
### `continue`
### `break`
### `inc`
### `dec`

# Standard Library

The standard library covers a lot of the basics.

## Misc

A handful of generic functions that aren't operators.

### `(print ...args)`
```lisp
; Prints all function arguments to the console.
; @param ...args value[]
; @returns nothing

(define name "Alan")
(print "Hello " 5 ": " name)

; Outputs
Hello 5: Alan
```

### `(typeof value)`
```lisp
; Returns the type name of the value
; @param input value
; @returns string

(define name "Alan")
(define year 2022)
(print (typeof name) ": " (typeof year))

; Outputs
string: number
```

### `(toString value)`
```lisp
; Turns the argument into a string
; @param input value
; @returns string

(define year 2022)
(print year ": " (typeof year))

(define yearStr (toString year))
(print yearStr ": " (typeof yearStr))

; Outputs
2022: number
2022: string
```

### `(compareTo value1 value2)`
```lisp
; Compares two values returning either -1, 0 or 1.
; Can be used for sorting and checking if two values are the same.
; @param value1 value
; @param value2 value
; @returns number

(print (compareTo 5 10))
(print (compareTo 10 5))
(print (compareTo 10 10))

; Outputs
-1
1
0
```

# Ports

Current ports are for C++11, .Net 6, Unity and TypeScript. The .Net version could be downgraded if need be.

Author
-
Alan Lawrey 2022
