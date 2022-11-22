# Lysithea Scripting Language

This repository contains code for a simple stack based virtual machine called Lysithea. It is expected that it in embedded in another program which will provide the real functionality. This machine is very simple and very general, it contains only the code necessary to push and pop from the main stack, jump to labels, jump with condition, call labels like functions and return from a call. It does come with a standard library which a few of the internal calls make use of, however the standard library does not need to be made available to any given script in order to run.

A list of the builtin types:
- **Null**: An empty value.
- **Undefined**: More an internally used value, but usually represents an error.
- **Strings**: Based off the standard string type from the host programming language.
- **Boolean**: true/false
- **Number**: 64 bit double.
- **Object/Dictionary/Map**: Key value pair with string keys and the value being another value.
- **List/Array**: A list of values.
- **Function**: A collection of code put into a value, it also contains the labels for jumps and input parameters.
- **BuiltinFunction**: A reference to a builtin function that can be passed around like a value.

All values should be considered *immutable*! This means that values can be shared between Virtual Machine instances without issue.

All of these are built on top of several base interfaces:

- **IValue**: Base interface for all value types, the only things that a value needs is a way to compare with another value, a way to turn it into a string and what is it's type name.
- **IArrayValue**: For any array type values this only requires knowing a list of all values and a way to access single values.
- **IObjectValue**: For any object type values this only requires knowing a list of accessible keys and a way to look up individual key value.
- **IFunctionValue**: For any callable function like value it just needs a way to be invoked.

**Note**: These are just the general building blocks and interfaces, as shown by the differences between the different ports, the C++ does not adhere to the same interface.

The C++ interface contains a basic **value** which contains:
- **double**: The value for a number.
- **type**: An enum that indicates the type (null, undefined, number, true, false, complex).
- **std::shared_ptr<complex_type>**: A pointer to a more complex type that contains more information, such strings, arrays and objects.

The end result does mean there's always some wasted memory either in the double or the shared pointer, however instead of using a union or std::variant this simplified approach means that it remains safe and fast.

# Code

Currently the code is written with a Lisp like syntax. It should not be assumed that it is Lisp or that it supports all the things that Lisp would support. Lisp was chosen for it's ease of parsing and tokenising.

```lisp
(function main ()
    (print "Result: " (+ 5 12))
)

(main)
```

This will push the `5` and `12` to the stack and then run the `+` operators, then push `"print"` to the stack and run the `call` opcode. As for `"print"` will do it up to environment that the virtual machine is running in. Ideally however the final result would print to a console `Result: 17`.

Here is an example of a run command handler for the above program in C#:
```csharp
private static Scope CreateScope()
{
    var result = new Scope();

    result.Define("print", (vm, args) =>
    {
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

Labels are used to let you jump around the code, optionally based on some condition. This example assume that some of the standard library is included for the `print` function.

```lisp
(function main()
    (set x 0)
    (:start)

    (++ x)
    (if (< x 10)
        (
            (print "Less than 10: " x)
            (jump :start)
        )
    )
    (done)
)

(main)
```

## Variables

Variables can be defined, set and retrieved again. These variables will be scoped to the function that they are created in.

```lisp
(define name "Global")
(function main()
    (print "Started main")
    (print name)

    (set name "Set from scope")
    (print name)

    (define name "Created in scope")
    (print name)
    (print "End main")
)

