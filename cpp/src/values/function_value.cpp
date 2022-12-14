#include "function_value.hpp"

#include "../virtual_machine.hpp"
#include "./array_value.hpp"

namespace lysithea_vm
{
    void function_value::invoke(virtual_machine &vm, std::shared_ptr<const array_value> args, bool push_to_stack_trace) const
    {
        vm.execute_function(data, args, push_to_stack_trace);
    }
} // lysithea_vm