#include "virtual_machine.hpp"

namespace stack_vm
{
    virtual_machine::virtual_machine(int stack_size, stack_vm::run_handler run_handler) : stack_size(stack_size), program_counter(0), running(false), paused(false), run_handler(run_handler)
    {
    }

    void virtual_machine::add_scope(std::shared_ptr<scope> scope)
    {
        scopes[scope->name] = scope;
    }

    void virtual_machine::add_scopes(std::vector<std::shared_ptr<scope>> scopes)
    {
        for (auto &scope : scopes)
        {
            add_scope(scope);
        }
    }

    void virtual_machine::run(const std::string &start_scope_name)
    {
        if (start_scope_name.size() > 0)
        {
            auto find = scopes.find(start_scope_name);
            if (find == scopes.end())
            {
                throw std::runtime_error("Unable to find start scope");
            }
            else
            {
                current_scope = find->second;
            }
        }

        running = true;
        paused = false;

        while (running && !paused)
        {
            step();
        }
    }

    void virtual_machine::stop()
    {
        running = false;
    }

    void virtual_machine::pause(bool value)
    {
        paused = value;
    }

    value virtual_machine::get_arg(const code_line &input)
    {
        if (input.value.has_value())
        {
            return input.value.value();
        }

        return pop_stack();
    }

    void virtual_machine::step()
    {
        if (program_counter >= current_scope->code.size())
        {
            stop();
            return;
        }

        const auto &code_line = current_scope->code[program_counter++];

        switch (code_line.op)
        {
            default:
            {
                throw std::runtime_error("Unknown operator");
            }
            case vm_operator::push:
            {
                if (!code_line.value.has_value())
                {
                    throw std::runtime_error("Push code line requires input");
                }

                stack.push(code_line.value.value());
                break;
            }
            case vm_operator::jump:
            {
                const auto &label = get_arg(code_line);
                jump(label);
                break;
            }
            case vm_operator::jump_true:
            {
                const auto &label = get_arg(code_line);
                const auto &top = pop_stack();
                if (top.is_true())
                {
                    jump(label);
                }
                break;
            }
            case vm_operator::jump_false:
            {
                const auto &label = get_arg(code_line);
                const auto &top = pop_stack();
                if (top.is_false())
                {
                    jump(label);
                }
                break;
            }
            case vm_operator::call:
            {
                const auto &label = get_arg(code_line);
                call(label);
                break;
            }
            case vm_operator::call_return:
            {
                call_return();
                break;
            }
            case vm_operator::run:
            {
                const auto &top = get_arg(code_line);
                run_handler(top, *this);
                break;
            }
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
            jump(*std::get<std::shared_ptr<std::string>>(input.data), "");
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
        if (stack_trace.size() == 0)
        {
            throw std::runtime_error("Unable to return, stack trace empty");
        }

        auto last = stack_trace.top();
        stack_trace.pop();
        current_scope = last.scope;
        program_counter = last.line_counter;
    }
} // namespace stack_vm
