#include "virtual_machine.hpp"

#include <cmath>
#include <iostream>

#include "./values/value_property_access.hpp"
#include "./values/object_value.hpp"
#include "./standard_library/standard_array_library.hpp"
#include "./utils.hpp"
#include "./errors/virtual_machine_error.hpp"
#include "./errors/error_common.hpp"

namespace lysithea_vm
{
    std::shared_ptr<const array_value> virtual_machine::empty_args(std::make_shared<const array_value>(true));

    virtual_machine::virtual_machine(int stack_size) :
        stack(stack_size), stack_trace(stack_size), program_counter(0), running(false), paused(false),
        global_scope(std::make_shared<scope>())
    {
        current_scope = global_scope;
    }

    void virtual_machine::reset()
    {
        program_counter = 0;
        global_scope = std::make_shared<scope>();
        current_scope = global_scope;
        stack.clear();
        stack_trace.clear();
        running = false;
        paused = false;
    }

    void virtual_machine::change_to_script(std::shared_ptr<script> script)
    {
        program_counter = 0;
        stack.clear();
        stack_trace.clear();

        builtin_scope = script->builtin_scope;
        current_code = script->code;
    }

    void virtual_machine::execute(std::shared_ptr<script> script)
    {
        change_to_script(script);

        running = true;
        paused = false;

        while (running && !paused)
        {
            step();
        }
    }

