#include <iostream>

#include <random>
#include <fstream>

// #include "src/virtual_machine.hpp"
// #include "src/assembler.hpp"
#include "src/parser.hpp"

using namespace stack_vm;

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

// int mainOld()
// {
//     std::ifstream json_input;
//     json_input.open("../../examples/perfTest.json");
//     if (!json_input)
//     {
//         std::cout << "Could not find file to open!\n";
//         return -1;
//     }

//     nlohmann::json json;
//     json_input >> json;

//     auto parsed_scopes = assembler::parse_scopes(json);

//     virtual_machine vm(64, runHandler);
//     vm.add_scopes(parsed_scopes);
//     vm.set_current_scope("Main");
//     vm.running = true;

//     while (vm.running && !vm.paused)
//     {
//         vm.step();
//     }

//     return 0;
// }

int main()
{
    std::ifstream input_file;
    input_file.open("../../examples/perfTest.lisp");
    if (!input_file)
    {
        std::cout << "Could not find file to open!\n";
        return -1;
    }

    parser parse(input_file);

    std::string token;
    while (parse.move_next(token))
    {
        std::cout << "Token: |" << token << "|\n";
    }

    std::cout << "- Done\n";

    return 0;
}