#pragma once

#include <unordered_map>
#include <vector>
#include <string>

#include "code_line.hpp"

namespace stack_vm
{
    class function
    {
        public:
            // Fields
            std::string name;
            const std::vector<code_line> code;
            const std::vector<std::string> parameters;
            const std::unordered_map<std::string, int> labels;

            // Constructor
            function(const std::vector<code_line> &code, const std::vector<std::string> &parameters) :
                name("anonymous"), code(code), parameters(parameters) { }

            function(const std::vector<code_line> &code, const std::vector<std::string> &parameters, const std::unordered_map<std::string, int> &labels) :
                name("anonymous"), code(code), parameters(parameters), labels(labels) { }

            // Methods
    };
} // stack_vm