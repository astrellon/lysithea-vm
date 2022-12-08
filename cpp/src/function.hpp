#pragma once

#include <vector>
#include <string>
#include <unordered_map>

#include "./code_line.hpp"

namespace lysithea_vm
{
    class function
    {
        public:
            // Fields
            const std::string name;
            const std::vector<code_line> code;
            const std::vector<std::string> parameters;
            const std::unordered_map<std::string, int> labels;
            const bool has_name;

            // Constructor
            function(const std::vector<code_line> &code, const std::vector<std::string> &parameters, const std::string &name) :
                name(name.size() > 0 ? name : "anonymous"), code(code), parameters(parameters), has_name(name.size() > 0) { }

            function(const std::vector<code_line> &code, const std::vector<std::string> &parameters, const std::unordered_map<std::string, int> &labels, const std::string &name) :
                name(name.size() > 0 ? name : "anonymous"), code(code), parameters(parameters), labels(labels), has_name(name.size() > 0) { }

            // Methods
    };
} // lysithea_vm