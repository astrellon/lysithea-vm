#pragma once

#include <map>
#include <vector>
#include <string>

#include "code_line.hpp"

namespace stack_vm
{
    class scope
    {
        public:
            // Fields
            const std::string name;
            const std::vector<code_line> code;
            const std::map<std::string, int> labels;

            // Constructor
            scope(const std::string &name, const std::vector<code_line> &code) : name(name), code(code) { }
            scope(const std::string &name, const std::vector<code_line> &code, const std::map<std::string, int> &labels) : name(name), code(code), labels(labels) { }

            // Methods
    };
} // stack_vm