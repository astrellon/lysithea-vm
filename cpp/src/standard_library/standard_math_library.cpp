#include "standard_math_library.hpp"

#include <math.h>

#include "../virtual_machine.hpp"
#include "../values/object_value.hpp"
#include "../utils.hpp"
#include "../scope.hpp"

#define M_DEG_TO_RAD M_PI / 180.0

namespace lysithea_vm
{
    std::shared_ptr<const scope> standard_math_library::library_scope = create_scope();

    std::shared_ptr<scope> standard_math_library::create_scope()
    {
        auto result = std::make_shared<scope>();

        auto functions = std::make_shared<object_value>();

        functions->data["E"] = value(M_E);
        functions->data["PI"] = value(M_PI);
        functions->data["DegToRad"] = value(M_DEG_TO_RAD);

        functions->data["sin"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &top = args.get_number(0);
            vm.push_stack(sin(top));
        });
        functions->data["cos"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &top = args.get_number(0);
            vm.push_stack(cos(top));
        });
        functions->data["tan"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &top = args.get_number(0);
            vm.push_stack(tan(top));
        });

        functions->data["pow"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &x = args.get_number(0);
            const auto &y = args.get_number(1);
            vm.push_stack(pow(x, y));
        });
        functions->data["exp"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &x = args.get_number(0);
            vm.push_stack(exp(x));
        });
        functions->data["floor"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &x = args.get_number(0);
            vm.push_stack(floor(x));
        });
        functions->data["ceil"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &x = args.get_number(0);
            vm.push_stack(ceil(x));
        });
        functions->data["round"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &x = args.get_number(0);
            vm.push_stack(round(x));
        });
        functions->data["isNaN"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &x = args.get_number(0);
            vm.push_stack(std::isnan(x));
        });
        functions->data["isFinite"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &x = args.get_number(0);
            vm.push_stack(std::isfinite(x));
        });
        functions->data["parse"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &top = args.get_index(0);
            if (top.is_number())
            {
                vm.push_stack(top);
                return;
            }

            vm.push_stack(std::stod(top.to_string()));
        });

        functions->data["log"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &x = args.get_number(0);
            vm.push_stack(log(x));
        });
        functions->data["log2"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &x = args.get_number(0);
            vm.push_stack(log2(x));
        });
        functions->data["log10"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &x = args.get_number(0);
            vm.push_stack(log10(x));
        });
        functions->data["abs"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            const auto &x = args.get_number(0);
            vm.push_stack(abs(x));
        });

        functions->data["max"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto max = args.get_index(0);
            for (auto iter = args.data.cbegin() + 1; iter != args.data.cend(); ++iter)
            {
                if (iter->compare_to(max) > 0)
                {
                    max = *iter;
                }
            }

            vm.push_stack(max);
        });

        functions->data["min"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto min = args.get_index(0);
            for (auto iter = args.data.cbegin() + 1; iter != args.data.cend(); ++iter)
            {
                if (iter->compare_to(min) < 0)
                {
                    min = *iter;
                }
            }

            vm.push_stack(min);
        });

        functions->data["sum"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
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
        });

        return result;
    }
} // lysithea_vm