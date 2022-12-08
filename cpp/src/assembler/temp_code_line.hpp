#pragma once

#include <string>

#include "../operator.hpp"
#include "../values/value.hpp"

namespace lysithea_vm
{
    struct temp_code_line
    {
        // Fields
        vm_operator op;
        std::string jump_label;
        value argument;

        // Constructor
        temp_code_line(const std::string &jump_label) : op(vm_operator::unknown), jump_label(jump_label), argument() { }
        temp_code_line(vm_operator op, std::shared_ptr<complex_value> arg) : op(op), argument(arg) { }
        temp_code_line(vm_operator op, value arg) : op(op), argument(arg) { }
        temp_code_line(vm_operator op) : op(op), argument() { }

        // Methods
        bool is_label() const { return jump_label.size() > 0; }
        std::string to_string() const;
    };
} // lysithea_vm