#pragma once

#include <string>
#include <istream>
#include <memory>
#include <stack>

#include "./values/ivalue.hpp"
#include "./values/string_value.hpp"
#include "./values/builtin_function_value.hpp"
#include "./script.hpp"
#include "./scope.hpp"
#include "./operator.hpp"
#include "./function.hpp"

namespace stack_vm
{
    class assembler
    {
        public:
            struct temp_code_line
            {
                // Fields
                vm_operator op;
                std::string jump_label;
                std::shared_ptr<ivalue> argument;

                // Constructor
                temp_code_line(const std::string &jump_label) : op(vm_operator::unknown), jump_label(jump_label), argument(nullptr) { }
                temp_code_line(vm_operator op, std::shared_ptr<ivalue> arg) : op(op), argument(arg) { }
                temp_code_line(vm_operator op) : op(op), argument(nullptr) { }

                // Methods
                bool is_label() const { return jump_label.size() > 0; }
                std::string to_string() const;
            };

            // Fields
            static const std::string keyword_function;
            static const std::string keyword_loop;
            static const std::string keyword_continue;
            static const std::string keyword_break;
            static const std::string keyword_if;
            static const std::string keyword_unless;
            static const std::string keyword_set;
            static const std::string keyword_define;
            static const std::string keyword_inc;
            static const std::string keyword_dec;

            scope builtin_scope;

            // Constructor
            assembler();

            // Methods
            std::shared_ptr<script> parse_from_text(const std::string &input);
            std::shared_ptr<script> parse_from_stream(std::istream &input);
            std::shared_ptr<script> parse_from_value(const array_value &input);
            std::vector<temp_code_line> parse(std::shared_ptr<ivalue> input);

            std::vector<temp_code_line> parse_set(const array_value &input);
            std::vector<temp_code_line> parse_define(const array_value &input);
            std::vector<temp_code_line> parse_loop(const array_value &input);
            std::vector<temp_code_line> parse_cond(const array_value &input, bool is_if_statement);
            std::vector<temp_code_line> parse_flatten(std::shared_ptr<ivalue> input);
            std::vector<temp_code_line> parse_loop_jump(const std::string &keyword, bool jump_to_start);
            std::shared_ptr<function> parse_function(const array_value &input);
            std::vector<temp_code_line> parse_change_variable(std::shared_ptr<ivalue> input, builtin_function_value change_func);
            std::vector<temp_code_line> parse_keyword(const std::string &keyword, const array_value &input);

            std::shared_ptr<function> parse_global_function(const array_value &input);

            std::vector<temp_code_line> optimise_call_symbol_value(const std::string &variable, int num_args);
            std::vector<temp_code_line> optimise_get_symbol_value(const std::string &variable);

            static bool is_get_property_request(const std::string &variable, std::shared_ptr<string_value> &parent_key, std::shared_ptr<array_value> &property);

        private:
            struct loop_labels
            {
                // Fields
                std::shared_ptr<string_value> start;
                std::shared_ptr<string_value> end;

                // Constructor
                loop_labels(std::shared_ptr<string_value> start, std::shared_ptr<string_value> end): start(start), end(end) { }
            };

            // Fields
            int label_count;
            std::stack<loop_labels> loop_stack;

            // Methods
            static std::shared_ptr<function> process_temp_function(const std::vector<std::string> &parameters, const std::vector<temp_code_line> &temp_code_lines);

    };
} // stack_vm