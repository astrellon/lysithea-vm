#include "virtual_machine_mini.hpp"

namespace stack_vm
{
    virtual_machine_mini::virtual_machine_mini(int stack_size, stack_vm::run_handler_mini global_run_handler) : stack(stack_size), stack_trace(stack_size), program_counter(0), running(false), global_run_handler(global_run_handler)
    {
    }

    void virtual_machine_mini::set_global_run_handler(stack_vm::run_handler_mini handler)
    {
        global_run_handler = handler;
    }

    void virtual_machine_mini::set_current_scope(const std::shared_ptr<scope> scope)
    {
        current_scope = scope;
    }

    void virtual_machine_mini::reset()
    {
        program_counter = 0;
        stack.clear();
        running = false;
    }

    value virtual_machine_mini::get_arg(const code_line &input)
    {
        if (input.value.has_value())
        {
            return input.value.value();
        }

        return pop_stack();
    }

    void virtual_machine_mini::step()
    {
        if (program_counter >= current_scope->code.size())
        {
            running = false;
            return;
        }

        // print_stack_debug();

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
                    auto top = peek_stack();
                    stack.push(top);
                }
                else
                {
                    stack.push(code_line.value.value());
                }
                break;
            }
            case vm_operator::pop:
            {
                pop_stack();
                break;
            }
            case vm_operator::swap:
            {
                const auto &value = get_arg(code_line);
                if (value.is_number())
                {
                    swap(static_cast<int>(value.get_number()));
                }
                else
                {
                    throw std::runtime_error("Swap operator needs a number value");
                }
                break;
            }
            case vm_operator::copy:
            {
                const auto &value = get_arg(code_line);
                if (value.is_number())
                {
                    copy(static_cast<int>(value.get_number()));
                }
                else
                {
                    throw std::runtime_error("Copy operator needs a number value");
                }
                break;
            }
            case vm_operator::jump:
            {
                const auto &label = get_arg(code_line);
                jump(label.to_string());
                break;
            }
            case vm_operator::jump_true:
            {
                const auto &label = get_arg(code_line);
                const auto &top = pop_stack();
                if (top.is_true())
                {
                    jump(label.to_string());
                }
                break;
            }
            case vm_operator::jump_false:
            {
                const auto &label = get_arg(code_line);
                const auto &top = pop_stack();
                if (top.is_false())
                {
                    jump(label.to_string());
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
                run_command(top);
                break;
            }
        }
    }

    void virtual_machine_mini::run_command(const value &input)
    {
        global_run_handler(input.to_string(), *this);
    }

    void virtual_machine_mini::call(const value &input)
    {
        stack_trace.push(program_counter);
        jump(input.to_string());
    }

    void virtual_machine_mini::call_return()
    {
        int top;
        if (!stack_trace.pop(top))
        {
            throw std::runtime_error("Unable to pop stack track, empty stack");
        }

        program_counter = top;
    }

    void virtual_machine_mini::jump(const std::string &label)
    {
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

    void virtual_machine_mini::swap(int top_offset)
    {
        stack.swap(top_offset);
    }

    void virtual_machine_mini::copy(int top_offset)
    {
        stack.copy(top_offset);
    }

    void virtual_machine_mini::print_stack_debug()
    {
        const auto &data = stack.stack_data();
        std::cout << "Stack size: " << data.size() << "\n";
        for (const auto &iter : data)
        {
            std::cout << "- " << iter.to_string() << "\n";
        }
    }
} // namespace stack_vm
