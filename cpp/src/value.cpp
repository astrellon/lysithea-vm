#include "value.hpp"

namespace stack_vm
{
    const value value::empty_array(std::make_shared<array_value>());
    const value value::empty_string(std::make_shared<std::string>());
    const value value::empty_object(std::make_shared<object_value>());
    const value value::null;
} // stack_vm