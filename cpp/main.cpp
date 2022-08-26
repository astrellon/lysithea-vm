#include <iostream>

#include <random>

#include "src/value.hpp"
#include "src/virtual_machine.hpp"

using namespace stack_vm;

std::random_device _rd;
std::mt19937 _rand(_rd());

int counter = 0;

void runHandler(const Value &value, VirtualMachine &vm)
{
    const auto command = value.to_string();
    if (command == "rand")
    {
        std::uniform_real_distribution<double> dist(0.0, 1.0);
        vm.pushStack(Value(dist(_rand)));
    }
    else if (command == "add")
    {
        auto num1 = vm.popStack();
        auto num2 = vm.popStack();
        vm.pushStack(Value(std::get<double>(num1.data) + std::get<double>(num2.data)));
    }
    else if (command == "isDone")
    {
        counter++;
        vm.pushStack(Value(counter >= 1000000));
    }
    else if (command == "done")
    {
        auto total = std::get<double>(vm.popStack().data);
        std::cout << "Done: " << total << "\n";
    }
}

int main()
{
    std::vector<CodeLine> code;
    code.emplace_back(Operator::Push, Value(0.0));
    code.emplace_back(Operator::Push, Value(array_value{Value(""), Value("Step")}));
    code.emplace_back(Operator::Call);
    code.emplace_back(Operator::Push, Value("isDone"));
    code.emplace_back(Operator::Run);
    code.emplace_back(Operator::Push, Value(":Start"));
    code.emplace_back(Operator::JumpFalse);
    code.emplace_back(Operator::Push, Value("done"));
    code.emplace_back(Operator::Run);

    std::map<std::string, int> labels;
    labels[":Start"] = 1;
    auto scope = std::make_shared<Scope>("Main", code, labels);

    std::vector<CodeLine> code2;
    code2.emplace_back(Operator::Push, Value("rand"));
    code2.emplace_back(Operator::Run);
    code2.emplace_back(Operator::Push, Value("rand"));
    code2.emplace_back(Operator::Run);
    code2.emplace_back(Operator::Push, Value("add"));
    code2.emplace_back(Operator::Run);
    code2.emplace_back(Operator::Push, Value("add"));
    code2.emplace_back(Operator::Run);
    code2.emplace_back(Operator::Return);
    auto scope2 = std::make_shared<Scope>("Step", code2);

    VirtualMachine vm(64, runHandler);
    vm.addScope(scope);
    vm.addScope(scope2);

    vm.run("Main");

    return 0;
}