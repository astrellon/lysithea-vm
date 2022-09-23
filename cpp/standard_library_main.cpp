#include <iostream>

#include <random>
#include <fstream>

#include "src/virtual_machine.hpp"
#include "src/assembler.hpp"

#define M_DEG_TO_RAD M_PI / 180.0

using namespace stack_vm;

void value_handler(const std::string &command, virtual_machine &vm)
{
    switch (hash(command))
    {
        case hash("typeof"):
        {
            const auto &top = vm.peek_stack();
            vm.push_stack(std::make_shared<std::string>(top.type()));
            break;
        }
        case hash("toString"):
        {
            const auto &top = vm.peek_stack();
            vm.push_stack(std::make_shared<std::string>(top.to_string()));
        }
    }
}

void comparison_handler(const std::string &command, virtual_machine &vm)
{
    switch (hash(command))
    {
        case hash(">"):
        case hash("greater"):
        {
            const auto &right = vm.pop_stack();
            const auto &left = vm.pop_stack();
            vm.push_stack(left.compare(right) > 0);
            break;
        }
        case hash(">="):
        case hash("greaterEquals"):
        {
            const auto &right = vm.pop_stack();
            const auto &left = vm.pop_stack();
            vm.push_stack(left.compare(right) >= 0);
            break;
        }
        case hash("=="):
        case hash("equals"):
        {
            const auto &right = vm.pop_stack();
            const auto &left = vm.pop_stack();
            vm.push_stack(left.compare(right) == 0);
            break;
        }
        case hash("!="):
        case hash("notEquals"):
        {
            const auto &right = vm.pop_stack();
            const auto &left = vm.pop_stack();
            vm.push_stack(left.compare(right) != 0);
            break;
        }
        case hash("<"):
        case hash("less"):
        {
            const auto &right = vm.pop_stack();
            const auto &left = vm.pop_stack();
            vm.push_stack(left.compare(right) < 0);
            break;
        }
        case hash("<="):
        case hash("lessEquals"):
        {
            const auto &right = vm.pop_stack();
            const auto &left = vm.pop_stack();
            vm.push_stack(left.compare(right) <= 0);
            break;
        }
        case hash("!"):
        case hash("not"):
        {
            const auto &top = vm.pop_stack();
            vm.push_stack(!top.get_bool());
            break;
        }
    }
}

void string_handler(const std::string &command, virtual_machine &vm)
{
    switch (hash(command))
    {
        case hash("append"):
        {
            const auto &right = vm.pop_stack();
            const auto &left = vm.pop_stack();
            auto str = std::make_shared<std::string>(left.to_string() + right.to_string());
            vm.push_stack(str);
            break;
        }
        case hash("prepend"):
        {
            const auto &right = vm.pop_stack();
            const auto &left = vm.pop_stack();
            auto str = std::make_shared<std::string>(right.to_string() + left.to_string());
            vm.push_stack(str);
            break;
        }
    }
}

void math_handler(const std::string &command, virtual_machine &vm)
{
    switch (hash(command))
    {
        case hash("+"):
        case hash("add"):
        {
            const auto &right = vm.pop_stack();
            const auto &left = vm.pop_stack();
            vm.push_stack(left.get_number() + right.get_number());
            break;
        }
        case hash("-"):
        case hash("sub"):
        {
            const auto &right = vm.pop_stack();
            const auto &left = vm.pop_stack();
            vm.push_stack(left.get_number() - right.get_number());
            break;
        }
        case hash("*"):
        case hash("mul"):
        {
            const auto &right = vm.pop_stack();
            const auto &left = vm.pop_stack();
            vm.push_stack(left.get_number() * right.get_number());
            break;
        }
        case hash("/"):
        case hash("command"):
        {
            const auto &right = vm.pop_stack();
            const auto &left = vm.pop_stack();
            vm.push_stack(left.get_number() / right.get_number());
            break;
        }
        case hash("sin"):
        {
            const auto &top = vm.pop_stack();
            vm.push_stack(sin(top.get_number()));
            break;
        }
        case hash("sinDeg"):
        {
            const auto &top = vm.pop_stack();
            vm.push_stack(sin(M_DEG_TO_RAD * top.get_number()));
            break;
        }
    }
}

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
    vm.add_run_handler("comp", comparison_handler);
    vm.add_run_handler("string", string_handler);
    vm.add_run_handler("math", math_handler);
    vm.add_run_handler("value", value_handler);
    vm.add_scopes(parsed_scopes);
    vm.set_current_scope("Main");
    vm.running = true;

    while (vm.running && !vm.paused)
    {
        vm.step();
    }

    return 0;
}