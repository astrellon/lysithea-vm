# Lysithea Scripting Language

This repository contains code for a simple stack based virtual machine called Lysithea. It is expected that it in embedded in another program which will provide the real functionality. This machine is very simple and very general, it contains only the code necessary to push and pop from the main stack, jump to labels, jump with condition, call labels like functions and return from a call. It does come with a standard library which a few of the internal calls make use of, however the standard library does not need to be made available to any given script in order to run.


Check out the [live demo](https://profile.alanlawrey.me/lysithea-vm/) and the [YouTube video](https://www.youtube.com/watch?v=xeuKEHhp0jc) about this project.

A list of the builtin types:
- **Null**: An empty value.
- **Undefined**: More an internally used value, but usually represents an error.
- **Strings**: Based off the standard string type from the host programming language.
- **Boolean**: true/false
- **Number**: 64 bit double.
- **Map**: Key value pair with string keys and the value being another value.
- **List**: A list of values.
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

Here is an example of defining a new function for the above program in C#:
```csharp
private static Scope CreateScope()
{
    var result = new Scope();

    result.TryDefine("print", (vm, args) =>
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

Labels are used to let you jump around the code, optionally based on some condition. This example assume that some of the standard library is included for the `print` function and that there's some sort of custom `done` function.

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

## Types
The number of builtin types is fairly limited and extended the number of supported types is done through the host platform and not from within the script itself.

**Note:** All the systems are built with the mindset that they are immutable or at the very least will always be safe to pass around. Which means strings, lists and maps are all immutable.

Any new type doesn't have to be immutable, but it should be safe that numerous different places could be interacting with it at the same time, like a manager class that already expects different places to talk to it.

### Number
Numbers are all stored internally as a 64bit double, and by default does not have any bitwise operators or support as it's not intended for that sort of low level number manipulation. These make use of the platforms native `double` type generally and so any quirks of that platform will be present.

```lisp
(define age 33)
(define distanceToShops 1.54)
(print "Person is " age " years old and is " distanceToShops "km away from shops")
; Outputs Person is 33 years old and is 1.54km away from shops
```

### Boolean
Booleans are just `true` or `false`. These are also used with `loop` and `if` statements.

```lisp
(define isRunning true)
(define counter 0)
(loop (isRunning)
    (++ counter)
    (set isRunning (< counter 10))
)
(print "Counter is at: " counter) ; Outputs Counter is at: 10
```

### Strings
Strings are just an array of characters. This type will be built on top of the default string data structure from the host platform.
Strings are created using either a pair of double quotes or a pair of single quotes. They also support multiline input without needing escaped characters.

```lisp
(define name "Alan")
(print "Name is " name " and the name length is " name.length " bytes long") ; Outputs Name is Alan and the name length is 4 bytes long.
```

A multiline string
```lisp
(define introText "Welcome to the program, choose an option:
- 1: Option 1
- 2: Option 2")

(print introText) ; Outputs
; Welcome to the program, choose an option:
; - 1: Option 1
; - 2: Option 2
```

### Lists
A list is as defined in the code is considered to be a literal, and as such is immutable. They are created using a pair of square brackets.

```lisp
(define list ["Hello" "there" "how" "are" "you?"])
(print list) ; Outputs [Hello there how are you?]
```

They can contain any constant value.
```lisp
(const name "Alan")
(function callback()
    (print "From callback")
)

(define list ["Person" name callback])
(print list) ; Output [Person Alan function:callback]
```

### Maps
A map is a string key to any type value pair dictionary. They are created using pairs of keys and values within a pair of curly brackets.

```lisp
(define map {
    name "Alan"
    age 33
})
(print map) ; Outputs {"name" Alan "age" 33}
```

The value can be any constant value.
```lisp
(const personName "Alan")
(const personAge 33)
(function debugPrint ()
    (print "Debug print")
)

(define map {
    name personName
    age personAge
    callback debugPrint
})

(print map)
; Outputs {"name" Alan "age" 33 "callback" function:debugPrint}
```

## Internal Operators

**Note:** Internal operators that the virtual machine uses, these are used by the *internals* of the virtual machine and are not the actual code that is written by a user.

### `(unknown)`
Usually only used for error situations when assembling or if a code line (see next) does not have an argument.

### `(push arg)`
Pushes a value onto the data stack. Can be of any value.

### `(call numArgs)`
Attempts to invoke a function from the top of the stack, the code line is expected to contain a number which has how many arguments to call the function with.

### `(return)`
Returns from a function call. This is only needed if you want to return early from a function.

Importantly! It does not function as a way to return values from a function, as the system is stack based whatever is put onto the stack can be accessed from the stack by other functions after a function has finished.

### `(get varName?)`
Attempts to find a variable based on it's name in the current scope, up to the builtin scope.

The get operator will either lookup the variable based on the code line input or it will grab the value at the top of the stack and use that as the variable name.

### `(getProperty propertyArray?)`
Attempts to find a value from the value that is on top of the stack. For example looking up `math.sin` will be broken up into `(get "math") (getProperty ("sin"))`. This also works for arrays `arrayValue.0.name` which will get turned into `(get "arrayValue") (getProperty (0 "name"))`.

The value will have to be something that implements the `IArrayValue` or `IObjectValue` interface.

### `(define varName?)`
This will create/set a variable with the `varName` with the next value from the stack.

If `varName` is not provided it will be taken from the top of the stack before getting the value.

The new variable will be created in the current scope.

### `(set varName?)`
This like the `define` operator will set the variable `varName` with the next value from the stack.

If `varName` is not provided it will be taken from the top of the stack before getting the value.

Unlike `define` the variable must exist in a scope otherwise an error is thrown. If the variable does not exist in the current scope it will follow the parent scope until it reaches the global scope.

### `(jump label?)`
Unconditionally jumps to the provided `label`. If the label is not given then it is taken from the top of the stack.

### `(jumpFalse label?)`
Conditionally jumps to the provided `label`. If the label is not given then it is taken from the top of the stack.

After the label is grabbed the next value from the top of the stack is compared with `false` and if they are equal then it will jump to the label.

### `(jumpTrue label?)`
Conditionally jumps to the provided `label`. If the label is not given then it is taken from the top of the stack.

After the label is grabbed the next value from the top of the stack is compared with `true` and if they are equal then it will jump to the label.

**These are more advanced operator:**

### `(toArgument value?)`
Turns the value from the code line or the value from the top of the stack into an argument array, and expects the top to be array like in the first place.

This is used in situations where we need to distinguish between arrays and argument arrays when unpacking variable length argument calls.

**Note** Unpacking is designed for working with function calls, calls to operators will *not* support unpacking.

 For example:
 ```lisp
 (define list [5 10])
 (define x (+ ...list)) ; This will throw an error because the add operator expects two number inputs.
 (print x)
 ```

For reference this is a working example above, making use of a regular `math.sum` function call instead of using a the `+` operator:

```lisp
(define list [5 10])
(define x (math.sum ...list))
(print x) ; Outputs 15
```


### `(callDirect (function numArgs))`
An optimised version of **Call** where the code_line contains an array `(function_value num_args)` which side-steps the need to look up the value from the current scope. These operators come from knowing what values that can be found assemble time.

**Note**: Since `callDirect` operators come from assemble time, it means that if a value is redefined at run time, the operators will still have a reference to the old value from assemble time and will be unaffected.

## Math Operators

General basic math operators, the arithmetic ones (`+`, `-`, `*` and `/`) only take two inputs. However during the assembling stage multiple inputs can be used and they will be changed together. As such don't think that it is any more performant to use one call vs chaining multiple ones.

**Note:** These are used by the virtual machine and are also directly written by the user.

### Addition `(+ num1 num2)`
Takes the top two values from the stack and pushes the result of adding those numbers together.

```lisp
(print (+ 10 15)) ; Outputs 25
```

More than two inputs can be used, however it will be turned into a chain of operators that only take two inputs. This done at assembler time.

```lisp
(print (+ 1 2 3 4)) ; Outputs 10
; Is effectively
(print (+ 1 (+ 2 (+ 3 4)))) ; Outputs 10
```

### Subtract `(- num1 num2 ...numN)`
Takes the top two values from the stack and pushes the result of subtracting the top value from the second top value.

```lisp
(print (- 10 15)) ; Outputs -5
```

More than two inputs can be used, however it will be turned into a chain of operators that only take two inputs. This is done at assembler time.

```lisp
(print (- 1 2 3 4)) ; Outputs -8
; Is effectively
(print (+ 1 (+ -2 (+ -3 -4))) ; Outputs -8
```

 It does not actually turn each input into the negative value and then add them, but each new number is another `push` and `sub` pair of operators that are in the code, which has the same effect.

### Unary Negative `(- num1)`
Takes the top value from the stack and pushes the negated value.

```lisp
(print (- 10)) ; Outputs -10
```

### Multiply `(* num1 num2 ...numN)`
Takes the top two values from the stack and pushes the result of multiplying those numbers together.

```lisp
(print (* 5 10)) ; Outputs 50
```

More than two inputs can be used, however it will be turned into a chain of operators that only take two inputs. This is done at assembler time.

```lisp
(print (* 1 2 3 4)) ; Outputs 24
; Is effectively
(print (* 1 (* 2 (* 3 4)))) ; Outputs 24
```

### Divide `(/ num1 num2 ...numN)`
Takes the top two values from the stack and pushes the result of dividing the second top value from the top value.

```lisp
(print (/ 10 2)) ; Outputs 5
```

More than two inputs can be used, however it will be turned into a chain of operators that only take two inputs. This is done at assembler time.

```lisp
(print (/ 1 2 3 4)) ; Outputs 0.0416666666666667
; Is effectively
(print (* 1 (* 0.5 (* 0.3333333333333333 0.25))) ; Outputs 0.0416666666666667
```
It does not actually convert the inputs into the reciprocal and multiplies them together, instead the end result is the same.

Each number just adds another pair of `push` and `divide` operators into the code.

### Increment `(++ variable ...variableN)`
Increments the variable by one.

**Note:** The increment operator *only* increments a variable, it does not push the resulting value onto the stack!

```lisp
(define x 10)
(++ x)
(print x) ; Outputs 11
```

 With multiple variables each one is incremented.
 ```lisp
 (define x 10)
 (define y 20)
 (++ x y)
 (print x ":" y) ; Outputs 11:21
 ```

### Decrement `(-- variable ...variableN)`
Decrements the variable by one.

**Note:** The decrement operator *only* decrements a variable, it does not push the resulting value onto the stack!

```lisp
(define x 10)
(-- x)
(print x) ; Outputs 9
```

 With multiple variables each one is decremented.
 ```lisp
 (define x 10)
 (define y 20)
 (-- x y)
 (print x ":" y) ; Outputs 9:19
 ```

## Comparison Operators

The comparison operators calculate a numeric value between two value (-1, 0, 1) with zero meaning they are equal, a one meaning that the `left` value is greater than the `right` and a negative one meaning that the `left` value is less than the `right` value. This makes use of the internal `CompareTo` methods on the value types.

The final result from each of these comparisons is to push a `true` or `false` onto the stack.

As any two variables can be compared this means the type does have to match. However it is up to the `left` operator to determine if the `right` value should match or not. For all builtin types the types have to match, there is **no** type coercion. And as such should be all symmetrical.

But for custom types there's nothing stopping you from having something that lets you compare if a `Vector3` matches an `Array` of numbers for example.

**However** the custom type would always have to be on the left as the array class would not satisfy the same comparison. So be careful to avoid situations where you end up with:

```lisp
(define myVec (newVector 1 2 3))
(define myArr [1 2 3])
(print (== myVec myArr)) ; Uses custom comparison from myVec, outputs 0
(print (== myArr myVec)) ; Uses the builtin array comparison, outputs 1
```

### Less Than `(< left right)`
Checks if the comparison is less than 0.

```lisp
(define num 5)
(print (< num 5)) ; Outputs false

(define str "ABC")
(print (< str "BCD")) ; Outputs true
```

### Less Than Or Equals `(<= left right)`
Checks if the comparison is less than or equal to 0.

```lisp
(define num 5)
(print (<= num 5)) ; Outputs true

(define str "ABC")
(print (<= str "BCD")) ; Outputs true
```

### Greater Than `(> left right)`
Checks if the comparison is greater than 0.

```lisp
(define num 5)
(print (> num 5)) ; Outputs false

(define str "ABC")
(print (> str "BCD")) ; Outputs false
```

### Greater Than Or Equals `(>= left right)`
Checks if the comparison is greater than or equal to 0.

```lisp
(define num 5)
(print (>= num 5)) ; Outputs true

(define str "ABC")
(print (>= str "BCD")) ; Outputs false
```

### Equals `(== left right)`
Checks if the comparison is equal to 0.

```lisp
(define num 5)
(print (== num 5)) ; Outputs true

(define str "ABC")
(print (== str "BCD")) ; Outputs false
```

### Not Equals `(!= left right)`
Checks if the comparison is not equal to 0.

```lisp
(define num 5)
(print (!= num 5)) ; Outputs false

(define str "ABC")
(print (!= str "BCD")) ; Outputs true
```

## Boolean Operators

The boolean operators work on boolean values.

### And `(&& left right ...inputN)`
Outputs true if both `left` and `right` are true.

```lisp
(print (&& true true)) ; Outputs true
(print (&& false true)) ; Outputs false
(print (&& true false)) ; Outputs false
(print (&& false false)) ; Outputs false

(print (&& (== 5 5) (!= 5 10))) ; Outputs true
```

Like the math operators the and operator also supports multiple inputs, basically all inputs must be `true` for the result to be `true`:
```lisp
(print (&& true true true true)) ; Outputs true
(print (&& true false true true)) ; Outputs false
(print (&& true false false true)) ; Outputs false
```

### Or `(|| left right ...inputN)`
Outputs true if either `left` or `right` are true.

```lisp
(print (|| true true)) ; Outputs true
(print (|| false true)) ; Outputs true
(print (|| true false)) ; Outputs true
(print (|| false false)) ; Outputs false

(print (|| (== 5 5) (!= 5 10))) ; Outputs true
```

Like the math operators the or operator also supports multiple inputs, basically if any input is `true` then the result is `true`:
```lisp
(print (|| true true true true)) ; Outputs true
(print (|| true false true true)) ; Outputs true
(print (|| false false false true)) ; Outputs true
(print (|| false false false false)) ; Outputs false
```

### Not `(! input ...inputN)`
Pushes the opposite boolean value onto the stack, `true` -> `false` and `false` -> `true`.

```lisp
(print (! true)) ; Outputs false
(print (! false)) ; Outputs true
```

This can also take multiple inputs for multiple variables.
```lisp
(define x y (! true false))
(print x ":" y) ; Outputs false:true
```

## Misc Operators

### String Concatenation `($ ...inputs)`
Concatenates all inputs input a single string value and pushes that onto the stack.

```lisp
(print ($ "Hello" "there"))
; Outputs Hellothere

(define name "Alan")
(define degrees 25)
(print ($ "Hello " name ", today it is " degrees " degrees outside"))
; Outputs Hello Alan, today it is 25 degrees outside
```

**Note:** Unlike the other operators string concatenation does support unpacking:
```lisp
(define list [1 2 3 4])
(print ($ "Joined list " ...list))
; Outputs Joined list 1234
```

If you are wondering about adding a separator between the joined strings, there is a function in the `string` library `string.join` which takes a separator and then a list of inputs to join together.

## Conjoined Operators

These aren't operators in their own right, they are more just shorthand for slightly cumbersome code. This means that it's an assembler time code transform, which means it behaves the same way.

### Additional Assignment `(+= varName input)`
This is shorthand for:

```lisp
(set varName (+ varName input))
```

It also supports handling multiple inputs too (and also not supporting array unpacking).
```lisp
(define total 5)
(+= total 1 2 3 4)
(print total) ; Outputs 15

; The same as
(define total 5)
(set total (+ total 1 2 3 4))
(print total) ; Outputs 15
```

### Subtract Assignment `(-= varName input)`
This is shorthand for:

```lisp
(set varName (- varName input))
```

It also supports handling multiple inputs too (and also not supporting array unpacking).
```lisp
(define total 5)
(-= total 1 2 3 4)
(print total) ; Outputs -5

; The same as
(define total 5)
(set total (- total 1 2 3 4))
(print total) ; Outputs -5
```

### Multiply Assignment `(*= varName input)`
This is shorthand for:

```lisp
(set varName (* varName input))
```

It also supports handling multiple inputs too (and also not supporting array unpacking).
```lisp
(define total 5)
(*= total 1 2 3 4)
(print total) ; Outputs 120

; The same as
(define total 5)
(set total (* total 1 2 3 4))
(print total) ; Outputs 120
```

### Divide Assignment `(/= varName input)`
This is shorthand for:

```lisp
(set varName (/ varName input))
```

It also supports handling multiple inputs too (and also not supporting array unpacking).
```lisp
(define total 12)
(/= total 1 2 3 4)
(print total) ; Outputs 0.5

; The same as
(define total 12)
(set total (/ total 1 2 3 4))
(print total) ; Outputs 0.5
```

### And Assignment `(&&= varName input)`
This is shorthand for:

```lisp
(set varName (&& varName input))
```

It also supports handling multiple inputs too (and also not supporting array unpacking).
```lisp
(define result false)
(&&= result true)
(print result) ; Outputs false

; The same as
(define result false)
(set result (&& false true))
(print result) ; Outputs false
```
### Or Assignment `(||= varName input)`
This is shorthand for:

```lisp
(set varName (|| varName input))
```

It also supports handling multiple inputs too (and also not supporting array unpacking).
```lisp
(define result false)
(||= result true)
(print result) ; Outputs true

; The same as
(define result false)
(set result (|| false true))
(print result) ; Outputs true
```

### String Concatenation Assignment `($= varName ...inputs)`
This is shorthand for:

```lisp
(set varName ($ varName ...inputs))
```

It also supports handling multiple inputs too and array unpacking.
```lisp
(define result "Time Taken: ")
($= result 50 "ms")
(print result) ; Outputs Time Taken: 50ms

; The same as
(define result "Time Taken: ")
(set result ($ result 50 "ms"))
(print result) ; Outputs Time Taken: 50ms
```

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

### `(const varName value)`
This works slightly different from `define` and `set` in that only one constant can be set since the `value` must be a compile time constant, and right now there is no execution of code during the compile phase.

```lisp
(const name "Alan")
(const age 33)

(function main()
    (define list ["Person" name age])
    (print "List: " list) ; Outputs ["Person" "Alan" 33]
)

(main)
```

As constants are handled at compile time they effectively act like replacements. At the global level those values will exist within the global scope for retrieval outside of the script, however within a function the constants will **not** exist in the function scope because they would have to exist already and so are not kept around.

For example if you try to use `isDefined` to check if a value exists in the current scope, a constant will be false.

```lisp
(const name "Alan")
(define age 33)

(print "Name " name ", is defined:  " (isDefined name))
; Outputs Name Alan, is defined: false

(print "Age " age ", is defined:  " (isDefined age))
; Outputs Age 33, is defined: true
```

However as constants are a compile time aspect, global ones do get stored in the `BuiltinScope` of a `Script` which means that they can still be accessed from outside of the script.

This will create a constant called `testFunc`.
```lisp
(function testFunc ()
    (print "Called from test func!")
)
```

And you can access that value from the scripts builtin scope.
```cs
if (script.BuiltinScope.TryGetKey<IFunctionValue>("testFunc", out var func))
{
    Console.WriteLine("Calling test function");

    // Calling the function puts the function code next to execute.
    vm.CallFunction(func, 0, false);

    // Execute the code until a halt.
    vm.Execute();
}
else
{
    Console.WriteLine("Failed to find testFunc to call");
}
```

### `(if (conditionalCode) (whenTrueCode) (whenFalseCode?))`
The conditional code is executed first and if result in a `true` value then the `whenTrueCode` is executed. If another block is provided then that will be executed if the value is not true.

Simple example:
```lisp
(function logCounter (counter)
    (if (< counter 10)
        (print "Counter less than 10")
        (print "Counter more than 10")
    )
)

(logCounter 5) ; Prints Counter less than 10
(logCounter 20) ; Prints Counter more than 10
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

Inlining `if` statements. As `if` statements are block expressions then you can use them inside other statements.

```lisp
(define progress 0)
(define message (if (< progress 100) "Still in progress" "All done"))
(print "Progress " progress "%: " message)
; Outputs Progress 0%: Still in progress

(define progress 100)
(define message (if (< progress 100) "Still in progress" "All done"))
(print "Progress " progress "%: " message)
; Outputs Progress 100%: All done
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

### `(switch (comparisons...))`
This creates a conditional block, this is what the `if` and `unless` statements are built upon. This isn't quite like a C-language style `switch` as it's not focused on a single value but rather it is a series of comparison calls until one is found to be true.

```lisp
(define width 100)
(define length 70)
(switch
    ((< width 70) (print "Not wide enough"))
    ((< length 70) (print "Not long enough"))

    ; The true line is considered the else block
    (true (print "All good"))
)

; Outputs
; Not long enough
```

Internally each comparison is run in order until one equals `true` and then the rest are skipped. It does handle the `true` part specially and won't check if `true` equals `true`. If no block equals true then nothing happens.

```lisp
(function menu (timeOfDay)
    (print "For: " timeOfDay)
    (switch
        ((< timeOfDay "09:00")
            (print "Too early")
            (print "Come back later")
        )
        ((< timeOfDay "15:00")
            (print "Welcome")
            (print "Please choose a menu item")
        )
        ((< timeOfDay "19:00")
            (print "Good evening")
            (print "Please choose a menu item")
        )
        (true
            (print "Sorry we are closed")
        )
    )
    (print "Thank you")
)

(menu "08:00")
; Outputs
; For: 08:00
; Too early
; Come back later
; Thank you

(menu "12:00")
; Outputs
; For: 12:00
; Welcome
; Please choose a menu item
; Thank you

(menu "16:00")
; Outputs
; For: 16:00
; Good evening
; Please choose a menu item
; Thank you

(menu "20:00")
; Outputs
; For: 20:00
; Sorry we are closed
; Thank you
```

Switch statements like the `if` and `unless` statements can be used as an expression for getting a value as well. Here a function simply returns the value of a `switch`.

```lisp
(function getProgress (progress)
    (return (switch
        ((< progress 5) "Not even started")
        ((< progress 25) "A little bit done")
        ((< progress 50) "Somewhat done")
        ((< progress 75) "It's getting there")
        ((< progress 100) "Almost done")
        ((== progress 100) "All done!")
        (true "Unknown progress")
    ))
)

(define i 0)
(loop (<= i 5)
    (define progress (* i 20))
    (print "Progress " progress "%: " (getProgress progress))
    (++ i)
)

; Outputs
; Progress 0%: Not even started
; Progress 20%: A little bit done
; Progress 40%: Somewhat done
; Progress 60%: It's getting there
; Progress 80%: Almost done
; Progress 100%: All done!
```

An even more inlined version of the one above, I wouldn't recommend a long switch statement being used inlined but it does work.

```lisp
(define i 0)
(loop (<= i 5)
    (define progress (* i 20))
    (print "Progress " progress "%: " (switch
        ((< progress 5) "Not even started")
        ((< progress 25) "A little bit done")
        ((< progress 50) "Somewhat done")
        ((< progress 75) "It's getting there")
        ((< progress 100) "Almost done")
        ((== progress 100) "All done!")
        (true "Unknown progress")
    ))
    (++ i)
)

; Outputs
; Progress 0%: Not even started
; Progress 20%: A little bit done
; Progress 40%: Somewhat done
; Progress 60%: It's getting there
; Progress 80%: Almost done
; Progress 100%: All done!
```

### `(function name? (parameterList) (codeBody))`
Creates a new function value, takes a parameter list, the list itself is required but it can be empty.

The parameter list itself is parsed only as a list of strings.

The `name` is optional and if it's left out the function will be anonymous.
Additionally if the name is present and the function is not being used a value in a function call, the function will also be defined.

By default a function is created as a `const`.

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
[Info]: Minimum Number: 10
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
Alan Lawrey 2023
