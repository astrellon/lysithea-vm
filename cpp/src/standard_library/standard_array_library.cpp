#include "standard_array_library.hpp"

#include <sstream>

#include "../virtual_machine.hpp"
#include "../values/object_value.hpp"
#include "../utils.hpp"
#include "../scope.hpp"

namespace lysithea_vm
{
    std::shared_ptr<const scope> standard_array_library::library_scope = create_scope();

    std::shared_ptr<scope> standard_array_library::create_scope()
    {
        auto result = std::make_shared<scope>();

        auto functions = std::make_shared<object_value>();
        functions->data["join"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            vm.push_stack(value(std::make_shared<array_value>(args.data, false)));
        });
        functions->data["length"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index<const array_value>(0);
            vm.push_stack(top->array_length());
        });
        functions->data["get"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index<const array_value>(0);
            auto index = args.get_int(1);
            vm.push_stack(get(top->data, index));
        });
        functions->data["set"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index<const array_value>(0);
            auto index = args.get_int(1);
            auto input = args.get_index(2);
            vm.push_stack(set(top->data, index, input));
        });
        functions->data["insert"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index<const array_value>(0);
            auto index = args.get_int(1);
            auto input = args.get_index(2);
            vm.push_stack(insert(top->data, index, input));
        });
        functions->data["insertFlatten"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index<const array_value>(0);
            auto index = args.get_int(1);
            auto input = args.get_index<const array_value>(2);
            vm.push_stack(insert_flatten(top->data, index, input->data));
        });
        functions->data["removeAt"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index<const array_value>(0);
            auto index = args.get_int(1);
            vm.push_stack(remove_at(top->data, index));
        });
        functions->data["remove"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index(0);
            auto input = args.get_index(1);
            vm.push_stack(remove(top, input));
        });
        functions->data["removeAll"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index(0);
            auto input = args.get_index(1);
            vm.push_stack(remove_all(top, input));
        });
        functions->data["contains"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index<const array_value>(0);
            auto input = args.get_index(1);
            vm.push_stack(contains(top->data, input));
        });
        functions->data["indexOf"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index<const array_value>(0);
            auto input = args.get_index(1);
            vm.push_stack(index_of(top->data, input));
        });
        functions->data["sublist"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index<const array_value>(0);
            auto index = args.get_int(1);
            auto length = args.get_int(2);
            vm.push_stack(sublist(top->data, index, length));
        });

        result->try_set_constant("array", value(functions));

        return result;
    }

    value standard_array_library::set(const array_vector &target, int index, const value &input)
    {
        array_vector arr(target);
        if (index < 0)
        {
            arr[arr.size() + index] = input;
        }
        else
        {
            arr[index] = input;
        }
        return array_value::make_value(arr);
    }
    value standard_array_library::get(const array_vector &target, int index)
    {
        return *get_iter(target, index);
    }
    value standard_array_library::insert(const array_vector &target, int index, const value &input)
    {
        array_vector arr(target);
        arr.insert(get_iter(arr, index), input);
        return array_value::make_value(arr);
    }
    value standard_array_library::insert_flatten(const array_vector &target, int index, const array_vector &input)
    {
        array_vector arr(target);
        arr.insert(get_iter(arr, index), input.cbegin(), input.cend());
        return array_value::make_value(arr);
    }
    value standard_array_library::remove_at(const array_vector &target, int index)
    {
        array_vector arr(target);
        arr.erase(get_iter(arr, index));
        return array_value::make_value(arr);
    }
    value standard_array_library::remove(const value &target, const value &input)
    {
        const auto &arr = target.get_complex<const array_value>()->data;
        for (auto i = 0; i < arr.size(); i++)
        {
            if (arr[i].compare_to(input) == 0)
            {
                array_vector result(arr);
                result.erase(result.begin() + i);
                return array_value::make_value(result);
            }
        }

        return target;
    }
    value standard_array_library::remove_all(const value &target, const value &input)
    {
        const auto &arr = target.get_complex<const array_value>()->data;
        auto i = 0;
        auto found_to_remove = false;

        for (; i < arr.size(); i++)
        {
            if (arr[i].compare_to(input) == 0)
            {
                found_to_remove = true;
                break;
            }
        }

        if (found_to_remove)
        {
            array_vector result(arr);
            result.erase(result.begin() + i);
            auto arr_offset = -1;
            i++;

            for (; i < arr.size(); i++)
            {
                if (arr[i].compare_to(input) == 0)
                {
                    result.erase(result.begin() + (i - arr_offset));
                    arr_offset--;
                }
            }

            return array_value::make_value(result);
        }

        return target;
    }
    value standard_array_library::contains(const array_vector &target, const value &input)
    {
        for (auto i = 0; i < target.size(); i++)
        {
            if (target[i].compare_to(input) == 0)
            {
                return lysithea_vm::value(true);
            }
        }

        return lysithea_vm::value(false);
    }

    value standard_array_library::index_of(const array_vector &target, const value &input)
    {
        for (auto i = 0; i < target.size(); i++)
        {
            if (target[i].compare_to(input) == 0)
            {
                return lysithea_vm::value(i);
            }
        }

        return lysithea_vm::value(-1);
    }
    value standard_array_library::sublist(const array_vector &target, int index, int length)
    {
        if (length == 0)
        {
            return array_value::empty;
        }

        auto offset = index;
        if (index < 0)
        {
            offset = target.size() + index;
        }

        if (length < 0 || offset + length > target.size())
        {
            array_vector result(target.cbegin() + offset, target.cend());
            return array_value::make_value(result);
        }
        else
        {
            array_vector result(target.cbegin() + offset, target.cbegin() + (offset + length));
            return array_value::make_value(result);
        }
    }
}