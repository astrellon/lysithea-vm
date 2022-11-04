#include "standard_misc_library.hpp"

#include <sstream>

#include "../values/complex_value.hpp"
#include "../values/array_value.hpp"
#include "../values/builtin_function_value.hpp"
#include "../virtual_machine.hpp"
#include "../scope.hpp"
#include "../utils.hpp"

namespace stack_vm
{
    std::shared_ptr<const scope> standard_misc_library::library_scope = create_scope();

    std::shared_ptr<scope> standard_misc_library::create_scope()
    {
        auto result = std::make_shared<stack_vm::scope>();

        result->define("typeof", [](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.value->at(0);
            vm.push_stack(top->type_name());
        });

        result->define("toString", [](virtual_machine &vm, const array_value &args) -> void
        {
            auto top = args.value->at(0);
            vm.push_stack(top->to_string());
        });

        result->define("compareTo", [](virtual_machine &vm, const array_value &args) -> void
        {
            auto left = args.value->at(0);
            auto right = args.value->at(1);
            vm.push_stack(left->compare_to(right.get()));
        });

        result->define("print", [](virtual_machine &vm, const array_value &args) -> void
        {
            std::stringstream ss;
            for (auto iter : *args.value)
            {
                ss << iter->to_string();
            }
            vm.push_stack(ss.str());
        });

        return result;
    }
} // stack_vm