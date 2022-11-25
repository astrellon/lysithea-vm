# TypeScript
The TypeScript port of the Lysithea VM.

## Examples
The core code is under `./src` folder with ported examples under the `./examples` folder.

```sh
$ npm install

$ npm run perfTest
$ npm run runCommands
$ npm run diagTree
```

## Build

The VM can be built into a single file using the following:
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

    result.defineFunc("print", (vm, args) =>
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

## More details
For more details, see the main [repositories documentation](https://github.com/astrellon/lysithea-vm).

## License
MIT

## Author
Alan Lawrey 2022