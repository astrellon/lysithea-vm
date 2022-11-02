#include "assembler.hpp"

#include <sstream>

#include "./parser.hpp"
#include "./utils.hpp"
#include "./values/function_value.hpp"
#include "./values/variable_value.hpp"

namespace stack_vm
{
    const std::string assembler::keyword_function("function");
    const std::string assembler::keyword_loop("loop");
    const std::string assembler::keyword_continue("continue");
    const std::string assembler::keyword_break("break");
    const std::string assembler::keyword_if("if");
    const std::string assembler::keyword_unless("unless");
    const std::string assembler::keyword_set("set");
    const std::string assembler::keyword_define("define");
    const std::string assembler::keyword_inc("inc");
    const std::string assembler::keyword_dec("dec");

    std::string assembler::temp_code_line::to_string() const
    {
        if (is_label())
        {
            return jump_label;
        }

        std::stringstream result;
        result << stack_vm::to_string(op);
        if (argument)
        {
            result << ": " << argument->to_string();
        }
        else
        {
            result << ": <no arg>";
        }

        return result.str();
    }

    assembler::assembler() : label_count(0)
    {

    }

    std::shared_ptr<script> assembler::parse_from_text(const std::string &input)
    {
        std::stringstream stream(input);
        return parse_from_stream(stream);
    }

    std::shared_ptr<script> assembler::parse_from_stream(std::istream &input)
    {
        auto parsed = parser::read_from_stream(input);
        return parse_from_value(parsed);
    }

    std::shared_ptr<script> assembler::parse_from_value(const array_value &input)
    {
        auto code = parse_global_function(input);

        auto script_scope = std::make_shared<scope>();
        script_scope->combine_scope(builtin_scope);

        return std::make_shared<script>(script_scope, code);
    }

    std::shared_ptr<function> assembler::parse_global_function(const array_value &input)
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

    std::vector<assembler::temp_code_line> assembler::parse(std::shared_ptr<ivalue> input)
    {
        std::vector<temp_code_line> result;
        auto array_input = std::dynamic_pointer_cast<array_value>(input);
        if (array_input)
        {
            if (array_input->value->size() == 0)
            {
                return result;
            }

            auto first = array_input->value->at(0);
            // If the first item in an array is a symbol we assume that it is a function call or a label
            auto first_symbol_value = std::dynamic_pointer_cast<variable_value>(first);
            if (first_symbol_value)
            {
                if (first_symbol_value->is_label())
                {
                    result.emplace_back(*first_symbol_value->value);
                    return result;
                }

                // Check for keywords
                auto keyword_parse = parse_keyword(*first_symbol_value->value, *array_input);
                if (keyword_parse.size() > 0)
                {
                    return keyword_parse;
                }

                // Handle general opcode or function call.
                for (auto iter = array_input->value->cbegin() + 1; iter != array_input->value->cend(); ++iter)
                {
                    push_range(result, parse(*iter));
                }

                // If it is not an opcode then it must be a function call
                auto op_code = parse_operator(*first_symbol_value->value);
                if (op_code == vm_operator::unknown)
                {
                    push_range(result, optimise_call_symbol_value(*first_symbol_value->value, array_input->value->size() - 1));
                }
                else if (op_code != vm_operator::push)
                {
                    result.emplace_back(op_code);
                }

                return result;
            }
        }
        else
        {
            auto symbol_value = std::dynamic_pointer_cast<variable_value>(input);
            if (symbol_value && !symbol_value->is_label())
            {
                return optimise_get_symbol_value(*symbol_value->value);
            }
        }

        result.emplace_back(vm_operator::push, input);
        return result;
    }

    std::vector<assembler::temp_code_line> assembler::parse_set(const array_value &input)
    {
        auto result = parse(input.value->at(2));
        result.emplace_back(vm_operator::set, input.value->at(1));
        return result;
    }

    std::vector<assembler::temp_code_line> assembler::parse_define(const array_value &input)
    {
        auto result = parse(input.value->at(2));
        result.emplace_back(vm_operator::define, input.value->at(1));

        auto is_function = std::dynamic_pointer_cast<function>(result[0].argument);
        if (is_function)
        {
            is_function->name = input.value->at(1)->to_string();
        }
        return result;
    }

