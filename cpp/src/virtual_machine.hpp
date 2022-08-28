#pragma once

#include <functional>
#include <vector>
#include <stack>
#include <map>
#include <string>
#include <memory>

#include "operator.hpp"
#include "code_line.hpp"
#include "scope.hpp"
#include "value.hpp"

namespace stack_vm
{
    class virtual_machine;

    using run_handler = std::function<void(const value &, virtual_machine &)>;

    class scope_frame
    {
        public:
            // Fields
            const int line_counter;
            const std::shared_ptr<stack_vm::scope> scope;

            // Constructor
            scope_frame() : line_counter(0), scope(nullptr) { }
            scope_frame(int line_counter, std::shared_ptr<stack_vm::scope> scope) : line_counter(line_counter), scope(scope) { }

            // Methods
    };

    class virtual_machine
    {
        public:
            // Fields

            // Constructor
            virtual_machine(int stackSize, run_handler run_handler);

            // Methods
            void add_scope(std::shared_ptr<scope> scope);
            void add_scopes(const std::vector<std::shared_ptr<scope>> scopes);
            void run(const std::string &start_scope_name);
            void stop();
            void pause(bool value);
            void step();

            void call(const value &label);
            void jump(const value &label);
            void jump(const std::string &label, const std::string &scope_name);
            void call_return();

            inline value pop_stack()
            {
                auto end = stack.top();
                stack.pop();
                return end;
            }

            inline void push_stack(value input)
            {
                stack.push(input);
            }

        private:
            // Fields
            std::stack<value> stack;
            std::stack<scope_frame> stack_trace;
            std::map<std::string, std::shared_ptr<scope>> scopes;
            std::shared_ptr<scope> current_scope;
            int program_counter;
            int stack_size;
            bool running;
            bool paused;
            stack_vm::run_handler run_handler;

            // Methods
    };
} // stack_vm