Simple Stack Virtual Machine
=

This repository contains code for a simple stack based virtual machine. This machine is very simple and very general, it contains only the code necessary to push and pop from the main stack, jump to labels, jump with condition, call labels like functions and return from a call. The final command is to run an external command through a handler.

This is built it contains no math functions, string manipulation or anything, but the values the stack support is:
- Strings
- Boolean: True/False
- Number: 64 bit double.
- Object/Dictionary/Map: Key value pair with string keys.
- List/Array: A list of values.

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