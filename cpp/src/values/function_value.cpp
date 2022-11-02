#include "function_value.hpp"

#include "../virtual_machine.hpp"
#include "./array_value.hpp"

namespace stack_vm
{
    void function_value::invoke(virtual_machine &vm, std::shared_ptr<array_value> args, bool push_to_stack_trace) const
    {
        vm.execute_function(value, args, push_to_stack_trace);
    }
} // stack_vm