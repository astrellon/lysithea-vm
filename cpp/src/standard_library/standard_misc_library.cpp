#include "standard_misc_library.hpp"

#include <iostream>

#include "../values/complex_value.hpp"
#include "../values/array_value.hpp"
#include "../values/builtin_function_value.hpp"
#include "../virtual_machine.hpp"
#include "../scope.hpp"
#include "../utils.hpp"

namespace lysithea_vm
{
    std::shared_ptr<const scope> standard_misc_library::library_scope = create_scope();

    std::shared_ptr<scope> standard_misc_library::create_scope()
    {
        auto result = std::make_shared<scope>();

        result->define("typeof", [](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.data[0];
            vm.push_stack(top.type_name());
        });

        result->define("isDefined", [](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.data[0].to_string();
            value temp;
            auto is_defined = vm.current_scope->try_get_key(top, temp);
            vm.push_stack(is_defined);
        });

        result->define("toString", [](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.data[0];
            vm.push_stack(top.to_string());
        });

        result->define("compareTo", [](virtual_machine &vm, const array_value &args) -> void
        {
            auto left = args.data[0];
            auto right = args.data[1];
            vm.push_stack(left.compare_to(right));
        });

        result->define("print", [](virtual_machine &vm, const array_value &args) -> void
        {
            for (auto iter : args.data)
            {
                std::cout << iter.to_string();
            }
            std::cout << "\n";
        });

        return result;
    }
} // lysithea_vm