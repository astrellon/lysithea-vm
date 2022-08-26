#pragma once

#include <string>
#include <optional>
#include <vector>
#include <memory>

#include "operator.hpp"
#include "value.hpp"
#include "json.hpp"
#include "scope.hpp"

using nlohmann::json;

namespace stack_vm
{
    class assembler
    {
        public:
            // Methods
            static std::vector<std::shared_ptr<scope>> parse_scopes(const json &j);
            static std::shared_ptr<scope> parse_scope(const json &j);

        private:
            // Classes
            class temp_code_line
            {
                public:
                    // Fields
                    const vm_operator op;
                    const std::string jump_label;
                    const std::optional<value> argument;

                    // Constructor
                    temp_code_line(const std::string &jump_label) : op(vm_operator::unknown), jump_label(jump_label) { }
                    temp_code_line(vm_operator op, value arg) : op(op), argument(arg) { }
                    temp_code_line(vm_operator op) : op(op) { }

                    // Methods
                    bool is_label() const { return jump_label.size() > 0; }
            };

            static std::vector<temp_code_line> parse_code_line(const json &j);
            static std::optional<value> parse_json_value(const json &j);
    };
} // stack_vm