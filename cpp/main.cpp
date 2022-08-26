#include <iostream>

#include <random>

#include "src/value.hpp"
#include "src/virtual_machine.hpp"

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
    std::vector<code_line> code;
    code.emplace_back(vm_operator::push, value(0.0));
    code.emplace_back(vm_operator::push, value(array_value{value(""), value("Step")}));
    code.emplace_back(vm_operator::call);
    code.emplace_back(vm_operator::push, value("isDone"));
    code.emplace_back(vm_operator::run);
    code.emplace_back(vm_operator::push, value(":Start"));
    code.emplace_back(vm_operator::jump_false);
    code.emplace_back(vm_operator::push, value("done"));
    code.emplace_back(vm_operator::run);

    std::map<std::string, int> labels;
    labels[":Start"] = 1;
    auto mainScope = std::make_shared<scope>("Main", code, labels);

    std::vector<code_line> code2;
    code2.emplace_back(vm_operator::push, value("rand"));
    code2.emplace_back(vm_operator::run);
    code2.emplace_back(vm_operator::push, value("rand"));
    code2.emplace_back(vm_operator::run);
    code2.emplace_back(vm_operator::push, value("add"));
    code2.emplace_back(vm_operator::run);
    code2.emplace_back(vm_operator::push, value("add"));
    code2.emplace_back(vm_operator::run);
    code2.emplace_back(vm_operator::call_return);
    auto scope2 = std::make_shared<scope>("Step", code2);

    virtual_machine vm(64, runHandler);
    vm.add_scope(mainScope);
    vm.add_scope(scope2);

    vm.run("Main");

    return 0;
}