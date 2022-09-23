#include "standard_array_library.hpp"

#include <sstream>

#include "../virtual_machine.hpp"
#include "../utils.hpp"

namespace stack_vm
{
    const std::string &standard_array_library::handle_name = "array";

    void standard_array_library::add_handler(virtual_machine &vm)
    {
        vm.add_run_handler(handle_name, handler);
    }

    void standard_array_library::handler(const std::string &command, virtual_machine &vm)
    {
        switch (hash(command))
        {
            case hash("append"):
            {
                const auto &right = vm.pop_stack();
                const auto &left = vm.pop_stack();
                vm.push_stack(append(left, right));
                break;
            }
            case hash("prepend"):
            {
                const auto &right = vm.pop_stack();
                const auto &left = vm.pop_stack();
                vm.push_stack(prepend(left, right));
                break;
            }
            case hash("length"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(static_cast<double>(top.get_array()->size()));
                break;
            }
            case hash("get"):
            {
                const auto &index = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(top.get_array()->at(index.get_int()));
                break;
            }
            // case hash("set"):
            // {
            //     const auto &value = vm.pop_stack();
            //     const auto &index = vm.pop_stack();
            //     const auto &top = vm.pop_stack();
            //     vm.push_stack(set(top, index.get_int(), value.to_string()));
            //     break;
            // }
            // case hash("insert"):
            // {
            //     const auto &value = vm.pop_stack();
            //     const auto &index = vm.pop_stack();
            //     const auto &top = vm.pop_stack();
            //     vm.push_stack(insert(top, index.get_int(), value.to_string()));
            //     break;
            // }
        }
    }

    value standard_array_library::append(const value &target, const value &input)
    {
        auto arr = std::make_shared<array_value>(target.get_array());
        arr->push_back(input);
        return arr;
    }
    value standard_array_library::prepend(const value &target, const value &input)
    {
        auto arr = std::make_shared<array_value>(target.get_array());
        arr->insert(arr->begin(), input);
        return arr;
    }
}