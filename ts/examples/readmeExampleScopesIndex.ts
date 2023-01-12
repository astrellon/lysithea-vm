import { VirtualMachine, Assembler, addToScope, LibraryType, Script, ObjectValueMap, BuiltinFunctionValue, IArrayValue, isArrayValue, ObjectValue, Scope } from '../src/index';
import { assertScope } from '../src/standardLibrary/standardAssertLibrary';

const randFuncs: ObjectValueMap =
{
    int: new BuiltinFunctionValue((vm, args) =>
    {
        const min = args.getNumber(0);
        const max = args.getNumber(1);
        const diff = max - min + 1;
        vm.pushStackNumber(Math.floor(Math.random() * diff) + min);
    }, 'random.int'),
    bool: new BuiltinFunctionValue((vm, args) =>
    {
        vm.pushStackBool(Math.random() > 0.5);
    }, 'random.bool')
}
const randObj = new ObjectValue(randFuncs);

function noScope()
{
    const assembler = new Assembler();
    assembler.builtinScope.tryDefine('random', randObj);
    addToScope(assembler.builtinScope, LibraryType.all);

    const vm = new VirtualMachine(8);

    const script = assembler.parseFromText('randomScript', `
        (print 'Int 0-100 ' (random.int 0 100))
        (print 'Int 0-100 ' (random.int 0 100))
        (print 'Bool ' (random.bool))
        (print 'Bool ' (random.bool))
    `);

    console.log('No Scope');
    vm.execute(script);
}

function assemblerScope()
{
    const randomScope = new Scope();
    randomScope.tryDefine('random', randObj);

    const assembler = new Assembler();
    addToScope(assembler.builtinScope, LibraryType.all);
    assembler.builtinScope.combineScope(randomScope);

    const vm = new VirtualMachine(8);

    const script = assembler.parseFromText('randomScript', `
        (print 'Int 0-100 ' (random.int 0 100))
        (print 'Int 0-100 ' (random.int 0 100))
        (print 'Bool ' (random.bool))
        (print 'Bool ' (random.bool))
    `);

    console.log('Assembler Scope');
    vm.execute(script);
}

function vmScope()
{
    const randomScope = new Scope();
    randomScope.tryDefine('random', randObj);

    const assembler = new Assembler();
    addToScope(assembler.builtinScope, LibraryType.all);

    const vm = new VirtualMachine(8);
    vm.globalScope.combineScope(randomScope);

    const script = assembler.parseFromText('randomScript', `
        (print 'Int 0-100 ' (random.int 0 100))
        (print 'Int 0-100 ' (random.int 0 100))
        (print 'Bool ' (random.bool))
        (print 'Bool ' (random.bool))
    `);

    console.log('Virtual Machine Scope');
    vm.execute(script);
}

noScope();
assemblerScope();
vmScope();