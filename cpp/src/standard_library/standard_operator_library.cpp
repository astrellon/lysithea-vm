#include "standard_operator_library.hpp"

#include "../virtual_machine.hpp"
#include "../scope.hpp"
#include "../utils.hpp"
#include "./standard_string_library.hpp"

namespace lysithea_vm
{
    std::shared_ptr<const scope> standard_operator_library::library_scope = create_scope();

    std::shared_ptr<scope> standard_operator_library::create_scope()
    {
        auto result = std::make_shared<scope>();

        result->define(">", [](virtual_machine &vm, const array_value &args) -> void
        {
            vm.push_stack(greater(args.get_index(0), args.get_index(1)));
        });
        result->define(">=", [](virtual_machine &vm, const array_value &args) -> void
        {
            vm.push_stack(greater_equals(args.get_index(0), args.get_index(1)));
        });
        result->define("==", [](virtual_machine &vm, const array_value &args) -> void
        {
            vm.push_stack(equals(args.get_index(0), args.get_index(1)));
        });
        result->define("!=", [](virtual_machine &vm, const array_value &args) -> void
        {
            vm.push_stack(not_equals(args.get_index(0), args.get_index(1)));
        });
        result->define("<", [](virtual_machine &vm, const array_value &args) -> void
        {
            vm.push_stack(less(args.get_index(0), args.get_index(1)));
        });
        result->define("<=", [](virtual_machine &vm, const array_value &args) -> void
        {
            vm.push_stack(less_equals(args.get_index(0), args.get_index(1)));
        });
        result->define("!", [](virtual_machine &vm, const array_value &args) -> void
        {
            vm.push_stack(not_bool(args.get_index(0)));
        });

        result->define("+", [](virtual_machine &vm, const array_value &args) -> void
        {
            if (args.data.size() == 0)
            {
                throw std::runtime_error("Addition operator expects at least 1 input");
            }

            if (args.data[0].is_number())
            {
                auto total = 0.0;
                for (const auto &iter : args.data)
                {
                    if (!iter.is_number())
                    {
                        throw std::runtime_error("Invalid addition operator usage, must be all numbers");
                    }
                    total += iter.get_number();
                }
                vm.push_stack(total);
            }
            else
            {
                vm.push_stack(standard_string_library::join("", args.data.cbegin(), args.data.cend()));
                return;
            }
        });

        result->define("-", [](virtual_machine &vm, const array_value &args) -> void
        {
            if (args.data.size() == 0)
            {
                throw std::runtime_error("Subtract operator expects at least 1 input");
            }

            auto total = args.get_index(0).get_number();
            if (args.data.size() == 1)
            {
                vm.push_stack(-total);
                return;
            }
            for (auto iter = args.data.cbegin() + 1; iter != args.data.cend(); ++iter)
            {
                if (!iter->is_number())
                {
                    throw std::runtime_error("Invalid + operator usage, must be all numbers");
                }
                total -= iter->get_number();
            }
            vm.push_stack(total);
        });

        result->define("*", [](virtual_machine &vm, const array_value &args) -> void
        {
            if (args.data.size() == 0)
            {
                throw std::runtime_error("Multiply operator expects more than 1 input");
            }

            auto total = 0.0;
            for (auto iter : args.data)
            {
                if (!iter.is_number())
                {
                    throw std::runtime_error("Multiple operator needs all numbers");
                }
                total += iter.get_number();
            }
            vm.push_stack(total);
        });

        result->define("%", [](virtual_machine &vm, const array_value &args) -> void
        {
            if (args.data.size() != 2)
            {
                throw std::runtime_error("Modulo operator expects 2 numbers");
            }

            auto result = fmod(args.get_number(0), args.get_number(1));
            vm.push_stack(result);
        });

        return result;
    }

} // lysithea_vm