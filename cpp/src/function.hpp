#pragma once

#include <memory>
#include <vector>
#include <string>
#include <unordered_map>

#include "./code_line.hpp"
#include "./debug_symbols.hpp"

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
            std::shared_ptr<debug_symbols> symbols;
            const bool has_name;

            // Constructor
            function(const std::vector<code_line> &code, const std::vector<std::string> &parameters, const std::unordered_map<std::string, int> &labels, const std::string &name, std::shared_ptr<debug_symbols> debug_symbols) :
                name(name.size() > 0 ? name : "anonymous"), code(code), parameters(parameters), labels(labels), has_name(name.size() > 0), symbols(debug_symbols) { }

            // Methods
    };
} // lysithea_vm