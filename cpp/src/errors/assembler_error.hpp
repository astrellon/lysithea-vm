#pragma once

#include <stdexcept>
#include <string>
#include <vector>

#include "../assembler/token.hpp"

namespace lysithea_vm
{
    class assembler_error : public std::runtime_error
    {
        public:
            // Fields
            token at_token;
            std::string message;

            // Constructor
            assembler_error(const token &at_token, std::string message):
                at_token(at_token), message(message), std::runtime_error(message.c_str()) { }

            // Methods
            static assembler_error create(const token &at_token, const std::string &message)
            {
                return assembler_error(at_token, at_token.location.to_string() + ": " + message);
            }
    };
} // lysithea_vm