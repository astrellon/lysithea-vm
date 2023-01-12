import {
    VirtualMachine,
    Assembler,
    addToScope,
    LibraryType,
    ObjectValueMap,
    BuiltinFunctionValue,
    ObjectValue,
    Scope,
    NumberValue,
    BuiltinFunctionCallback } from '../src/index';

const randomInt: BuiltinFunctionCallback = (vm, args) =>
{
    const min = args.getNumber(0);
    const max = args.getNumber(1);
    const diff = max - min + 1;
    vm.pushStackNumber(Math.floor(Math.random() * diff) + min);
};

const randomBool: BuiltinFunctionCallback = (vm, args) =>
{
    vm.pushStackBool(Math.random() > 0.5);
};

const randomFloat: BuiltinFunctionCallback = (vm, args) =>
{
    vm.pushStackNumber(Math.random());
};

// Create an object that contains will contain functions, however this is just
// a way to group things under a single object, kind of like a namespace, but it's only an object.
const randFuncs: ObjectValueMap =
{
    int: new BuiltinFunctionValue(randomInt, 'random.int'),
    bool: new BuiltinFunctionValue(randomBool, 'random.bool')
};
const randomObj = new ObjectValue(randFuncs);

// Create a scope to store all the values.
const randomScope = new Scope();

// Set the object onto the scope
randomScope.tryDefine('random', randomObj);

// You can also set a function directly onto the scope
randomScope.tryDefineFunc('randomFloat', randomFloat);

// You can define a simple value as well.
randomScope.tryDefine('seed', new NumberValue(1234));

// The actual code that will be used.
const scriptText = `
    (print 'Int 0-100 ' (random.int 0 100))
    (print 'Bool ' (random.bool))
    (print 'Random Value ' (randomFloat))
    (print 'Seed ' seed)
`;

// This does not make use of the randomScope object and instead directly adds the
// values onto the assembler.
function noScope()
{
    const assembler = new Assembler();
    assembler.builtinScope.tryDefine('random', randomObj);
    assembler.builtinScope.tryDefineFunc('randomFloat', randomFloat);
    assembler.builtinScope.tryDefine('seed', new NumberValue(1234));
    addToScope(assembler.builtinScope, LibraryType.all);

    const vm = new VirtualMachine(8);

    const script = assembler.parseFromText('randomScript', scriptText);
    vm.execute(script);
}

// Makes use of the randomScope to simplify adding values to the assemblers known values.
function assemblerScope()
{
    const assembler = new Assembler();
    addToScope(assembler.builtinScope, LibraryType.all);
    assembler.builtinScope.combineScope(randomScope);

    const vm = new VirtualMachine(8);

    const script = assembler.parseFromText('randomScript', scriptText);
    vm.execute(script);
}

// Makes use of the randomScope but adds the values to just the virtual machine.
function vmScope()
{
    const assembler = new Assembler();
    addToScope(assembler.builtinScope, LibraryType.all);

    const vm = new VirtualMachine(8);
    vm.globalScope.combineScope(randomScope);

    const script = assembler.parseFromText('randomScript', scriptText);
    vm.execute(script);
}

console.log('No Scope');
noScope();

console.log('\nAssembler Scope');
assemblerScope();

console.log('\nVirtual Machine Scope');
vmScope();