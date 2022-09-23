#include "standard_value_library.hpp"

#include "../virtual_machine.hpp"
#include "../utils.hpp"

namespace stack_vm
{
    const std::string &standard_value_library::handle_name = "value";

    void standard_value_library::add_handler(virtual_machine &vm)
    {
        vm.add_run_handler(handle_name, handler);
    }

    void standard_value_library::handler(const std::string &command, virtual_machine &vm)
    {
        switch (hash(command))
        {
            case hash("typeof"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(std::make_shared<std::string>(top.type()));
                break;
            }
            case hash("toString"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(std::make_shared<std::string>(top.to_string()));
            }
        }
    }
} // stack_vm