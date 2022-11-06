#include "assembler.hpp"

#include <sstream>

#include "./parser.hpp"
#include "./utils.hpp"
#include "./values/function_value.hpp"
#include "./values/variable_value.hpp"
#include "./values/value_property_access.hpp"
#include "./standard_library/standard_math_library.hpp"
#include "./virtual_machine.hpp"

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

    const builtin_function_value assembler::inc_number([](virtual_machine &vm, const array_value &args) -> void
    {
        vm.push_stack(args.get_number(0) + 1);
    });
    const builtin_function_value assembler::dec_number([](virtual_machine &vm, const array_value &args) -> void
    {
        vm.push_stack(args.get_number(0) - 1);
    });

    std::string assembler::temp_code_line::to_string() const
    {
        if (is_label())
        {
            return jump_label;
        }

        std::stringstream result;
        result << stack_vm::to_string(op);
        if (!argument.is_null())
        {
            result << ": " << argument.to_string();
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
        for (const auto &iter : input.data)
        {
            auto lines = parse(iter);
            push_range(temp_code_lines, lines);
        }

        std::vector<std::string> empty_parameters;
        auto code = process_temp_function(empty_parameters, temp_code_lines);
        code->name = "global";

        return code;
    }

    std::vector<assembler::temp_code_line> assembler::parse(value input)
    {
        std::vector<temp_code_line> result;
        auto array_input = input.get_complex<array_value>();
        if (array_input)
        {
            if (array_input->data.size() == 0)
            {
                return result;
            }

            auto first = array_input->data[0];
            // If the first item in an array is a symbol we assume that it is a function call or a label
            auto first_symbol_value = first.get_complex<variable_value>();
            if (first_symbol_value)
            {
                if (first_symbol_value->is_label())
                {
                    result.emplace_back(first_symbol_value->data);
                    return result;
                }

                // Check for keywords
                auto keyword_parse = parse_keyword(first_symbol_value->data, *array_input);
                if (keyword_parse.size() > 0)
                {
                    return keyword_parse;
                }

                // Handle general opcode or function call.
                for (auto iter = array_input->data.cbegin() + 1; iter != array_input->data.cend(); ++iter)
                {
                    push_range(result, parse(*iter));
                }

                // If it is not an opcode then it must be a function call
                auto op_code = parse_operator(first_symbol_value->data);
                if (op_code == vm_operator::unknown)
                {
                    push_range(result, optimise_call_symbol_value(first_symbol_value->data, array_input->data.size() - 1));
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
            auto symbol_value = input.get_complex<variable_value>();
            if (symbol_value && !symbol_value->is_label())
            {
                return optimise_get_symbol_value(symbol_value->data);
            }
        }

        result.emplace_back(vm_operator::push, input);
        return result;
    }

    std::vector<assembler::temp_code_line> assembler::parse_set(const array_value &input)
    {
        auto result = parse(input.data[2]);
        result.emplace_back(vm_operator::set, input.data[1]);
        return result;
    }

    std::vector<assembler::temp_code_line> assembler::parse_define(const array_value &input)
    {
        auto result = parse(input.data[2]);
        result.emplace_back(vm_operator::define, input.data[1]);

        auto is_function = result[0].argument.get_complex<function_value>();
        if (is_function)
        {
            is_function->data->name = input.data[1].to_string();
        }
        return result;
    }

    std::vector<assembler::temp_code_line> assembler::parse_loop(const array_value &input)
    {
        if (input.data.size() < 3)
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

        auto comparison_value = input.data[1];
        auto comparison_call = comparison_value.get_complex<const array_value>();
        if (!comparison_call)
        {
            throw std::runtime_error("Loop comparison input needs to be an array");
        }

        push_range(result, parse(comparison_value));
        result.emplace_back(vm_operator::jump_false, label_end);

        for (auto i = 2; i < input.data.size(); i++)
        {
            push_range(result, parse(input.data[i]));
        }

        result.emplace_back(vm_operator::jump, label_start);
        result.emplace_back(ss_label_end.str());

        loop_stack.pop();
        return result;
    }

    std::vector<assembler::temp_code_line> assembler::parse_cond(const array_value &input, bool is_if_statement)
    {
        if (input.data.size() < 3)
        {
            throw std::runtime_error("Condition input has too few inputs");
        }
        if (input.data.size() < 3)
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

        auto has_else_call = input.data.size() == 4;
        auto jump_operator = is_if_statement ? vm_operator::jump_false : vm_operator::jump_true;

        auto comparison_value = input.data[1];
        auto comparison_call = comparison_value.get_complex<const array_value>();
        if (!comparison_call)
        {
            throw std::runtime_error("Condition needs comparison to be an array");
        }

        auto first_block_value = input.data[2];
        auto first_block_call = first_block_value.get_complex<const array_value>();
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
            auto second_block_value = input.data[2];
            auto second_block_call = second_block_value.get_complex<const array_value>();
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

    std::vector<assembler::temp_code_line> assembler::parse_flatten(value input)
    {
        auto is_array = input.get_complex<const array_value>();
        if (is_array)
        {
            auto all_array = true;
            for (auto iter : is_array->data)
            {
                if (!iter.get_complex<const array_value>())
                {
                    all_array = false;
                    break;
                }
            }

            if (all_array)
            {
                std::vector<temp_code_line> result;
                for (auto iter : is_array->data)
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
        auto parameters_array = input.data[1].get_complex<const array_value>();
        for (auto iter : parameters_array->data)
        {
            parameters.emplace_back(iter.to_string());
        }

        std::vector<temp_code_line> temp_code_lines;
        for (auto i = 2; i < input.data.size(); i++)
        {
            push_range(temp_code_lines, parse(input.data[i]));
        }

        return process_temp_function(parameters, temp_code_lines);
    }

    std::vector<assembler::temp_code_line> assembler::parse_change_variable(value input, builtin_function_value change_func)
    {
        auto var_name = std::make_shared<string_value>(input.to_string());
        value num_args(1);

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
        if (keyword == keyword_inc) { return parse_change_variable(input.data[1], inc_number); }
        if (keyword == keyword_dec) { return parse_change_variable(input.data[1], dec_number); }

        return result;
    }

    std::vector<assembler::temp_code_line> assembler::optimise_call_symbol_value(const std::string &variable, int num_args)
    {
        std::vector<temp_code_line> result;
        value num_arg_value(num_args);

        std::shared_ptr<string_value> parent_key;
        std::shared_ptr<array_value> property;
        auto is_property = is_get_property_request(variable, parent_key, property);

        // Check if we know about the parent object? (eg: string.length, the parent is the string object)
        value found_parent;
        if (builtin_scope.try_get_key(parent_key->data, found_parent))
        {
            // If the get is for a property? (eg: string.length, length is the property)
            value found_property;
            if (is_property && try_get_property(found_parent, *property, found_property))
            {
                if (found_property.is_function())
                {
                    // If we found the property then we're done and we can just push that known value onto the stack.
                    array_vector call_vector;
                    call_vector.emplace_back(found_property);
                    call_vector.emplace_back(num_arg_value);

                    auto call_value = std::make_shared<array_value>(call_vector, false);
                    result.emplace_back(vm_operator::call_direct, call_value);
                    return result;
                }

                throw std::runtime_error("Attempting to call a value that is not a function");
            }
            else if (!is_property)
            {
                // This was not a property request but we found the parent so just push onto the stack.
                if (found_parent.is_function())
                {
                    array_vector call_vector;
                    call_vector.emplace_back(found_parent);
                    call_vector.emplace_back(num_arg_value);

                    auto call_value = std::make_shared<array_value>(call_vector, false);
                    result.emplace_back(vm_operator::call_direct, call_value);
                    return result;
                }

                throw std::runtime_error("Attempting to call a value that is not a function");
            }
        }

        // Could not find the parent right now, so look for the parent at runtime.
        result.emplace_back(vm_operator::get, parent_key);

        // If this was also a property check also look up the property at runtime.
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

        value found_parent;
        // Check if we know about the parent object? (eg: string.length, the parent is the string object)
        if (builtin_scope.try_get_key(parent_key->data, found_parent))
        {
            // If the get is for a property? (eg: string.length, length is the property)
            if (is_property)
            {
                value found_property;
                if (try_get_property(found_parent, *property, found_property))
                {
                    // If we found the property then we're done and we can just push that known value onto the stack.
                    result.emplace_back(vm_operator::push, found_property);
                }
                else
                {
                    // We didn't find the property at compile time, so look it up at run time.
                    result.emplace_back(vm_operator::push, found_parent);
                    result.emplace_back(vm_operator::get_property, property);
                }
            }
            else
            {
                // This was not a property request but we found the parent so just push onto the stack.
                result.emplace_back(vm_operator::push, found_parent);
            }
        }
        else
        {
            // Could not find the parent right now, so look for the parent at runtime.
            result.emplace_back(vm_operator::get, parent_key);

            // If this was also a property check also look up the property at runtime.
            if (is_property)
            {
                result.emplace_back(vm_operator::get_property, property);
            }
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