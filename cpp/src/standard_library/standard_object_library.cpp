#include "standard_object_library.hpp"

#include "../values/array_value.hpp"
#include "../virtual_machine.hpp"

namespace lysithea_vm
{
    std::shared_ptr<const scope> standard_object_library::library_scope = create_scope();

    std::shared_ptr<scope> standard_object_library::create_scope()
    {
        auto result = std::make_shared<scope>();

        auto functions = std::make_shared<object_value>();
        functions->data["set"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto obj = args.get_index<const object_value>(0);
            auto key = args.get_index<const string_value>(1);
            auto value = args.get_index(2);
            vm.push_stack(set(obj->data, key->data, value));
        });
        functions->data["get"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto obj = args.get_index<const object_value>(0);
            auto key = args.get_index<const string_value>(1);
            vm.push_stack(get(obj->data, key->data));
        });
        functions->data["keys"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto obj = args.get_index<const object_value>(0);
            vm.push_stack(keys(obj->data));
        });
        functions->data["values"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto obj = args.get_index<const object_value>(0);
            vm.push_stack(values(obj->data));
        });
        functions->data["length"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto obj = args.get_index<const object_value>(0);
            vm.push_stack(obj->data.size());
        });
        functions->data["removeKey"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto obj = args.get_index(0);
            auto key = args.get_index<const string_value>(1);
            vm.push_stack(removeKey(obj, key->data));
        });
        functions->data["removeValues"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto obj = args.get_index(0);
            auto values = args.get_index(1);
            vm.push_stack(removeValues(obj, values));
        });

        result->define("object", value(functions));

        return result;
    }

    value standard_object_library::set(const object_map &target, const std::string &key, const value &input)
    {
        object_map obj(target);
        obj[key] = input;
        return object_value::make_value(obj);
    }
    value standard_object_library::get(const object_map &target, const std::string &key)
    {
        auto find = target.find(key);
        if (find != target.end())
        {
            return find->second;
        }
        else
        {
            return value::make_null();
        }
    }

    value standard_object_library::keys(const object_map &target)
    {
        array_vector arr;
        for (auto iter : target)
        {
            arr.push_back(iter.first);
        }
        return array_value::make_value(arr);
    }
    value standard_object_library::values(const object_map &target)
    {
        array_vector arr;
        for (auto iter : target)
        {
            arr.push_back(iter.second);
        }
        return array_value::make_value(arr);
    }

    value standard_object_library::removeKey(const value &target, const std::string &key)
    {
        auto obj_target = target.get_complex<const object_value>();
        auto find = obj_target->data.find(key);
        if (find == obj_target->data.cend())
        {
            return target;
        }

        object_map obj(obj_target->data);
        obj.erase(obj.find(key));
        return object_value::make_value(obj);
    }

    value standard_object_library::removeValues(const value &target, const value &input)
    {
        auto obj_target = target.get_complex<const object_value>();
        object_map obj(obj_target->data);
        for (auto iter = obj.begin(); iter != obj.end();)
        {
            if (iter->second.compare_to(input) == 0)
            {
                obj.erase(iter++);
            }
            else
            {
                ++iter;
            }
        }
        return object_value::make_value(obj);
    }
} // lysithea_vm