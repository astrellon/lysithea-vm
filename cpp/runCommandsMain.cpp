#include <iostream>

#include <fstream>

#include "src/virtual_machine.hpp"
#include "src/assembler.hpp"

using namespace stack_vm;

bool runHandler(const value &input, virtual_machine &vm)
{
    if (input.is_string())
    {
        const auto &command = *input.get_string().get();
        if (command == "add")
        {
            auto num1 = vm.pop_stack();
            auto num2 = vm.pop_stack();
            vm.push_stack(value(num1.get_number() + num2.get_number()));
        }
        else if (command == "print")
        {
            auto top = vm.pop_stack();
            std::cout << "Print: " << top.to_string() << "\n";
        }
        else
        {
            std::cout << "Error! Unknown string run command: " << command << "\n";
        }
    }
    else if (input.is_number())
    {
        auto num = input.get_number();
        std::cout << "The number: " << num << "\n";
    }
    else if (input.is_bool())
    {
        auto boolean = input.get_bool();
        std::cout << "The boolean: " << boolean << "\n";
    }
    else if (input.is_array())
    {
        auto top = vm.pop_stack();
        std::cout << "The array command: " << input.to_string() << "\n";
        std::cout << "- Top array value: " << top.to_string() << "\n";
    }
    else if (input.is_object())
    {
        std::cout << "The object command: " << input.to_string() << "\n";
    }
    else
    {
        return false;
    }

    return true;
}

int main()
{
    std::ifstream json_input;
    json_input.open("../../examples/testRunCommands.json");
    if (!json_input)
    {
        std::cout << "Could not find file to open!\n";
        return -1;
    }

    nlohmann::json json;
    json_input >> json;

    auto parsed_scopes = assembler::parse_scopes(json);

    virtual_machine vm(64, runHandler);
    vm.add_scopes(parsed_scopes);

    vm.run("Main");

    return 0;
}