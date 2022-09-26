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
            case hash("keys"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(keys(*top.get_object()));
                break;
            }
            case hash("values"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(values(*top.get_object()));
                break;
            }
            case hash("length"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(static_cast<int>(top.get_object()->size()));
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

    value standard_object_library::keys(const object_value &target)
    {
        auto arr = std::make_shared<array_value>();
        for (auto iter : target)
        {
            arr->push_back(iter.first);
        }
        return arr;
    }
    value standard_object_library::values(const object_value &target)
    {
        auto arr = std::make_shared<array_value>();
        for (auto iter : target)
        {
            arr->push_back(iter.second);
        }
        return arr;
    }

} // stack_vm