(print name)
(main)
(print name)
```

Internally the assembler sees only the last argument as the code line argument except in the case of the `Run` command which has it's own runs (see below). So all other arguments in between are turned into `Push` commands.

# Architecture

The internals of the virtual machine is fairly simple and broken into general purpose operators, math operators, comparison operators, boolean operators and one bonus string concatenation operators.

## Some concepts

### General Purpose Operators
Internal operators that the virtual machine uses:

#### `(unknown)`
Usually only used for error situations when assembling or if a code line (see next) does not have an argument.

#### `(push arg)`
Pushes a value onto the data stack. Can be of any value.

#### `(call numArgs)`
Attempts to invoke a function from the top of the stack, the code line is expected to contain a number which has how many arguments to call the function with.

#### `(return)`
Returns from a function call. This is only needed if you want to return early from a function.

Importantly! It does not function as a way to return values from a function, as the system is stack based whatever is put onto the stack can be accessed from the stack by other functions after a function has finished.

#### `(get varName?)`
Attempts to find a variable based on it's name in the current scope, up to the builtin scope.

The get operator will either lookup the variable based on the code line input or it will grab the value at the top of the stack and use that as the variable name.

#### `(getProperty propertyArray?)`
Attempts to find a value from the value that is on top of the stack. For example looking up `math.sin` will be broken up into `(get "math") (getProperty ("sin"))`. This also works for arrays `arrayValue.0.name` which will get turned into `(get "arrayValue") (getProperty (0 "name"))`.

The value will have to be something that implements the `IArrayValue` or `IObjectValue` interface.

#### `(define varName?)`
This will create/set a variable with the `varName` with the next value from the stack.

If `varName` is not provided it will be taken from the top of the stack before getting the value.

The new variable will be created in the current scope.

#### `(set varName?)`
This like the `define` operator will set the variable `varName` with the next value from the stack.

If `varName` is not provided it will be taken from the top of the stack before getting the value.

Unlike `define` the variable must exist in a scope otherwise an error is thrown. If the variable does not exist in the current scope it will follow the parent scope until it reaches the global scope.

#### `(jump label?)`
Unconditionally jumps to the provided `label`. If the label is not given then it is taken from the top of the stack.

#### `(jumpFalse label?)`
Conditionally jumps to the provided `label`. If the label is not given then it is taken from the top of the stack.

After the label is grabbed the next value from the top of the stack is compared with `false` and if they are equal then it will jump to the label.

#### `(jumpTrue label?)`
Conditionally jumps to the provided `label`. If the label is not given then it is taken from the top of the stack.

After the label is grabbed the next value from the top of the stack is compared with `true` and if they are equal then it will jump to the label.

**These are more advanced operator:**

#### `(toArgument value?)`
Turns the value from the code line or the value from the top of the stack into an argument array, and expects the top to be array like in the first place.

This is used in situations where we need to distinguish between arrays and argument arrays when unpacking variable length argument calls.

**Note** Unpacking is designed for working with function calls, calls to operators will *not* support unpacking.

 For example:
 ```lisp
 (define list (5 10))
 (define x (+ ...list)) ; This will throw an error because the add operator expects two number inputs.
 (print x)
 ```

For reference this is a working example above, making use of a regular `math.sum` function call instead of using a the `+` operator:

```lisp
(define list (5 10))
(define x (math.sum ...list))
(print x) ; Outputs 15
```


#### `(callDirect (function numArgs))`
An optimised version of **Call** where the code_line contains an array `(function_value num_args)` which side-steps the need to look up the value from the current scope. These operators come from knowing what values that can be found assemble time.

**Note**: Since `callDirect` operators come from assemble time, it means that if a value is redefined at run time, the operators will still have a reference to the old value from assemble time and will be unaffected.

### Math Operators

#### `(+ num1 num2)`
Takes the top two values from the stack and pushes the result of adding those numbers together.

```lisp
(print (+ 10 15)) ; Outputs 25
```

#### `(- num1 num2)`
Takes the top two values from the stack and pushes the result of subtracting the top value from the second top value.

```lisp
(print (- 10 15)) ; Outputs -5
```

#### `(- num1)`
Takes the top value from the stack and pushes the negated value.

```lisp
(print (- 10)) ; Outputs -10
```

#### `(* num1 num2)`
Takes the top two values from the stack and pushes the result of multiplying those numbers together.

#### `(/ num1 num2)`
Takes the top two values from the stack and pushes the result of dividing the second top value from the top value.

```lisp
(print (/ 10 2)) ; Outputs 5
```

#### `(++ variable)`
Increments the variable by one.

```lisp
(define x 10)
(++ x)
(print x) ; Outputs 11
```

#### `(-- variable)`
Decrements the variable by one.

```lisp
(define x 10)
(-- x)
(print x) ; Outputs 9
```

### Comparison Operators

The comparison operators calculate a numeric value between two value (-1, 0, 1) with zero meaning they are equal, a one meaning that the `left` value is greater than the `right` and a negative one meaning that the `left` value is less than the `right` value. This makes use of the internal `CompareTo` methods on the value types.

The final result from each of these comparisons is to push a `true` or `false` onto the stack.

As any two variables can be compared this means the type does have to match. However it is up to the `left` operator to determine if the `right` value should match or not. For all builtin types the types have to match, there is **no** type coercion. And as such should be all symmetrical.

But for custom types there's nothing stopping you from having something that lets you compare if a `Vector3` matches an `Array` of numbers for example.

**However** the custom type would always have to be on the left as the array class would not satisfy the same comparison. So be careful to avoid situations where you end up with:

```lisp
(define myVec (newVector 1 2 3))
(define myArr (1 2 3))
(print (== myVec myArr)) ; Uses custom comparison from myVec, outputs 0
(print (== myArr myVec)) ; Uses the builtin array comparison, outputs 1
```

#### `(< left right)`
Checks if the comparison is less than 0.

```lisp
(define num 5)
(print (< num 5)) ; Outputs false

