#include "standard_assert_library.hpp"

#include <sstream>
#include <iostream>

#include "../virtual_machine.hpp"
#include "../values/object_value.hpp"
#include "../utils.hpp"
#include "../scope.hpp"

namespace lysithea_vm
{
    std::shared_ptr<const scope> standard_assert_library::library_scope = create_scope();

    std::shared_ptr<scope> standard_assert_library::create_scope()
    {
        auto result = std::make_shared<scope>();

        auto functions = std::make_shared<object_value>();

        functions->data["true"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index(0);
            if (!top.is_true())
            {
                vm.running = false;
                std::cout << "Assert expected true\n";
            }
        });

        functions->data["false"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.get_index(0);
            if (!top.is_false())
            {
                vm.running = false;
                std::cout << "Assert expected false\n";
            }
        });

        functions->data["equals"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto expected = args.get_index(0);
            auto actual = args.get_index(1);
            if (expected.compare_to(actual) != 0)
            {
                vm.running = false;
                std::cout << "Assert expected equals:"
                    << "\nExpected: " << expected.to_string()
                    << "\nActual: " << actual.to_string()
                    << "\n";
            }
        });

        functions->data["notEquals"] = value::make_builtin([](virtual_machine &vm, const array_value &args) -> void
        {
            auto expected = args.get_index(0);
            auto actual = args.get_index(1);
            if (expected.compare_to(actual) == 0)
            {
                vm.running = false;
                std::cout << "Assert expected not equals:"
                    << "\nExpected: " << expected.to_string()
                    << "\nActual: " << actual.to_string()
                    << "\n";
            }
        });

        result->try_set_constant("assert", value(functions));

        return result;
    }
}