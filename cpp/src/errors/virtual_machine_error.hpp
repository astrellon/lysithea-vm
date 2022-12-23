#pragma once

#include <stdexcept>
#include <string>
#include <vector>

namespace lysithea_vm
{
    class virtual_machine_error : public std::runtime_error
    {
        public:
            // Fields
            std::vector<std::string> stack_trace;
            std::string message;

            // Constructor
            virtual_machine_error(std::vector<std::string> stack_trace, std::string message): stack_trace(stack_trace), message(message), std::runtime_error(message.c_str()) { }
            virtual_machine_error(std::vector<std::string> stack_trace, const char *message): stack_trace(stack_trace), message(message), std::runtime_error(message) { }

            // Methods
    };
} // lysithea_vm