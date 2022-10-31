#include "virtual_machine.hpp"

#include <cmath>
#include <stdexcept>

namespace stack_vm
{
    virtual_machine::virtual_machine(int stack_size) : stack(stack_size), stack_trace(stack_size), program_counter(0), running(false), paused(false)
    {
    }

    void virtual_machine::reset()
    {
        program_counter = 0;
        stack.clear();
        stack_trace.clear();
        running = false;
        paused = false;
    }

    void virtual_machine::step()
    {
        if (program_counter >= current_code->code.size())
        {
            running = false;
            return;
        }

        // print_stack_debug();

        const auto &code_line = current_code->code[program_counter++];

        switch (code_line.op)
        {
            default:
            {
                throw std::runtime_error("Unknown operator");
            }
            case vm_operator::push:
            {
                if (code_line.value)
                {
                    stack.push(code_line.value);
                }
                else
                {
                    throw std::runtime_error("Push needs an input");
                }
                break;
            }
            case vm_operator::to_argument:
            {
                const auto top = get_operator_arg<iarray_value>(code_line);
                if (!top)
                {
                    throw std::runtime_error("Unable to convert input to argument");
                }

                const auto values = top->array_values();
                this->push_stack(std::make_shared<array_value>(values));
            }
            case vm_operator::get
            {
                break;
            }
            case vm_operator::get_property:
            {
                break;
            }
            case vm_operator::define:
            {
                break;
            }
            case vm_operator::set:
            {
                break;
            }
            case vm_operator::jump_false:
            {
                const auto &label = get_operator_arg(code_line);
                const auto &top = pop_stack();
                if (top.is_false())
                {
                    jump(label);
                }
                break;
            }
            case vm_operator::jump_true:
            {
                const auto &label = get_operator_arg(code_line);
                const auto &top = pop_stack();
                if (top.is_true())
                {
                    jump(label);
                }
                break;
            }
            case vm_operator::jump:
            {
                const auto label = get_operator_arg(code_line);
                jump(label);
                break;
            }
            case vm_operator::call_return:
            {
                call_return();
                break;
            }
            case vm_operator::call:
            {
                const auto &label = get_operator_arg(code_line);
                call(label);
                break;
            }
            case vm_operator::call_direct:
            {
                break;
            }
        }
    }

    std::shared_ptr<array_value> virtual_machine::get_args(int num_args)
    {
        if (num_args == 0)
        {
            array_vector empty;
            return std::make_shared<array_value>(empty, true);
        }

        auto has_arguments = false;
        array_vector temp(num_args);
        for (auto i = 0; i < num_args; i++)
        {
            auto value = pop_stack();
            auto is_arg = dynamic_cast<const array_value *>(value.get());
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
                auto is_arg = dynamic_cast<const array_value *>(iter.get());
                if (is_arg && is_arg->is_arguments_value)
                {
                    for (const auto &arg_iter : *is_arg->value)
                    {
                        combined.push_back(arg_iter);
                    }
                }
                else
                {
                    combined.push_back(iter);
                }
            }

            return std::make_shared<array_value>(combined, true);
        }

        return std::make_shared<array_value>(temp, true);
    }

    void virtual_machine::jump(const std::string &label)
    {
        auto find = current_code->labels.find(label);
        if (find == current_code->labels.end())
        {
            throw std::runtime_error("Unable to jump to label");
        }

        program_counter = find->second;
    }

    void virtual_machine::call_function(const ifunction_value &value, int num_args, bool push_to_stack_trace)
    {
        auto args = get_args(num_args);
        value.invoke(*this, args, push_to_stack_trace);
    }

    void virtual_machine::execute_function(std::shared_ptr<function> func, std::shared_ptr<array_value> args, bool push_to_stack_trace)
    {
        if (push_to_stack_trace)
        {
            // push_stack()
        }

        current_code = func;
        current_scope = std::make_shared<scope>(current_scope);
        program_counter = 0;

        auto num_called_args = std::min(args->value->size(), func->parameters.size());
        for (auto i = 0; i < num_called_args; i++)
        {
            const auto &arg_name = func->parameters[i];
            auto starts_with_unpack = arg_name.length() > 3 &&
                arg_name[0] == '.' && arg_name[1] == '.' && arg_name[2] == '.';
            if (starts_with_unpack)
            {
                // TODO
                break;
            }
            current_scope->define(arg_name, args->value->at(i));
        }
    }

    void virtual_machine::call(const value &input)
    {
        stack_trace.push(scope_frame(program_counter, current_scope));
        jump(input);
    }

    void virtual_machine::jump(const value &input)
    {
        if (input.is_string())
        {
            jump(*input.get_string().get());
        }
        else if (input.is_array())
        {
            const auto &list = *std::get<std::shared_ptr<array_value>>(input.data);
            if (list.size() == 0)
            {
                throw std::runtime_error("Cannot jump to empty array");
            }

            const auto &label = list[0].to_string();
            if (list.size() > 1)
            {
                jump(label, list[1].to_string());
            }
            else
            {
                jump(label, "");
            }
        }
    }


    void virtual_machine::jump(const std::string &label, const std::string &scope_name)
    {
        if (scope_name.size() > 0)
        {
            auto find = scopes.find(scope_name);
            if (find == scopes.end())
            {
                throw std::runtime_error("Unable to find scope to jump to");
            }
            else
            {
                current_scope = find->second;
            }
        }

        if (label.size() == 0)
        {
            program_counter = 0;
            return;
        }

        auto find_label = current_scope->labels.find(label);
        if (find_label == current_scope->labels.end())
        {
            throw std::runtime_error("Unable to find label in current scope to jump to");
        }

        program_counter = find_label->second;
    }

    void virtual_machine::call_return()
    {
        scope_frame top;
        if (!stack_trace.pop(top))
        {
            throw std::runtime_error("Unable to pop stack track, empty stack");
        }

        current_scope = top.scope;
        program_counter = top.line_counter;
    }

    void virtual_machine::swap(int top_offset)
    {
        stack.swap(top_offset);
    }

    void virtual_machine::copy(int top_offset)
    {
        stack.copy(top_offset);
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
} // namespace stack_vm