(define str "ABC")
(print (< str "BCD")) ; Outputs true
```

#### `(<= left right)`
Checks if the comparison is less than or equal to 0.

```lisp
(define num 5)
(print (<= num 5)) ; Outputs true

(define str "ABC")
(print (<= str "BCD")) ; Outputs true
```

#### `(> left right)`
Checks if the comparison is greater than 0.

```lisp
(define num 5)
(print (> num 5)) ; Outputs false

(define str "ABC")
(print (> str "BCD")) ; Outputs false
```

#### `(>= left right)`
Checks if the comparison is greater than or equal to 0.

```lisp
(define num 5)
(print (>= num 5)) ; Outputs true

(define str "ABC")
(print (>= str "BCD")) ; Outputs false
```

#### `(== left right)`
Checks if the comparison is equal to 0.

```lisp
(define num 5)
(print (== num 5)) ; Outputs true

(define str "ABC")
(print (== str "BCD")) ; Outputs false
```

#### `(!= left right)`
Checks if the comparison is not equal to 0.

```lisp
(define num 5)
(print (!= num 5)) ; Outputs false

(define str "ABC")
(print (!= str "BCD")) ; Outputs true
```

### Boolean Operators

The boolean operators work on boolean values.

#### `(&& left right)`
Outputs true if both `left` and `right` are true.

```lisp
(print (&& true true)) ; Outputs true
(print (&& false true)) ; Outputs false
(print (&& true false)) ; Outputs false
(print (&& false false)) ; Outputs false

(print (&& (== 5 5) (!= 5 10))) ; Outputs true
```

#### `(|| left right)`
Outputs true if either `left` or `right` are true.

```lisp
(print (|| true true)) ; Outputs true
(print (|| false true)) ; Outputs true
(print (|| true false)) ; Outputs true
(print (|| false false)) ; Outputs false

(print (|| (== 5 5) (!= 5 10))) ; Outputs true
```

#### `(! input)`
Pushes the opposite boolean value onto the stack, `true` -> `false` and `false` -> `true`.

```lisp
(print (! true)) ; Outputs false
(print (! false)) ; Outputs true
```

### Misc Operators

#### `($ ...inputs)`
Concatenates all inputs input a single string value and pushes that onto the stack.

```lisp
(print ($ "Hello" "there"))
; Outputs Hellothere

(define name "Alan")
(define degrees 25)
(print ($ "Hello " name ", today it is " degrees " degrees outside"))
; Outputs Hello Alan, today it is 25 degrees outside

