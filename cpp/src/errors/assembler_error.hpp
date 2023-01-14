#pragma once

#include <stdexcept>
#include <string>
#include <vector>

#include "../assembler/token.hpp"
#include "./error_common.hpp"

namespace lysithea_vm
{
    class assembler_error : public std::runtime_error
    {
        public:
            // Fields
            token at_token;
            std::string trace;
            std::string message;

            // Constructor
            assembler_error(const token &at_token, const std::string &trace, const std::string &message):
                at_token(at_token), trace(trace), message(message), std::runtime_error(message.c_str()) { }
    };
} // lysithea_vm