# TypeScript
The TypeScript port of the Lysithea VM.

## Examples
The core code is under `./src` folder with ported examples under the `./examples` folder.

```sh
$ npm install

$ npm run perfTest
$ npm run stdLib
$ npm run diagTree
```

## Build

The TypeScript can be built for JavaScript.
```sh
$ npm run build
```

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
```typescript
function createScope(): Scope
{
    var result = new Scope();

    result.tryDefineFunc("print", (vm, args) =>
    {
        console.log(args.value.map(c => c.toString()).join(''));
    });

    return result;
}
```

The program will output

```
Result: 17
```

## Simple Program
To make use of Lysithea in a TS project you can either copy of the contents of the `src` folder into your project and make changes to it as you see fit. Or get a copy of it from [npm](https://www.npmjs.com/package/lysithea-vm).

```ts
import { VirtualMachine, Assembler, LibraryType, addToScope } from 'lysithea-vm';

// The assembler is responsible for turning script text into an executable script.
const assembler = new Assembler();

// By default only basic math operators will be understood, this adds all the
// standard libraries to the assembler. You can also add your own functions and values.
addToScope(assembler.builtinScope, LibraryType.all);

// Now we can assemble a script.
// The first argument lets it know the source of the text for throwing errors in a specific file.
// The second is the script text itself.
const script = assembler.parseFromText('CodeExample', '(print "Result: " (+ 5 12))');

// To actually execute a script it needs a virtual machine.
// The VM actually holds all the information about a current execution such as the current
// call stack and code location.

// The argument indicates how big the stack and call stack can be.
// The call stack limits how many functions can be called from inside each other.
// The stack itself limits how many arguments can be used to call a function.
// These are two independent stacks.
const vm = new VirtualMachine(8);

// Internally the VM runs on a loop and you can either ask it to run until it's finished
vm.execute(script);
//
// OR
//

// You can control the loop yourself, you may want to do this when you want to do more advanced things
// like control how many steps can be executed at once for a long running process or by some other limiting factor.
vm.changeToScript(script);
vm.running = true;
vm.paused = false;
while (vm.running && !vm.paused)
{
    vm.step();
}

// Regardless of which execution path chosen it should log to the console
// Result: 17
```
This example skips worrying about catching any assembler errors or runtime errors.

## Adding Error Handling
Let's say you do want to do some error handling.

There's 3 types of errors:

**ParserError**: Basically was it able to tokenise the input, generally speaking the only errors here are mismatched brackets or attempting to use an expression as a maps key value.

This will contain a `trace` location as to where in the script this happened.

**AssemblerError**: When parsing the tokens do things make reasonable sense, have you given the right number of arguments to certain keywords (if/unless), have you attempted to redefine a constant, etc.

This will contain a `trace` location as to where in the script this happened.

**VirtualMachineError**: These are run time errors that weren't able to be caught by the assembler.

This will be things like functions not being found, labels not found, etc.

This will contain a `stackTrace` as a list of string locations at to where this occurred in the script based on function calls.

```ts
import { VirtualMachine, Assembler, LibraryType, addToScope } from 'lysithea-vm';

const assembler = new Assembler();
addToScope(assembler.builtinScope, LibraryType.all);

const vm = new VirtualMachine(8);

function tryExecute(vm: VirtualMachine, sourceName: string, text: string)
{
    try
    {
        const script = assembler.parseFromText(sourceName, text);
        vm.execute(script);

        return true;
    }
    catch (err)
    {
        if (err instanceof VirtualMachineError)
        {
            const stackTrace = err.stackTrace.join('\n');
            console.error(`Runtime Error: ${err.message}\n${stackTrace}`);
        }
        else if (err instanceof ParserError)
        {
            console.error(`Parser Error: ${err.message}\n${err.trace}`);
        }
        else if (err instanceof AssemblerError)
        {
            console.error(`Assembler Error: ${err.message}\n${err.trace}`);
        }
        return false;
    }
}

const runtimeErrorExample = `
    (define num1 18)
    (define num2 3)
    (print "Div: " (/ num1 num2))
    (print "Mul: " (* num1 num3))`;

if (!tryExecute(vm, 'RuntimeErrorExample', runtimeErrorExample))
{
    console.log('Oh no!');
}

/* Outputs
Div: 6
Runtime Error: global:12: Unable to get variable: num3
RuntimeErrorExample:5:28
4:     (print "Div: " (/ num1 num2))
5:     (print "Mul: " (* num1 num3))
                             ^----^

Oh no!
*/

const parserErrorExample = `
    (define num1 18)
    (define num2 3))
    (print "Div: " (/ num1 num2))
    (print "Mul: " (* num1 num3))`;

if (!tryExecute(vm, 'ParserErrorExample', parserErrorExample))
{
    console.log('Oh no!');
}

/* Outputs
Parser Error: 2:17 -> 3:0}: ): Unexpected ): ParserErrorExample:3:18
2:     (define num1 18)
3:     (define num2 3))
                   ^---^
4:     (print "Div: " (/ num1 num2))

Oh no!
*/

const assemblerErrorExample = `
    (const num1 18)
    (define num1 "Redefined")
    (define num2 3)
    (print "Div: " (/ num1 num2))
    (print "Mul: " (* num1 num3))`;

if (!tryExecute(vm, 'AssemblerErrorExample', assemblerErrorExample))
{
    console.log('Oh no!');
}

/* Outputs
Assembler Error: 2:12 -> 2:17}: num1: Attempting to define a constant: num1: AssemblerErrorExample:3:13
2:     (const num1 18)
3:     (define num1 "Redefined")
              ^----^
4:     (define num2 3)

Oh no!
*/
```

## Adding Extra Functionality
So by default the VM only supports basic arithmetic (`+`, `-`, `*`, `/`) and comparisons (`<`, `>`, `>=`, `<=`, `==`, `!=`). The standard library gives you some more functionality around manipulating some of the builtin types, like strings, lists and maps but that still doesn't let you do much.

So in general you'll be adding extra functionality to actually do something for your specific program.

There's two main ways of doing this, adding extra functions and values to the assembler and adding extra functions and values to the virtual machine.

The main difference being that when done at the assembler stage it lets it inline calls to those functions increasing performance and it means that even if the virtual machine in unaware of those function it won't need to, unless you want to dynamically find those functions.

Additionally this extra functionality can be grouped together into a `scope` which can then be combined with the assembler or virtual machines scope. Or you can define individual values.

## More details
For more details, see the main [repositories documentation](https://github.com/astrellon/lysithea-vm).

## License
MIT

## Author
Alan Lawrey 2023