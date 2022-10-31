#pragma once

#include <functional>
#include <unordered_map>
#include <string>
#include <memory>

#include "operator.hpp"
#include "code_line.hpp"
#include "scope.hpp"
#include "script.hpp"
#include "function.hpp"
#include "fixed_stack.hpp"
#include "./values/ivalue.hpp"
#include "./values/array_value.hpp"
#include "./values/string_value.hpp"
#include "./values/bool_value.hpp"

namespace stack_vm
{
    class scope_frame
    {
        public:
            // Fields
            int line_counter;
            std::shared_ptr<function> function;
            std::shared_ptr<scope> scope;

            // Constructor
            scope_frame() : line_counter(0), function(nullptr), scope(nullptr) { }
            scope_frame(int line_counter, std::shared_ptr<stack_vm::function> function, std::shared_ptr<stack_vm::scope> scope) : line_counter(line_counter), function(function), scope(scope) { }

            // Methods
    };

    class virtual_machine
    {
        public:
            // Fields
            bool running;
            bool paused;
            std::shared_ptr<const scope> builtinScope;
            std::shared_ptr<function> current_code;

            // Constructor
            virtual_machine(int stackSize);

            // Methods
            void reset();
            void change_to_script(std::shared_ptr<script> input);
            void execute(std::shared_ptr<script> input);
            void step();
            void jump(const std::string &label);

            // Function methods
            std::shared_ptr<array_value> get_args(int num_args);
            void call_function(ifunction_value &value, int num_args, bool push_to_stack_trace);
            void execute_function(function &func, const array_value &args, bool push_to_stack_trace);
            void push_to_stack_trace(scope_frame &frame);

            // Stack methods
            inline ivalue pop_stack()
            {
                ivalue result;
                if (!stack.pop(result))
                {
                    throw std::runtime_error("Unable to pop stack, empty stack");
                }
                return result;
            }

            inline void push_stack(const ivalue &input)
            {
                if (!stack.push(input))
                {
                    throw std::runtime_error("Unable to push stack, stack full");
                }
            }

            inline ivalue peek_stack() const
            {
                ivalue result;
                if (!stack.peek(result))
                {
                    throw std::runtime_error("Unable to peek stack, empty stack");
                }
                return result;
            }

            void print_stack_debug();

        private:
            // Fields
            fixed_stack<ivalue> stack;
            fixed_stack<scope_frame> stack_trace;
            std::shared_ptr<scope> current_scope;
            std::shared_ptr<scope> global_scope;

            int program_counter;

            // Methods
            inline ivalue get_arg(const code_line &input)
            {
                if (input.value)
                {
                    return input.value;
                }

                return pop_stack();
            }

            template <typename T>
            inline T get_arg(const code_line &input)
            {
                auto result = input.value;
                if (!input.value)
                {
                    result = pop_stack();
                }

                return dynamic_cast<T *>(result.get());
            }
    };
} // stack_vm