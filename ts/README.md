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

```ts
import { VirtualMachine, Assembler, LibraryType, addToScope } from 'lysithea-vm';

const assembler = new Assembler();
addToScope(assembler.builtinScope, LibraryType.all);

const vm = new VirtualMachine(8);

try
{
    const script = assembler.parseFromText('CodeExample', `
        (define num1 18)
        (define num2 3)
        (print "Div: " (/ num1 num2))
        (print "Mul: " (* num1 num3))
    `);
    vm.execute(script);
}
catch (err)
{
    console.error(err);
}
```
## More details
For more details, see the main [repositories documentation](https://github.com/astrellon/lysithea-vm).

## License
MIT

## Author
Alan Lawrey 2023