    void virtual_machine::step()
    {
        if (program_counter >= current_code->code.size())
        {
            if (!try_return())
            {
                running = false;
            }
            return;
        }

        const auto &code_line = current_code->code[program_counter++];

        switch (code_line.op)
        {
            default:
            {
                throw virtual_machine_error(create_stack_trace(), "Unknown operator");
            }
            case vm_operator::push:
            {
                if (!code_line.value.is_undefined())
                {
                    stack.push(code_line.value);
                }
                else
                {
                    throw virtual_machine_error(create_stack_trace(), "Push needs an input");
                }
                break;
            }
            case vm_operator::to_argument:
            {
                auto top = get_operator_arg<array_value>(code_line);
                if (!top)
                {
                    throw virtual_machine_error(create_stack_trace(), std::string("Unable to convert input to argument: ") + top->to_string());
                }

                push_stack(std::make_shared<array_value>(top->data, true));
                break;
            }
            case vm_operator::get:
            {
                auto key = get_operator_arg(code_line);
                auto is_string = key.get_complex<string_value>();
                if (!is_string)
                {
                    throw virtual_machine_error(create_stack_trace(), std::string("Unable to get value, input needs to be a string: ") + key.to_string());
                }

                value found_value;
                if (current_scope->try_get_key(is_string->data, found_value) ||
                    (builtin_scope && builtin_scope->try_get_key(is_string->data, found_value)))
                {
                    push_stack(found_value);
                }
                else
                {
                    throw virtual_machine_error(create_stack_trace(), std::string("Unable to find value to get: ") + key.to_string());
                }
                break;
            }
            case vm_operator::get_property:
            {
                auto key = get_operator_arg<array_value>(code_line);
                if (!key)
                {
                    throw virtual_machine_error(create_stack_trace(), std::string("Unable to get property, input needs to be an array: ") + key->to_string());
                }

                auto top = pop_stack();
                value found;
                if (try_get_property(top, *key, found))
                {
                    push_stack(found);
                }
                else
                {
                    throw virtual_machine_error(create_stack_trace(), std::string("Unable to get property: ") + key->to_string());
                }

                break;
            }
            case vm_operator::define:
            {
                auto key = get_operator_arg(code_line);
                auto value = pop_stack();
                current_scope->try_define(key.to_string(), value);
                break;
            }
            case vm_operator::set:
            {
                auto key = get_operator_arg(code_line);
                auto value = pop_stack();
                if (!current_scope->try_set(key.to_string(), value))
                {
                    throw virtual_machine_error(create_stack_trace(), "Unable to set variable that has not been defined: " + key.to_string());
                }
                break;
            }
            case vm_operator::jump_false:
            {
                const auto label = get_operator_arg(code_line);
                auto top = pop_stack();
                if (top.is_false())
                {
                    jump(label.to_string());
                }
                break;
            }
            case vm_operator::jump_true:
            {
                const auto label = get_operator_arg(code_line);
                auto top = pop_stack();
                if (top.is_true())
                {
                    jump(label.to_string());
                }
                break;
            }
            case vm_operator::jump:
            {
                const auto label = get_operator_arg(code_line);
                jump(label.to_string());
                break;
            }
            case vm_operator::call_return:
            {
                call_return();
                break;
            }
            case vm_operator::call:
            {
                if (!code_line.value.is_number())
                {
                    throw virtual_machine_error(create_stack_trace(), "Call needs a num args code line input");
                }

                auto top = pop_stack();
                if (top.is_function())
                {
                    call_function(*top.get_complex(), code_line.value.get_int(), true);
                }
                else
                {
                    throw virtual_machine_error(create_stack_trace(), "Call needs a function to run");
                }
                break;
            }
            case vm_operator::call_direct:
            {
                auto error = false;
                if (!code_line.value.is_array())
                {
                    throw virtual_machine_error(create_stack_trace(), "Call direct needs an array input");
                }

                auto array_input = code_line.value.get_complex<const array_value>();
                if (array_input->data.size() != 2 ||
                    !array_input->data[0].is_function())
                {
                    throw virtual_machine_error(create_stack_trace(), "Call direct needs two inputs of func and number");
                }

                auto num_args = array_input->data[1];
                if (!num_args.is_number())
                {
                    throw virtual_machine_error(create_stack_trace(), "Call direct needs two inputs of func and number");
                }

                call_function(*array_input->data[0].get_complex(), num_args.get_int(), true);
                break;
            }

            // Misc Operator
            case vm_operator::string_concat:
            {
                if (!code_line.value.is_number())
                {
                    throw virtual_machine_error(create_stack_trace(), "StringConcat operator needs the number of args to concat");
                }

                auto args = get_args(code_line.value.get_int());
                std::stringstream ss;
                for (auto iter : args->data)
                {
                    ss << iter.to_string();
                }
                push_stack(ss.str());
                break;
            }

            // Math Operators
            case vm_operator::add:
            {
                push_stack(get_operator_num(code_line) + pop_stack_number());
                break;
            }

            case vm_operator::sub:
            {
                auto right = get_operator_num(code_line);
                auto left = pop_stack_number();
                push_stack(left - right);
                break;
            }

            case vm_operator::unary_negative:
            {
                push_stack(-pop_stack_number());
                break;
            }

            case vm_operator::multiply:
            {
                push_stack(get_operator_num(code_line) * pop_stack_number());
                break;
            }

            case vm_operator::divide:
            {
                auto right = get_operator_num(code_line);
                auto left = pop_stack_number();
                push_stack(left / right);
                break;
            }

            case vm_operator::inc:
            {
                if (!code_line.value.is_complex())
                {
                    throw virtual_machine_error(create_stack_trace(), "Inc operator needs code line variable");
                }

                auto key = code_line.value.to_string();
                double found_value;
                if (!current_scope->try_get_number(key, found_value))
                {
                    throw virtual_machine_error(create_stack_trace(), "Inc operator could not find variable or was not a number");
                }
                current_scope->try_set(key, value(found_value + 1.0));
                break;
            }

            case vm_operator::dec:
            {
                if (!code_line.value.is_complex())
                {
                    throw virtual_machine_error(create_stack_trace(), "Dec operator needs code line variable");
                }

                auto key = code_line.value.to_string();
                double found_value;
                if (!current_scope->try_get_number(key, found_value))
                {
                    throw virtual_machine_error(create_stack_trace(), "Dec operator could not find variable or was not a number");
                }
                current_scope->try_set(key, value(found_value - 1.0));
                break;
            }

            // Comparison Operators
            case vm_operator::less_than:
            {
                auto right = get_operator_arg(code_line);
                auto left = pop_stack();
                push_stack(left.compare_to(right) < 0);
                break;
            }
            case vm_operator::less_than_equals:
            {
                auto right = get_operator_arg(code_line);
                auto left = pop_stack();
                push_stack(left.compare_to(right) <= 0);
                break;
            }
            case vm_operator::equals:
            {
                auto right = get_operator_arg(code_line);
                auto left = pop_stack();
                push_stack(left.compare_to(right) == 0);
                break;
            }
            case vm_operator::not_equals:
            {
                auto right = get_operator_arg(code_line);
                auto left = pop_stack();
                push_stack(left.compare_to(right) != 0);
                break;
            }
            case vm_operator::greater_than:
            {
                auto right = get_operator_arg(code_line);
                auto left = pop_stack();
                push_stack(left.compare_to(right) > 0);
                break;
            }
            case vm_operator::greater_than_equals:
            {
                auto right = get_operator_arg(code_line);
                auto left = pop_stack();
                push_stack(left.compare_to(right) >= 0);
                break;
            }

            // Boolean Operators
            case vm_operator::op_and:
            {
                push_stack(get_operator_bool(code_line) && pop_stack_bool());
                break;
            }
            case vm_operator::op_or:
            {
                push_stack(get_operator_bool(code_line) || pop_stack_bool());
                break;
            }
            case vm_operator::op_not:
            {
                push_stack(!pop_stack_bool());
                break;
            }

            // Value Create
            case vm_operator::make_array:
            {
                if (!code_line.value.is_number())
                {
                    throw virtual_machine_error(create_stack_trace(), "MakeArray operator needs the number of args to pop");
                }

                auto args = get_args(code_line.value.get_int());
                push_stack(array_value::make_value(args->data));
                break;
            }
            case vm_operator::make_object:
            {
                if (!code_line.value.is_number())
                {
                    throw virtual_machine_error(create_stack_trace(), "MakeObject operator needs the number of args to pop");
                }

                auto args = get_args(code_line.value.get_int());
                push_stack(object_value::join(*args));
                break;
            }
        }
    }

