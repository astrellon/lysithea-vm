#include "lisp_assembler.hpp"

#include <sstream>

#include "./parser.hpp"
#include "./utils.hpp"
#include "./values/function_value.hpp"

namespace lysithea_vm
{
    const std::string lisp_assembler::keyword_function("function");
    const std::string lisp_assembler::keyword_loop("loop");
    const std::string lisp_assembler::keyword_continue("continue");
    const std::string lisp_assembler::keyword_break("break");
    const std::string lisp_assembler::keyword_if("if");
    const std::string lisp_assembler::keyword_unless("unless");
    const std::string lisp_assembler::keyword_set("set");
    const std::string lisp_assembler::keyword_define("define");
    const std::string lisp_assembler::keyword_inc("inc");
    const std::string lisp_assembler::keyword_dec("dec");

    lisp_assembler::lisp_assembler() : label_count(0)
    {

    }

    std::shared_ptr<script> lisp_assembler::parse_from_text(const std::string &input)
    {
        std::stringstream stream(input);
        return parse_from_stream(stream);
    }

    std::shared_ptr<script> lisp_assembler::parse_from_stream(std::istream &input)
    {
        auto parsed = parser::read_from_stream(input);
        auto code = parse_global_function(parsed);

        auto script_scope = std::make_shared<scope>();
        script_scope->combine_scope(builtin_scope);

        return std::make_shared<script>(script_scope, code);
    }

    std::shared_ptr<function> lisp_assembler::parse_global_function(const array_value &input)
    {
        std::vector<temp_code_line> temp_code_lines;
        for (const auto &iter : *input.value)
        {
            auto lines = parse(iter);
            push_range(temp_code_lines, lines);
        }

        std::vector<std::string> empty_parameters;
        auto code = process_temp_function(empty_parameters, temp_code_lines);
        code->name = "global";

        return code;
    }

    std::vector<lisp_assembler::temp_code_line> lisp_assembler::parse(std::shared_ptr<ivalue> input)
    {
        std::vector<temp_code_line> result;
        return result;
    }

    std::vector<lisp_assembler::temp_code_line> lisp_assembler::parse_set(const array_value &input)
    {
        auto result = parse(input.value->at(2));
        result.emplace_back(vm_operator::set, input.value->at(1));
        return result;
    }

    std::vector<lisp_assembler::temp_code_line> lisp_assembler::parse_define(const array_value &input)
    {
        auto result = parse(input.value->at(2));
        result.emplace_back(vm_operator::set, input.value->at(1));

        auto is_function = dynamic_cast<function *>(result[0].argument.get());
        if (is_function)
        {
            is_function->name = input.value->at(1)->to_string();
        }
        return result;
    }

    std::vector<lisp_assembler::temp_code_line> lisp_assembler::parse_loop(const array_value &input)
    {
        if (input.value->size() < 3)
        {
            throw std::runtime_error("Loop input has too few inputs");
        }

        auto loop_label_num = label_count++;
        std::stringstream ss_label_start(":LoopStart");
        ss_label_start << loop_label_num;
        auto label_start = std::make_shared<string_value>(ss_label_start.str());

        std::stringstream ss_label_end(":LoopEnd");
        ss_label_end << loop_label_num;
        auto label_end = std::make_shared<string_value>(ss_label_end.str());

        loop_stack.emplace(label_start, label_end);

        std::vector<temp_code_line> result;
        result.emplace_back(ss_label_start.str());

        auto comparison_value = input.value->at(1);
        auto comparison_call = dynamic_cast<const array_value *>(comparison_value.get());
        if (!comparison_call)
        {
            throw std::runtime_error("Loop comparison input needs to be an array");
        }

        push_range(result, parse(comparison_value));
        result.emplace_back(vm_operator::jump_false, label_end);

        for (auto i = 2; i < input.value->size(); i++)
        {
            push_range(result, parse(input.value->at(i)));
        }

        result.emplace_back(vm_operator::jump, label_start);
        result.emplace_back(ss_label_end.str());

        loop_stack.pop();
        return result;
    }

    std::vector<lisp_assembler::temp_code_line> lisp_assembler::parse_cond(const array_value &input, bool is_if_statement)
    {
        if (input.value->size() < 3)
        {
            throw std::runtime_error("Condition input has too few inputs");
        }
        if (input.value->size() < 3)
        {
            throw std::runtime_error("Condition input has too many inputs!");
        }

        auto if_label_num = label_count++;

        std::stringstream ss_label_else(":CondElse");
        ss_label_else << if_label_num;
        auto label_else = std::make_shared<string_value>(ss_label_else.str());

        std::stringstream ss_label_end(":CondEnd");
        ss_label_end << if_label_num;
        auto label_end = std::make_shared<string_value>(ss_label_end.str());

        auto has_else_call = input.value->size() == 4;
        auto jump_operator = is_if_statement ? vm_operator::jump_false : vm_operator::jump_true;

        auto comparison_value = input.value->at(1);
        auto comparison_call = dynamic_cast<const array_value *>(comparison_value.get());
        if (!comparison_call)
        {
            throw std::runtime_error("Condition needs comparison to be an array");
        }

        auto first_block_value = input.value->at(2);
        auto first_block_call = dynamic_cast<const array_value *>(first_block_value.get());
        if (!first_block_call)
        {
            throw std::runtime_error("Condition needs first block to be an array");
        }

        auto result = parse(comparison_value);

        if (has_else_call)
        {
            // Jump to else if the condition doesn't match
            result.emplace_back(jump_operator, label_else);

            // First block of code
            push_range(result, parse_flatten(first_block_value));
            // Jump after the condition, skipping second block of code.
            result.emplace_back(vm_operator::jump, label_end);

            // Jump target for else
            result.emplace_back(ss_label_else.str());

            // Second 'else' block of code
            auto second_block_value = input.value->at(2);
            auto second_block_call = dynamic_cast<const array_value *>(second_block_value.get());
            if (!second_block_call)
            {
                throw std::runtime_error("Condition else needs second block to be an array");
            }

            push_range(result, parse_flatten(second_block_value));
        }
        else
        {
            result.emplace_back(jump_operator, label_end);

            push_range(result, parse_flatten(first_block_value));
        }

        result.emplace_back(ss_label_end.str());

        return result;
    }

