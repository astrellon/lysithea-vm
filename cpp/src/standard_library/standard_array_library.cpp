#include "standard_array_library.hpp"

#include <sstream>

#include "../virtual_machine.hpp"
#include "../utils.hpp"
#include "./standard_comparison_library.hpp"

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
            case hash("set"):
            {
                const auto &value = vm.pop_stack();
                const auto &index = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(set(top, index.get_int(), value));
                break;
            }
            case hash("insert"):
            {
                const auto &value = vm.pop_stack();
                const auto &index = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(insert(top, index.get_int(), value));
                break;
            }
            case hash("insertFlatten"):
            {
                const auto &value = vm.pop_stack();
                const auto &index = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(insert_flatten(top, index.get_int(), *value.get_array()));
                break;
            }
            case hash("removeAt"):
            {
                const auto &index = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(remove_at(top, index.get_int()));
                break;
            }
            case hash("remove"):
            {
                const auto &value = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(remove(top, value));
                break;
            }
            case hash("removeAll"):
            {
                const auto &value = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(remove_all(top, value));
                break;
            }
            case hash("contains"):
            {
                const auto &value = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(contains(top, value));
                break;
            }
            case hash("indexOf"):
            {
                const auto &value = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(index_of(top, value));
                break;
            }
            case hash("sublist"):
            {
                const auto &length = vm.pop_stack();
                const auto &index = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(sublist(top, index.get_int(), length.get_int()));
                break;
            }
        }
    }

    value standard_array_library::append(const value &target, const value &input)
    {
        auto arr = copy(target);
        arr->push_back(input);
        return arr;
    }
    value standard_array_library::prepend(const value &target, const value &input)
    {
        return insert(target, 0, input);
    }
    value standard_array_library::set(const value &target, int index, const value &input)
    {
        auto arr = copy(target);
        if (index < 0)
        {
            (*arr)[arr->size() + index + 1] = input;
        }
        else
        {
            (*arr)[index] = input;
        }
        return arr;
    }
    value standard_array_library::insert(const value &target, int index, const value &input)
    {
        auto arr = copy(target);
        arr->insert(get_iter(*arr, index), input);
        return arr;
    }
    value standard_array_library::insert_flatten(const value &target, int index, const array_value &input)
    {
        auto arr = copy(target);
        arr->insert(get_iter(*arr, index), input.begin(), input.end());
        return arr;
    }
    value standard_array_library::remove_at(const value &target, int index)
    {
        auto arr = copy(target);
        arr->erase(get_iter(*arr, index));
        return arr;
    }
    value standard_array_library::remove(const value &target, const value &value)
    {
        auto orig_arr = target.get_array();
        for (auto i = 0; i < orig_arr->size(); i++)
        {
            if (standard_comparison_library::equals(orig_arr->at(i), value))
            {
                auto arr = copy(target);
                arr->erase(arr->begin() + i);
                return arr;
            }
        }

        return orig_arr;
    }
    value standard_array_library::remove_all(const value &target, const value &value)
    {
        auto orig_arr = target.get_array();
        auto arr = orig_arr;
        auto arr_offset = 0;

        for (auto i = 0; i < orig_arr->size(); i++)
        {
            if (standard_comparison_library::equals(orig_arr->at(i), value))
            {
                if (arr.get() == orig_arr.get())
                {
                    arr = copy(target);
                }

                arr->erase(arr->begin() + (i - arr_offset));
                arr_offset--;
            }
        }

        return arr;
    }
    value standard_array_library::contains(const value &target, const value &value)
    {
        auto arr = target.get_array();
        for (auto i = 0; i < arr->size(); i++)
        {
            if (standard_comparison_library::equals(arr->at(i), value))
            {
                return true;
            }
        }

        return false;
    }

    value standard_array_library::index_of(const value &target, const value &value)
    {
        auto arr = target.get_array();
        for (auto i = 0; i < arr->size(); i++)
        {
            if (standard_comparison_library::equals(arr->at(i), value))
            {
                return i;
            }
        }

        return -1;
    }
    value standard_array_library::sublist(const value &target, int index, int length)
    {
        if (length == 0 || index < 0 || !target.is_array())
        {
            return value::empty_array;
        }

        auto arr = target.get_array();
        auto offset = index;
        if (index < 0)
        {
            offset = arr->size() + index + 1;
        }

        if (length < 0 || offset + length > arr->size())
        {
            return std::make_shared<array_value>(arr->begin() + offset, arr->end());
        }
        return std::make_shared<array_value>(arr->begin() + offset, arr->begin() + (offset + length));
    }
}