    std::shared_ptr<const array_value> virtual_machine::get_args(int num_args)
    {
        if (num_args == 0)
        {
            array_vector empty;
            return empty_args;
        }

        auto has_arguments = false;
        array_vector temp(num_args);
        for (auto i = 0; i < num_args; i++)
        {
            auto value = pop_stack();
            auto is_arg = value.get_complex<const array_value>();
            if (is_arg && is_arg->is_arguments_value)
            {
                has_arguments = true;
            }
            temp[num_args - i - 1] = value;
        }

        if (has_arguments)
        {
            array_vector combined;
            for (const auto &iter : temp)
            {
                auto is_arg = iter.get_complex<const array_value>();
                if (is_arg && is_arg->is_arguments_value)
                {
                    for (const auto &arg_iter : is_arg->data)
                    {
                        combined.push_back(arg_iter);
                    }
                }
                else
                {
                    combined.push_back(iter);
                }
            }

            return std::make_shared<const array_value>(combined, true);
        }

        return std::make_shared<const array_value>(temp, true);
    }

    void virtual_machine::jump(const std::string &label)
    {
        auto find = current_code->labels.find(label);
        if (find == current_code->labels.cend())
        {
            throw virtual_machine_error(create_stack_trace(), std::string("Unable to jump to label: ") + label);
        }

        program_counter = find->second;
    }

    void virtual_machine::call_function(const complex_value &value, int num_args, bool push_to_stack_trace)
    {
        if (!value.is_function())
        {
            throw virtual_machine_error(create_stack_trace(), std::string("Unable to invoke non function value") + value.to_string());
        }
        auto args = get_args(num_args);
        value.invoke(*this, args, push_to_stack_trace);
    }

    void virtual_machine::execute_function(std::shared_ptr<function> code, std::shared_ptr<const array_value> args, bool push_to_stack_trace)
    {
        if (push_to_stack_trace)
        {
            push_stack_trace(scope_frame(program_counter, current_code, current_scope));
        }

        current_code = code;
        current_scope = std::make_shared<scope>(current_scope);
        program_counter = 0;

        auto num_called_args = std::min(args->data.size(), code->parameters.size());
        auto i = 0;
        for (; i < num_called_args; i++)
        {
            const auto &arg_name = code->parameters[i];
            auto is_unpack = starts_with_unpack(arg_name);
            if (is_unpack)
            {
                current_scope->try_define(arg_name.substr(3), standard_array_library::sublist(args->data, i, -1));
                i++;
                break;
            }
            current_scope->try_define(arg_name, args->data[i]);
        }

        if (i < code->parameters.size())
        {
            const auto &arg_name = code->parameters[i];
            auto is_unpack = starts_with_unpack(arg_name);
            if (is_unpack)
            {
                current_scope->try_define(arg_name.substr(3), array_value::empty);
            }
            else
            {
                throw virtual_machine_error(create_stack_trace(), "Function called without enough arguments");
            }
        }
    }

    bool virtual_machine::try_return()
    {
        scope_frame top;
        if (!stack_trace.pop(top))
        {
            return false;
        }

        current_code = top.code;
        current_scope = top.frame_scope;
        program_counter = top.line_counter;
        return true;
    }

    void virtual_machine::call_return()
    {
        if (!try_return())
        {
            throw virtual_machine_error(create_stack_trace(), "Unable to return, call stack empty");
        }
    }

    void virtual_machine::print_stack_debug()
    {
        const auto &data = stack.stack_data();
        std::cout << "Stack size: " << data.size() << "\n";
        for (const auto &iter : data)
        {
            std::cout << "- " << iter.to_string() << "\n";
        }
    }

    void virtual_machine::print_stack_trace_debug()
    {
        const auto &data = create_stack_trace();
        for (const auto &iter : data)
        {
            std::cout << iter << std::endl;
        }
    }

    std::vector<std::string> virtual_machine::create_stack_trace()
    {
        std::vector<std::string> result;

        result.emplace_back(debug_scope_line(*current_code, program_counter - 1));
        const auto &stack_data = stack_trace.stack_data();
        for (auto i = stack_trace.stack_size() - 1; i >= 0; i--)
        {
            const auto &stack_frame = stack_data[i];
            result.emplace_back(debug_scope_line(*stack_frame.code, stack_frame.line_counter - 1));
        }

        return result;
    }

    std::string virtual_machine::debug_scope_line(const function &func, int line)
    {
        std::stringstream ss;
        ss << "  at [" << func.name << "] in " << func.symbols->source_name;
        if (line >= func.code.size())
        {
            ss << " end of code";
            return ss.str();
        }
        else if (line < 0)
        {
            ss << " before start of code";
            return ss.str();
        }

        code_location location;
        func.symbols->try_get_location(line, location);

        ss << create_error_log_at(func.symbols->source_name, location, *func.symbols->full_text);
        return ss.str();
    }
} // namespace lysithea_vm
