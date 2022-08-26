#include "virtual_machine.hpp"

namespace stack_vm
{
    virtual_machine::virtual_machine(int stack_size, stack_vm::run_handler run_handler) : stack_size(stack_size), program_counter(0), running(false), paused(false), run_handler(run_handler)
    {
        stack.reserve(stack_size);
        stack_trace.reserve(stack_size);
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

    void virtual_machine::run(const std::string &startScopeName)
    {
        if (startScopeName.size() > 0)
        {
            auto find = scopes.find(startScopeName);
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

    void virtual_machine::step()
    {
        if (program_counter >= current_scope->code.size())
        {
            stop();
            return;
        }

        const auto &codeLine = current_scope->code[program_counter++];

        switch (codeLine.op)
        {
            default:
            {
                throw std::runtime_error("Unknown operator");
            }
            case vm_operator::push:
            {
                if (!codeLine.value.has_value())
                {
                    throw std::runtime_error("Push code line requires input");
                }

                stack.push_back(codeLine.value.value());
                break;
            }
            case vm_operator::pop:
            {
                stack.pop_back();
                break;
            }
            case vm_operator::jump:
            {
                const auto &label = pop_stack();
                jump(label);
                break;
            }
            case vm_operator::jump_true:
            {
                const auto &label = pop_stack();
                const auto &top = pop_stack();
                if (top.is_true())
                {
                    jump(label);
                }
                break;
            }
            case vm_operator::jump_false:
            {
                const auto &label = pop_stack();
                const auto &top = pop_stack();
                if (top.is_false())
                {
                    jump(label);
                }
                break;
            }
            case vm_operator::call:
            {
                const auto &label = pop_stack();
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
                const auto &top = pop_stack();
                run_handler(top, *this);
                break;
            }
        }
    }

    void virtual_machine::call(const value &input)
    {
        stack_trace.emplace_back(program_counter, current_scope);
        jump(input);
    }

    void virtual_machine::jump(const value &input)
    {
        if (input.is_string())
        {
            jump(std::get<std::string>(input.data), "");
        }
        else if (input.is_array())
        {
            const auto &list = std::get<array_value>(input.data);
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

    void virtual_machine::jump(const std::string &label, const std::string &scopeName)
    {
        if (scopeName.size() > 0)
        {
            auto find = scopes.find(scopeName);
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

        auto findLabel = current_scope->labels.find(label);
        if (findLabel == current_scope->labels.end())
        {
            throw std::runtime_error("Unable to find label in current scope to jump to");
        }

        program_counter = findLabel->second;
    }

    void virtual_machine::call_return()
    {
        if (stack_trace.size() == 0)
        {
            throw std::runtime_error("Unable to return, stack trace empty");
        }

        auto last = stack_trace.back();
        stack_trace.pop_back();
        current_scope = last.scope;
        program_counter = last.line_counter;
    }

    value virtual_machine::pop_stack()
    {
        auto end = stack.back();
        stack.pop_back();
        return end;
    }

    void virtual_machine::push_stack(value input)
    {
        stack.push_back(input);
    }
} // namespace stack_vm
