#include <iostream>

#include <random>
#include <fstream>

#include "src/virtual_machine.hpp"
#include "src/assembler.hpp"
#include "src/standard_library/standard_library.hpp"

using namespace stack_vm;

void run_handler(const std::string &command, virtual_machine &vm)
{
    if (command == "print")
    {
        auto total = vm.pop_stack();
        auto str = total.to_string();

        std::cout << "Print: " << total.to_string() << "\n";
    }
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

    virtual_machine vm(64, ::run_handler);
    standard_library::add_to_virtual_machine(vm);
    vm.add_scopes(parsed_scopes);
    vm.set_current_scope("Array");
    vm.running = true;

    while (vm.running && !vm.paused)
    {
        vm.step();
    }

    return 0;
}