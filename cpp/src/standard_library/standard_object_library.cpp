#include "standard_object_library.hpp"

#include "../virtual_machine.hpp"

namespace stack_vm
{
    const std::string &standard_object_library::handle_name = "object";

    void standard_object_library::add_handler(virtual_machine &vm)
    {
        vm.add_run_handler(handle_name, handler);
    }

    void standard_object_library::handler(const std::string &command, virtual_machine &vm)
    {
        switch (hash(command))
        {
            case hash("set"):
            {
                const auto &value = vm.pop_stack();
                const auto &key = vm.pop_stack();
                const auto &obj = vm.pop_stack();
                vm.push_stack(set(obj, key.to_string(), value));
                break;
            }
            case hash("get"):
            {
                const auto &key = vm.pop_stack();
                const auto &obj = vm.pop_stack();
                vm.push_stack(get(obj, key.to_string()));
                break;
            }
        }
    }

    value standard_object_library::set(const value &target, const std::string &key, const value &input)
    {
        auto obj = copy(target);
        (*obj)[key] = input;
        return obj;
    }
    value standard_object_library::get(const value &target, const std::string &key)
    {
        const auto &obj = target.get_object();
        auto find = obj->find(key);
        if (find != obj->end())
        {
            return find->second;
        }
        else
        {
            return value::null;
        }
    }
} // stack_vm