    std::vector<lisp_assembler::temp_code_line> lisp_assembler::parse_flatten(std::shared_ptr<ivalue> input)
    {
        auto is_array = dynamic_cast<const array_value *>(input.get());
        if (is_array)
        {
            auto all_array = true;
            for (auto iter : *is_array->value)
            {
                if (dynamic_cast<const array_value *>(iter.get()) == nullptr)
                {
                    all_array = false;
                    break;
                }
            }

            if (all_array)
            {
                std::vector<temp_code_line> result;
                for (auto iter : *is_array->value)
                {
                    push_range(result, parse(iter));
                }
                return result;
            }
        }

        return parse(input);
    }

    std::vector<lisp_assembler::temp_code_line> lisp_assembler::parse_loop_jump(const std::string &keyword, bool jump_to_start)
    {
        if (loop_stack.size() == 0)
        {
            throw std::runtime_error("Unexpected keyword outside of loop");
        }

        auto loop_label = loop_stack.top();
        loop_stack.pop();

        std::vector<temp_code_line> result;
        result.emplace_back(vm_operator::jump, jump_to_start ? loop_label.start : loop_label.end);
        return result;
    }

    std::shared_ptr<function> lisp_assembler::parse_function(const array_value &input)
    {
        std::vector<std::string> parameters;
        auto parameters_array = dynamic_cast<const array_value *>(input.value->at(1).get());
        for (auto iter : *parameters_array->value)
        {
            parameters.emplace_back(iter->to_string());
        }

        std::vector<temp_code_line> temp_code_lines;
        for (auto i = 2; i < input.value->size(); i++)
        {
            push_range(temp_code_lines, parse(input.value->at(i)));
        }

        return process_temp_function(parameters, temp_code_lines);
    }

    std::vector<lisp_assembler::temp_code_line> lisp_assembler::parse_change_variable(std::shared_ptr<ivalue> input, builtin_function_value change_func)
    {
        auto var_name = std::make_shared<string_value>(input->to_string());
        auto num_args = std::make_shared<number_value>(1);

        array_vector call_array_args;
        call_array_args.emplace_back(std::make_shared<builtin_function_value>(change_func));
        call_array_args.emplace_back(num_args);
        auto call_array_values = std::make_shared<array_value>(call_array_args, false);

        std::vector<temp_code_line> result;
        result.emplace_back(vm_operator::get, var_name);
        result.emplace_back(vm_operator::call_direct, call_array_values);
        result.emplace_back(vm_operator::set, var_name);

        return result;
    }

    std::vector<lisp_assembler::temp_code_line> lisp_assembler::parse_keyword(const std::string &keyword, const array_value &input)
    {
        std::vector<temp_code_line> result;
        if (keyword == keyword_function)
        {
            auto function = parse_function(input);
            auto function_value = std::make_shared<lysithea_vm::function_value>(function);

            result.emplace_back(vm_operator::push, function_value);
            return result;
        }
        if (keyword == keyword_continue) { return parse_loop_jump(keyword, true); }
        if (keyword == keyword_break) { return parse_loop_jump(keyword, false); }
        if (keyword == keyword_set) { return parse_set(input); }
        if (keyword == keyword_define) { return parse_define(input); }
        if (keyword == keyword_loop) { return parse_loop(input); }
        if (keyword == keyword_if) { return parse_cond(input, true); }
        if (keyword == keyword_unless) { return parse_cond(input, false); }
        // if (keyword == keyword_inc) { return parse_change_variable(input.value->at(1), false); }

        return result;
    }

    std::shared_ptr<function> lisp_assembler::process_temp_function(const std::vector<std::string> &parameters, const std::vector<temp_code_line> &temp_code_lines)
    {
        std::unordered_map<std::string, int> labels;
        std::vector<code_line> code;

        for (const auto &temp_line : temp_code_lines)
        {
            if (temp_line.is_label())
            {
                labels[temp_line.jump_label] = code.size();
            }
            else
            {
                code.emplace_back(temp_line.op, temp_line.argument);
            }
        }

        return std::make_shared<function>(code, parameters, labels);
    }
} // lysithea_vm