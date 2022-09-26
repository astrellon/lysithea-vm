#pragma once

#include <functional>
#include <vector>
#include <stack>
#include <string>
#include <memory>

#include "operator.hpp"
#include "code_line.hpp"
#include "scope.hpp"
#include "value.hpp"
#include "fixed_stack.hpp"

namespace stack_vm
{
    class virtual_machine_mini;

    using run_handler_mini = std::function<void (const std::string &, virtual_machine_mini &)>;

    class virtual_machine_mini
    {
        public:
            // Fields
            bool running;

            // Constructor
            virtual_machine_mini(int stackSize, run_handler_mini global_run_handler);

            // Methods
            void set_current_scope(const std::shared_ptr<scope> scope);
            void reset();
            void step();

            void jump(const std::string &label);

            void run_command(const value &label);

            void swap(int top_offset);
            void copy(int top_offset);

            inline value pop_stack()
            {
                value result;
                if (!stack.pop(result))
                {
                    throw std::runtime_error("Unable to pop stack, empty stack");
                }
                return result;
            }

            inline void push_stack(value input)
            {
                if (!stack.push(input))
                {
                    throw std::runtime_error("Unable to push stack, stack full");
                }
            }

            inline value peek_stack() const
            {
                value result;
                if (!stack.peek(result))
                {
                    throw std::runtime_error("Unable to peek stack, empty stack");
                }
                return result;
            }

            void print_stack_debug();

        private:
            // Fields
            fixed_stack<value> stack;
            std::shared_ptr<scope> current_scope;
            stack_vm::run_handler_mini global_run_handler;
            int program_counter;

            // Methods
            value get_arg(const code_line &input);
    };
} // stack_vm