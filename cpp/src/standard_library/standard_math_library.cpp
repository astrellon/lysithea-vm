#include "standard_math_library.hpp"

#include <math.h>

#include "../virtual_machine.hpp"
#include "../utils.hpp"

#define M_DEG_TO_RAD M_PI / 180.0

namespace stack_vm
{
    const std::string &standard_math_library::handle_name = "math";

    void standard_math_library::add_handler(virtual_machine &vm)
    {
        vm.add_run_handler(handle_name, handler);
    }

    void standard_math_library::handler(const std::string &command, virtual_machine &vm)
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
} // stack_vm