(define list (1 2 3 4))
(print ($ "Joined list " ...list))
; Outputs Joined list 1234
```

If you are wondering about adding a separator between the joined strings, there is a function in the `string` library `string.join` which takes a separator and then a list of inputs to join together.

### Code Line
A code line is a simple pair of **operator** and optionally a single **value**.

### Function
A function is made up of a list of **code lines**, a dictionary of **labels** to line numbers used by the jump operators and a list of **parameters**.

When a function is called (using either call operator) the current list of code is replaced with the ones from the new function, the current code is pushed to the call stack and the parameters list is used to create variables in a new scope that match the names from the parameters list.

### Scope

- **Program Counter:** Current code line.
- **Current Scope:** Stores the current functions variables.
- **Global Scope:** Start scope, stores variables at the global level.
- **Builtin Scope:** Scope for variables that come from outside of the virtual machine. Usually at assemble time. This is separate from the global scope as often the builtin scope will be shared between virtual machines, so it is detached from the usual scope parent system and is read only.

# Keywords

There's not many keywords that are built in and each are used by the assembler to turn into a set of operators that are used by the virtual machine.

### `(define varName1 ...varNameN value)`
Creates a new variable in the current scope. Currently it is allowed that variables can be redefined, as such define will never throw an error.

Simple example:
```lisp
(define name "Alan")
(print "Hello " name) ; Outputs Hello Alan
```

Scoping example:
```lisp
(define name "Global Name")
(function main ()
    (print name) ; Outputs Global Name

    (define name "Local Name")
    (print name) ; Outputs Local Name
)

(print name) ; Outputs Global Name
(main)
(print name) ; Outputs Global Name
```

For functions that return multiple values, you'll want to put those into variables.
```lisp
(function multiply(x)
    (return (* x 2) (* x 3))
)
(function divide(x)
    (return (/ x 2) (/ x 3))
)

(define left right (multiply 5))
(print "Multiply " left ", " right) ; Multiply 10, 15

(define left right (divide 15))
(print "Divide " left ", " right) ; Divide 7.5, 5
```

### `(set varName1 ...varNameN value)`
Updates a variable, if that variable does not exist in the current scope it will check the parent scope. Will throw an error if the variable has not been defined.

Simple example:
```lisp
(define name "Alan")
(print "Hello " name) ; Outputs Hello Alan

(set name "Lawrey")
(print "Hello " name) ; Outputs Hello Lawrey
```

Scoping example:
```lisp
(define name "Global Name")
(define main (function ()
    (print name) ; Outputs Global Name

    (set name "Local Name")
    (print name) ; Outputs Local Name
))

(print name) ; Outputs Global Name
(main)
(print name) ; Outputs Local Name
```

Error example:
```lisp
(define name "Name")
(set name 30) ; Okay to do
(set age 30)  ; throws an error because
```

Setting multiple variables follows the same logic as defining multiple variables.

### `(if (conditionalCode) (whenTrueCode) (whenFalseCode?))`
The conditional code is executed first and if result in a `true` value then the `whenTrueCode` is executed. If another block is provided then that will be executed if the value is not true.

Simple example:
```lisp
(function logCounter ()
    (if (< counter 10)
        (print "Counter less than 10")
        (print "Counter more than 10")
    )
)

(define counter 0)
(logCounter) ; Prints Counter less than 10

(define counter 20)
(logCounter) ; Prints Counter more than 10
```

Example of wrapping a code block for single when true block.
```lisp
(define progress 0)
(if (< progress 100)
    (
        (print "Still in progress")
        (print "Please wait...")
    )
    (
        (print "100% Progress")
        (print "All done")
    )
)

; Outputs
Still in progress
Please wait...
```

### `(unless (conditionalCode) (whenFalseCode) (whenTrueCode?))`
The conditional code is executed first and if result in a `false` value then the `whenFalseCode` is executed. If another block is provided then that will be executed if the value is not false.

```lisp
(define progress 0)
(unless (< progress 100)
    (
        (print "100% Progress")
        (print "All done")
    )
    (
        (print "Still in progress")
        (print "Please wait...")
    )
)

; Outputs
Still in progress
Please wait...
```

### `(function name? (parameterList) (codeBody))`
Creates a new function value, takes a parameter list, the list itself is required but it can be empty.

The parameter list itself is parsed only as a list of strings.

The `name` is optional and if it's left out the function will be anonymous.
Additionally if the name is present and the function is not being used a value in a function call, the function will also be defined.

```lisp
(function clamp(input lower upper)
    (if (< input lower)
        (return lower)
    )
    (if (> input upper)
        (return upper)
    )
    (return input)
)

