#include "standard_string_library.hpp"

#include <sstream>

#include "../virtual_machine.hpp"
#include "../values/object_value.hpp"
#include "../utils.hpp"
#include "../scope.hpp"

namespace lysithea_vm
{
    std::shared_ptr<const scope> standard_string_library::library_scope = create_scope();

    std::shared_ptr<scope> standard_string_library::create_scope()
    {
        auto result = std::make_shared<scope>();

        auto functions = std::make_shared<object_value>();
        functions->data["length"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index<string_value>(0);
            vm.push_stack(top->data.size());
        });
        functions->data["get"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index(0).to_string();
            auto index = args.get_int(1);
            vm.push_stack(get(top, index));
        });
        functions->data["set"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index(0).to_string();
            auto index = args.get_int(1);
            auto value = args.get_index(2).to_string();
            vm.push_stack(set(top, index, value));
        });
        functions->data["insert"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index(0).to_string();
            auto index = args.get_int(1);
            auto value = args.get_index(2).to_string();
            vm.push_stack(insert(top, index, value));
        });
        functions->data["substring"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index(0).to_string();
            auto index = args.get_int(1);
            auto length = args.get_int(2);
            vm.push_stack(substring(top, index, length));
        });
        functions->data["removeAt"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index(0).to_string();
            auto index = args.get_int(1);
            vm.push_stack(remove_at(top, index));
        });
        functions->data["removeAll"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index(0).to_string();
            auto values = args.get_index(1).to_string();
            vm.push_stack(remove_all(top, values));
        });
        functions->data["join"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto separator = args.get_index(0).to_string();
            vm.push_stack(join(separator, args.data.cbegin() + 1, args.data.cend()));
        });

        result->try_define("string", value(functions));

        return result;
    }

    value standard_string_library::get(const std::string &target, int index)
    {
        auto ch = target[get_index(target, index)];
        return value(std::make_shared<string_value>(std::string(ch, 1)));
    }
    value standard_string_library::set(const std::string &target, int index, const std::string &input)
    {
        std::stringstream ss;
        index = get_index(target, index);
        ss << target.substr(0, index) << input << target.substr(index + 1);
        return value(ss.str());
    }
    value standard_string_library::insert(const std::string &target, int index, const std::string &input)
    {
        std::string copy(target);
        return value(copy.insert(get_index(target, index), input));
    }
    value standard_string_library::substring(const std::string &target, int index, int length)
    {
        return target.substr(get_index(target, index), length);
    }
    value standard_string_library::remove_at(const std::string &target, int index)
    {
        std::string copy(target);
        copy.erase(get_index(target, index), 1);
        return copy;
    }
    value standard_string_library::remove_all(const std::string &target, const std::string &values)
    {
        std::string copy(target);
        auto i = copy.find(values);
        while (i != std::string::npos)
        {
            copy.erase(i, values.length());
            i = copy.find(values, i);
        }
        return copy;
    }

    value standard_string_library::join(const std::string &separator, const std::vector<value>::const_iterator begin, const std::vector<value>::const_iterator end)
    {
        auto first = true;
        std::stringstream ss;
        for (auto iter = begin; iter != end; ++iter)
        {
            if (!first)
            {
                ss << separator;
            }
            first = false;
            ss << iter->to_string();
        }
        return value(ss.str());
    }
} // lysithea_vm