#include "standard_string_library.hpp"

#include "../virtual_machine.hpp"
#include "../utils.hpp"

namespace stack_vm
{
    const std::string &standard_string_library::handle_name = "string";

    void standard_string_library::add_handler(virtual_machine &vm)
    {
        vm.add_run_handler(handle_name, handler);
    }

    void standard_string_library::handler(const std::string &command, virtual_machine &vm)
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
} // stack_vm