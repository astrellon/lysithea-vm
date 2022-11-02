#include "standard_value_library.hpp"

#include "../values/ivalue.hpp"
#include "../values/array_value.hpp"
#include "../values/builtin_function_value.hpp"
#include "../virtual_machine.hpp"
#include "../scope.hpp"
#include "../utils.hpp"

namespace stack_vm
{
    std::shared_ptr<const scope> standard_value_library::scope = create_scope();

    std::shared_ptr<const scope> standard_value_library::create_scope()
    {
        auto result = std::make_shared<stack_vm::scope>();

        result->define("typeof", [](virtual_machine &vm, const array_value &args)
        {
            const auto &top = vm.pop_stack();
            vm.push_stack(top.type());
        });

            case hash("toString"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(top.to_string());
                break;
            }
            case hash("compareTo"):
            {
                const auto &right = vm.pop_stack();
                const auto &left = vm.pop_stack();
                vm.push_stack(left.compare(right));
                break;
            }
        }

        return result;
    }
} // stack_vm