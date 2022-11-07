#pragma once

#include <string>
#include <istream>
#include <memory>
#include <stack>

#include "./values/value.hpp"
#include "./values/complex_value.hpp"
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
                value argument;

                // Constructor
                temp_code_line(const std::string &jump_label) : op(vm_operator::unknown), jump_label(jump_label), argument() { }
                temp_code_line(vm_operator op, std::shared_ptr<complex_value> arg) : op(op), argument(arg) { }
                temp_code_line(vm_operator op, value arg) : op(op), argument(arg) { }
                temp_code_line(vm_operator op) : op(op), argument() { }

                // Methods
                bool is_label() const { return jump_label.size() > 0; }
                std::string to_string() const;
            };

            using code_line_list = std::vector<temp_code_line>;

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
            static const std::string keyword_jump;
            static const std::string keyword_return;
            static const builtin_function_value inc_number;
            static const builtin_function_value dec_number;

            scope builtin_scope;

            // Constructor
            assembler();

            // Methods
            std::shared_ptr<script> parse_from_text(const std::string &input);
            std::shared_ptr<script> parse_from_stream(std::istream &input);
            std::shared_ptr<script> parse_from_value(const array_value &input);
            code_line_list parse(value input);

            code_line_list parse_set(const array_value &input);
            code_line_list parse_define(const array_value &input);
            code_line_list parse_loop(const array_value &input);
            code_line_list parse_cond(const array_value &input, bool is_if_statement);
            code_line_list parse_flatten(value input);
            code_line_list parse_loop_jump(const std::string &keyword, bool jump_to_start);
            std::shared_ptr<function> parse_function(const array_value &input);
            code_line_list parse_change_variable(value input, builtin_function_value change_func);
            code_line_list parse_jump(const array_value &input);
            code_line_list parse_return(const array_value &input);
            code_line_list parse_keyword(const std::string &keyword, const array_value &input);

            std::shared_ptr<function> parse_global_function(const array_value &input);

            code_line_list optimise_call_symbol_value(const std::string &variable, int num_args);
            code_line_list optimise_get_symbol_value(const std::string &variable);

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
            static std::shared_ptr<function> process_temp_function(const std::vector<std::string> &parameters, const code_line_list &temp_code_lines);

    };
} // stack_vm