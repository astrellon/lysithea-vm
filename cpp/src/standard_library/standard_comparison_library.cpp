#include "standard_comparison_library.hpp"

#include "../virtual_machine.hpp"
#include "../utils.hpp"

namespace stack_vm
{
    const std::string &standard_comparison_library::handle_name = "comp";

    void standard_comparison_library::add_handler(virtual_machine &vm)
    {
        vm.add_run_handler(handle_name, handler);
    }

    void standard_comparison_library::handler(const std::string &command, virtual_machine &vm)
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
} // stack_vm