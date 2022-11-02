#include <iostream>

#include <random>
#include <fstream>

#include "src/assembler.hpp"
#include "src/parser.hpp"
#include "src/values/values.hpp"
#include "src/virtual_machine.hpp"

// using namespace stack_vm;

std::random_device _rd;
std::mt19937 _rand(_rd());

int counter = 0;

// void runHandler(const std::string &command, virtual_machine &vm)
// {
//     if (command == "rand")
//     {
//         std::uniform_real_distribution<double> dist(0.0, 1.0);
//         vm.push_stack(value(dist(_rand)));
//     }
//     else if (command == "add")
//     {
//         auto num1 = vm.pop_stack();
//         auto num2 = vm.pop_stack();
//         vm.push_stack(value(num1.get_number() + num2.get_number()));
//     }
//     else if (command == "isDone")
//     {
//         counter++;
//         vm.push_stack(value(counter >= 1000000));
//     }
//     else if (command == "done")
//     {
//         auto total = vm.pop_stack();
//         auto str = total.to_string();

//         std::cout << "Done: " << total.get_number() << "\n";
//     }
// }

std::shared_ptr<stack_vm::scope> create_custom_scope()
{
    auto result = std::make_shared<stack_vm::scope>();

    result->define("rand", [](stack_vm::virtual_machine &vm, const stack_vm::array_value &args) -> void
    {
        std::uniform_real_distribution<double> dist(0.0, 1.0);
        vm.push_stack(dist(_rand));
    });

    result->define("add", [](stack_vm::virtual_machine &vm, const stack_vm::array_value &args) -> void
    {
        auto num1 = vm.pop_stack<stack_vm::number_value>();
        auto num2 = vm.pop_stack<stack_vm::number_value>();
        vm.push_stack(num1->value + num1->value);
    });

    result->define("isDone", [](stack_vm::virtual_machine &vm, const stack_vm::array_value &args) -> void
    {
        counter++;
        vm.push_stack(counter >= 1000000);
    });

    result->define("done", [](stack_vm::virtual_machine &vm, const stack_vm::array_value &args) -> void
    {
        auto total = vm.pop_stack<stack_vm::number_value>();

        std::cout << "Done: " << total->value << "\n";
    });

    return result;
}

int main()
{
    std::ifstream input_file;
    input_file.open("../../examples/perfTest.lisp");
    if (!input_file)
    {
        std::cout << "Could not find file to open!\n";
        return -1;
    }

    auto custom_scope = create_custom_scope();

    auto parsed = stack_vm::parser::read_from_stream(input_file);
    stack_vm::assembler assembler;
    assembler.builtin_scope.combine_scope(custom_scope);

    auto script = assembler.parse_from_value(parsed);

    stack_vm::virtual_machine vm(16);
    vm.execute(script);

    return 0;
}