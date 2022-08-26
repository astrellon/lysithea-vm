#pragma once

#include <map>
#include <vector>
#include <string>

#include "code_line.hpp"

namespace stack_vm
{
    class Scope
    {
        public:
            // Fields
            const std::string name;
            const std::vector<CodeLine> code;
            const std::map<std::string, int> labels;

            // Constructor
            Scope(const std::string &name, const std::vector<CodeLine> &code) : name(name), code(code) { }
            Scope(const std::string &name, const std::vector<CodeLine> &code, const std::map<std::string, int> &labels) : name(name), code(code), labels(labels) { }

            // Methods
    };
} // stack_vm