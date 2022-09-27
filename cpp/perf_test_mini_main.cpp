#include <iostream>

#include <random>

#include "src/virtual_machine_mini.hpp"

using namespace stack_vm;

std::random_device _rd;
std::mt19937 _rand(_rd());

int counter = 0;

void runHandler(const std::string &command, virtual_machine_mini &vm)
{
    if (command == "rand")
    {
        std::uniform_real_distribution<double> dist(0.0, 1.0);
        vm.push_stack(value(dist(_rand)));
    }
    else if (command == "add")
    {
        auto num1 = vm.pop_stack();
        auto num2 = vm.pop_stack();
        vm.push_stack(value(num1.get_number() + num2.get_number()));
    }
    else if (command == "isDone")
    {
        counter++;
        vm.push_stack(value(counter >= 1000000));
    }
    else if (command == "done")
    {
        auto total = vm.pop_stack();
        auto str = total.to_string();

        std::cout << "Done: " << total.get_number() << "\n";
        vm.running = false;
    }
}

int main()
{
    auto main_scope = std::make_shared<scope>("Main", std::vector<code_line>
    {
        { vm_operator::push, value(0) },
        { vm_operator::call, value(":Step") },
        { vm_operator::run, value("isDone") },
        { vm_operator::jump_false, value(":Start") },
        { vm_operator::run, value("done") },

        { vm_operator::run, value("rand") },
        { vm_operator::run, value("rand") },
        { vm_operator::run, value("add") },
        { vm_operator::run, value("add") },
        { vm_operator::call_return }
    },
    std::map<std::string, int>
    {
        { ":Start", 1 },
        { ":Step", 5 }
    });

    virtual_machine_mini vm(64, runHandler);
    vm.set_current_scope(main_scope);
    vm.running = true;

    while (vm.running)
    {
        vm.step();
    }

    return 0;
}