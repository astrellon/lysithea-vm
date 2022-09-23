#pragma once

#include <functional>
#include <vector>
#include <stack>
#include <unordered_map>
#include <string>
#include <memory>

#include "operator.hpp"
#include "code_line.hpp"
#include "scope.hpp"
#include "value.hpp"
#include "fixed_stack.hpp"

namespace stack_vm
{
    class virtual_machine;

    using run_handler = std::function<void (const std::string &, virtual_machine &)>;

    class scope_frame
    {
        public:
            // Fields
            int line_counter;
            std::shared_ptr<stack_vm::scope> scope;

            // Constructor
            scope_frame() : line_counter(0), scope(nullptr) { }
            scope_frame(int line_counter, std::shared_ptr<stack_vm::scope> scope) : line_counter(line_counter), scope(scope) { }

            // Methods
    };

    class virtual_machine
    {
        public:
            // Fields
            bool running;
            bool paused;

            // Constructor
            virtual_machine(int stackSize, run_handler global_run_handler);

            // Methods
            void add_scope(std::shared_ptr<scope> scope);
            void add_scopes(const std::vector<std::shared_ptr<scope>> scopes);
            void add_run_handler(const std::string &handler_name, run_handler handler);
            void set_current_scope(const std::string &scope_name);
            void reset();
            void step();

            void call(const value &label);
            void jump(const value &label);
            void jump(const std::string &label);
            void jump(const std::string &label, const std::string &scope_name);
            void call_return();

            void run_command(const value &label);

            void swap(int top_offset);

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
            fixed_stack<scope_frame> stack_trace;
            std::unordered_map<std::string, std::shared_ptr<scope>> scopes;
            std::shared_ptr<scope> current_scope;
            int program_counter;
            std::unordered_map<std::string, stack_vm::run_handler> run_handlers;
            stack_vm::run_handler global_run_handler;

            // Methods
            value get_arg(const code_line &input);
    };
} // stack_vm