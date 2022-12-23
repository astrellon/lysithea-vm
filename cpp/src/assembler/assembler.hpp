#pragma once

#include <string>
#include <istream>
#include <memory>
#include <vector>

#include "./temp_code_line.hpp"
#include "./token.hpp"

#include "../values/value.hpp"
#include "../values/complex_value.hpp"
#include "../values/string_value.hpp"
#include "../values/builtin_function_value.hpp"
#include "../script.hpp"
#include "../scope.hpp"
#include "../operator.hpp"
#include "../function.hpp"

namespace lysithea_vm
{
    class assembler
    {
        public:

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
            static const std::string keyword_const;
            static const std::string keyword_jump;
            static const std::string keyword_return;

            scope builtin_scope;

            // Constructor
            assembler();

            // Methods
            std::shared_ptr<script> parse_from_text(const std::string &source_name, const std::string &input);
            std::shared_ptr<script> parse_from_stream(const std::string &source_name, std::istream &input);
            code_line_list parse(const token &input);

            code_line_list parse_function_keyword(const token &input);
            code_line_list parse_define_set(const token &input, bool is_define);
            code_line_list parse_const(const token &input);
            code_line_list parse_loop(const token &input);
            code_line_list parse_cond(const token &input, bool is_if_statement);
            code_line_list parse_flatten(const token &input);
            code_line_list parse_loop_jump(const token &token, const std::string &keyword, bool jump_to_start);
            std::shared_ptr<function> parse_function(const token &input);
            code_line_list parse_jump(const token &input);
            code_line_list parse_return(const token &input);
            code_line_list parse_negative(const token &input);
            code_line_list parse_one_push_input(vm_operator op_code, const token &input);
            code_line_list parse_operator(vm_operator op_code, const token &input);
            code_line_list parse_one_variable_update(vm_operator op_code, const token &input);
            code_line_list parse_string_concat(const token &input);
            code_line_list transform_assignment_operator(const token &input);
            code_line_list parse_keyword(const std::string &keyword, const token &input);

            std::shared_ptr<function> parse_global_function(const token &input);

            code_line_list optimise_call_symbol_value(const token &input, const std::string &variable, int num_args);
            code_line_list optimise_get_symbol_value(const token &input, const std::string &variable);
            code_line_list optimise_get(const token &input, const std::string &variable);

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
            std::vector<loop_labels> loop_stack;
            std::vector<std::string> keyword_parsing_stack;
            std::shared_ptr<scope> const_scope;

            std::string source_name;
            std::shared_ptr<std::vector<std::string>> source_text;

            // Methods
            std::shared_ptr<script> parse_from_value(const token &input);

            std::shared_ptr<function> process_temp_function(const std::vector<std::string> &parameters, const code_line_list &temp_code_lines, const std::string &name);

    };
} // lysithea_vm