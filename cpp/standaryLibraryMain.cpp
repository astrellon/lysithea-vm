#include <iostream>

#include <random>
#include <fstream>

#include "src/virtual_machine.hpp"
#include "src/assembler.hpp"

using namespace stack_vm;

bool standard_handler(const value &input, virtual_machine &vm)
{
    if (!input.is_string())
    {
        return false;
    }

    const auto &command = *input.get_string().get();
    if (command == "+")
    {

    }
}

bool run_handler(const value &input, virtual_machine &vm)
{
    if (!input.is_string())
    {
        return false;
    }

    const auto &command = *input.get_string().get();

    if (command == "print")
    {
        auto total = vm.pop_stack();
        auto str = total.to_string();

        std::cout << "Print: " << total.get_number() << "\n";
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
    json_input.open("../../examples/testStandardLibrary.json");
    if (!json_input)
    {
        std::cout << "Could not find file to open!\n";
        return -1;
    }

    nlohmann::json json;
    json_input >> json;

    auto parsed_scopes = assembler::parse_scopes(json);

    virtual_machine vm(64, std::vector<stack_vm::run_handler>{ standard_handler, ::run_handler });
    vm.add_scopes(parsed_scopes);

    vm.run("Main");

    return 0;
}