    std::vector<assembler::temp_code_line> assembler::parse_loop(const array_value &input)
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
        auto comparison_call = std::dynamic_pointer_cast<const array_value>(comparison_value);
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

    std::vector<assembler::temp_code_line> assembler::parse_cond(const array_value &input, bool is_if_statement)
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
        auto comparison_call = std::dynamic_pointer_cast<const array_value>(comparison_value);
        if (!comparison_call)
        {
            throw std::runtime_error("Condition needs comparison to be an array");
        }

        auto first_block_value = input.value->at(2);
        auto first_block_call = std::dynamic_pointer_cast<const array_value>(first_block_value);
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
            auto second_block_call = std::dynamic_pointer_cast<const array_value>(second_block_value);
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

    std::vector<assembler::temp_code_line> assembler::parse_flatten(std::shared_ptr<ivalue> input)
    {
        auto is_array = std::dynamic_pointer_cast<const array_value>(input);
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

    std::vector<assembler::temp_code_line> assembler::parse_loop_jump(const std::string &keyword, bool jump_to_start)
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

    std::shared_ptr<function> assembler::parse_function(const array_value &input)
    {
        std::vector<std::string> parameters;
        auto parameters_array = std::dynamic_pointer_cast<const array_value>(input.value->at(1));
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

    std::vector<assembler::temp_code_line> assembler::parse_change_variable(std::shared_ptr<ivalue> input, builtin_function_value change_func)
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

    std::vector<assembler::temp_code_line> assembler::parse_keyword(const std::string &keyword, const array_value &input)
    {
        std::vector<temp_code_line> result;
        if (keyword == keyword_function)
        {
            auto function = parse_function(input);
            auto function_value = std::make_shared<stack_vm::function_value>(function);

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

    std::vector<assembler::temp_code_line> assembler::optimise_call_symbol_value(const std::string &variable, int num_args)
    {
        auto num_arg_value = std::make_shared<number_value>(num_args);

        std::shared_ptr<string_value> parent_key;
        std::shared_ptr<array_value> property;
        auto is_property = is_get_property_request(variable, parent_key, property);

        std::vector<temp_code_line> result;
        result.emplace_back(vm_operator::get, parent_key);
        if (is_property)
        {
            result.emplace_back(vm_operator::get_property, property);
        }
        result.emplace_back(vm_operator::call, num_arg_value);

        return result;
    }

    std::vector<assembler::temp_code_line> assembler::optimise_get_symbol_value(const std::string &variable)
    {
        std::string get_name = variable;
        auto is_argument_unpack = starts_with_unpack(variable);
        if (is_argument_unpack)
        {
            get_name = get_name.substr(3);
        }

        std::shared_ptr<string_value> parent_key;
        std::shared_ptr<array_value> property;
        auto is_property = is_get_property_request(get_name, parent_key, property);

        std::vector<temp_code_line> result;
        result.emplace_back(vm_operator::get, parent_key);
        if (is_property)
        {
            result.emplace_back(vm_operator::get_property, property);
        }

        if (is_argument_unpack)
        {
            result.emplace_back(vm_operator::to_argument);
        }

        return result;
    }

    bool assembler::is_get_property_request(const std::string &input, std::shared_ptr<string_value> &parent_key, std::shared_ptr<array_value> &property)
    {
        auto find = input.find('.');
        if (find != input.npos)
        {
            auto split = string_split(input, ".");
            parent_key = std::make_shared<string_value>(split[0]);

            array_vector property_vector;
            for (auto i = 1; i < split.size(); i++)
            {
                property_vector.emplace_back(std::make_shared<string_value>(split[i]));
            }
            property = std::make_shared<array_value>(property_vector, false);

            return true;
        }

        parent_key = std::make_shared<string_value>(input);
        return false;
    }

    std::shared_ptr<function> assembler::process_temp_function(const std::vector<std::string> &parameters, const std::vector<temp_code_line> &temp_code_lines)
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
} // stack_vm