(print "Clamped 5 "  (clamp 5 -1 1))  ; Clamped 5 1
(print "Clamped -5 " (clamp -5 -1 1)) ; Clamped -5 -1
(print "Clamped 0 "  (clamp 0 -1 1))  ; Clamped 0 0
)
```

Unpack arguments example:
```lisp
(function log (type ...inputs)
    (print "[" type "]: " ...inputs)
)

(function findMin(...values)
    (if (== values.length 0)
        (return null)
    )

    (define min values.0)
    (define i 1)
    (loop (< i values.length)
        (define curr (array.get values i))
        (if (> min curr)
            (set min curr)
        )
        (++ i)
    )

    (return min)
)

(log "Info" "Minimum Number: " (findMin 1 2 3))
(log "Info" "Minimum Number: " (findMin 20 30 10))
(log "Info" "Minimum Lexical: " (findMin "ABC" "DEF" "ZXC"))
(log "Info" "Minimum Empty: " (findMin))

; Outputs
[Info]: Minimum Number: 1
[Info]: Minimum Number: 20
[Info]: Minimum Lexical: ABC
[Info]: Minimum Empty: null
```

Example of function name defining.
```lisp
(define main1 (function ()
    (print "Inside main1")
))

(function main2 ()
    (print "Inside main2")
)

(main1)
(main2)

(print)

(print "Main1: " (toString main1))
(print "Main2: " (toString main2))

; Outputs
Inside main1
Inside main2

Main1: function:anonymous
Main2: function:main2
```

### `(loop (conditionalCode) (loopBody))`
Creates a looping section of the code. This is effectively a `while` found in other languages.

```lisp
(define i 0)
(loop (< i 4)
    (print i)
    (++ i)
)
(print "Done")

; Outputs
0
1
2
3
Done
```
The loop is closer to syntax sugar and is equivalent to:
```lisp
(define i 0)
(:LoopStart)
(if (< i 4)
    (
        (print i)
        (++ i)
        (jump :LoopStart)
    )
)
(:LoopEnd)
(print "Done")

; Outputs
0
1
2
3
Done
```

Whilst syntax sugar elements have been avoided for the sake of simplicity, in this case loops are very common and having to come up with unique loop start and end labels would definitely become tedious.

### `(continue)`
Used inside loops to jump back to the start of the loop.

```lisp
(define i 0)
(loop (< i 6)
    (++ i)

    (if (<= i 3)
        (continue)
    )
    (print i)
)
(print "Done")

; Output
4
5
6
Done
```

### `(break)`
Used inside loops to break out of the loop and jump to the end.

```lisp
(define i 0)
(loop (< i 6)
    (++ i)

    (print i)

    (if (> i 3)
        (break)
    )
)
(print "Done")

; Output
1
2
3
4
Done
```

### `(++ varName ...varNameN)`
Increments the value of a variable, or variables if there are multiple. In addition to be simpler to write that a `set` and addition operator it's also more performant since it's internally uses fewer instructions.

```lisp
(define i 0)
(define j 0)
(print i) ; 0
(++ i)
(print i " : " j) ; 1 : 0

(++ i j)
(print i " : " j) ; 2 : 0
```

### `(-- varName ...varNameN)`
Decrements the value of a variable, or variables if there are multiple. In addition to be simpler to write that a `set` and subtraction operator it's also more performant since it's internally uses fewer instructions.

```lisp
(define i 0)
(define j 0)
(print i) ; 0
(-- i)
(print i " : " j) ; -1 : 0

(-- i j)
(print i " : " j) ; -2 : -1
```

# Standard Library
By default there's basically no builtin functionality to manipulate data. So there is a minimal standard library offered.

- **Misc**: A handful of general functions: toString, print, compareTo and typeof.
- **String**: Basic string manipulation: get, set, substring, join, remove, etc.
- **Array**: Basic array manipulation: get, set, sublist, join, remove, etc.
- **Object**:Basic object manipulation: get, set, keys, values, remove, etc.
- **Math**: Basic math operations: trigonometry, log, exp, pow, min, max, etc.
- **Assert**: A very basic asserting library which will check if two values are equal or not equal, or true/false and will stop the VM if the assert fails.

# Ports
Current ports are for C++11, .Net 6, Unity and TypeScript. The .Net version could be downgraded if need be.

Author
-
Alan Lawrey 2022
