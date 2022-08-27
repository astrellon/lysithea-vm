#include <iostream>

#include <random>
#include <fstream>

#include "src/virtual_machine.hpp"
#include "src/assembler.hpp"

using namespace stack_vm;

std::random_device _rd;
std::mt19937 _rand(_rd());

int counter = 0;

void runHandler(const value &input, virtual_machine &vm)
{
    const auto command = input.to_string();
    if (command == "rand")
    {
        std::uniform_real_distribution<double> dist(0.0, 1.0);
        vm.push_stack(value(dist(_rand)));
    }
    else if (command == "add")
    {
        auto num1 = vm.pop_stack();
        auto num2 = vm.pop_stack();
        vm.push_stack(value(std::get<double>(num1.data) + std::get<double>(num2.data)));
    }
    else if (command == "isDone")
    {
        counter++;
        vm.push_stack(value(counter >= 1000000));
    }
    else if (command == "done")
    {
        auto total = std::get<double>(vm.pop_stack().data);
        std::cout << "Done: " << total << "\n";
    }
}

int main()
{
    std::cout << "Size: " << sizeof(value) << "\n";
    std::ifstream json_input;
    json_input.open("perfTest.json");
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