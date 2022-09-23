#include "standard_string_library.hpp"

#include <sstream>

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
                vm.push_stack(append(left, right.to_string()));
                break;
            }
            case hash("prepend"):
            {
                const auto &right = vm.pop_stack();
                const auto &left = vm.pop_stack();
                vm.push_stack(prepend(left, right.to_string()));
                break;
            }
            case hash("length"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(static_cast<double>(top.get_string()->size()));
                break;
            }
            case hash("get"):
            {
                const auto &index = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(get(top, index.get_int()));
                break;
            }
            case hash("set"):
            {
                const auto &value = vm.pop_stack();
                const auto &index = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(set(top, index.get_int(), value.to_string()));
                break;
            }
            case hash("insert"):
            {
                const auto &value = vm.pop_stack();
                const auto &index = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(insert(top, index.get_int(), value.to_string()));
                break;
            }
            case hash("substring"):
            {
                const auto &length = vm.pop_stack();
                const auto &index = vm.pop_stack();
                const auto &top = vm.pop_stack();
                vm.push_stack(substring(top, index.get_int(), length.get_int()));
                break;
            }
        }
    }

    value standard_string_library::append(const value &target, const std::string &input)
    {
        return target.to_string() + input;
    }
    value standard_string_library::prepend(const value &target, const std::string &input)
    {
        return input + target.to_string();
    }
    value standard_string_library::get(const value &target, int index)
    {
        auto ch = target.get_string()->at(index);
        return std::make_shared<std::string>(ch, 1);
    }
    value standard_string_library::set(const value &target, int index, const std::string &input)
    {
        std::stringstream ss;
        const auto str = target.get_string();
        ss << str->substr(0, index) << input << str->substr(index + 1);
        return ss.str();
    }
    value standard_string_library::insert(const value &target, int index, const std::string &input)
    {
        return target.to_string().insert(index, input);
    }
    value standard_string_library::substring(const value &target, int index, int length)
    {
        if (length < 0)
        {
            return target.get_string()->substr(index);
        }
        return target.get_string()->substr(index, length);
    }
} // stack_vm