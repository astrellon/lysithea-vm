#pragma once

#include <stdexcept>
#include <string>
#include <vector>

#include "../code_location.hpp"

namespace lysithea_vm
{
    class parser_error : public std::runtime_error
    {
        public:
            // Fields
            code_location location;
            std::string token;
            std::string trace;
            std::string message;

            // Constructor
            parser_error(const code_location &location, const std::string &token, const std::string &trace, const std::string &message):
                location(location), token(token), trace(trace), message(message), std::runtime_error(message.c_str()) { }

            // Methods
    };
} // lysithea_vm