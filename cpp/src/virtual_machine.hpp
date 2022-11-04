#pragma once

#include <functional>
#include <unordered_map>
#include <string>
#include <memory>
#include <stdexcept>

#include "operator.hpp"
#include "code_line.hpp"
#include "scope.hpp"
#include "script.hpp"
#include "function.hpp"
#include "fixed_stack.hpp"
#include "./values/value.hpp"
#include "./values/complex_value.hpp"
#include "./values/array_value.hpp"
#include "./values/string_value.hpp"

namespace stack_vm
{
    class scope_frame
    {
        public:
            // Fields
            int line_counter;
            std::shared_ptr<function> code;
            std::shared_ptr<scope> frame_scope;

            // Constructor
            scope_frame() : line_counter(0), code(nullptr), frame_scope(nullptr) { }
            scope_frame(int line_counter, std::shared_ptr<function> code, std::shared_ptr<scope> frame_scope) : line_counter(line_counter), code(code), frame_scope(frame_scope) { }

            // Methods
    };

    class virtual_machine
    {
        public:
            // Fields
            bool running;
            bool paused;
            std::shared_ptr<const scope> builtin_scope;
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
            std::shared_ptr<const array_value> get_args(int num_args);
            void call_function(const complex_value &value, int num_args, bool push_to_stack_trace);
            bool try_return();
            void call_return();
            void execute_function(std::shared_ptr<function> func, std::shared_ptr<const array_value> args, bool push_to_stack_trace);

            // Stack methods
            inline void push_stack_trace(const scope_frame &frame)
            {
                if (!stack_trace.push(frame))
                {
                    throw std::runtime_error("Unable to push to stack trace, stack full");
                }
            }

            template <typename T>
            inline std::shared_ptr<T> pop_stack()
            {
                std::shared_ptr<complex_value> result;
                if (!stack.pop(result))
                {
                    throw std::runtime_error("Unable to pop stack, empty stack");
                }

                auto casted = std::dynamic_pointer_cast<T>(result);
                if (!casted)
                {
                    throw std::bad_cast();
                }
                return casted;
            }

            inline std::shared_ptr<complex_value> pop_stack()
            {
                std::shared_ptr<complex_value> result;
                if (!stack.pop(result))
                {
                    throw std::runtime_error("Unable to pop stack, empty stack");
                }
                return result;
            }

            inline void push_stack(bool input)
            {
                push_stack(std::make_shared<bool_value>(input));
            }

            inline void push_stack(int input)
            {
                push_stack(std::make_shared<number_value>(input));
            }

            inline void push_stack(float input)
            {
                push_stack(std::make_shared<number_value>(input));
            }

            inline void push_stack(double input)
            {
                push_stack(std::make_shared<number_value>(input));
            }

            inline void push_stack(const char *input)
            {
                push_stack(std::make_shared<string_value>(input));
            }

            inline void push_stack(const std::string &input)
            {
                push_stack(std::make_shared<string_value>(input));
            }

            inline void push_stack(std::shared_ptr<complex_value> input)
            {
                if (!stack.push(input))
                {
                    throw std::runtime_error("Unable to push stack, stack full");
                }
            }

            inline std::shared_ptr<complex_value> peek_stack() const
            {
                std::shared_ptr<complex_value> result;
                if (!stack.peek(result))
                {
                    throw std::runtime_error("Unable to peek stack, empty stack");
                }
                return result;
            }

            void print_stack_debug();

        private:
            // Fields
            fixed_stack<stack_vm::value> stack;
            fixed_stack<scope_frame> stack_trace;
            std::shared_ptr<scope> current_scope;
            std::shared_ptr<scope> global_scope;

            static std::shared_ptr<const array_value> empty_args;

            int program_counter;

            // Methods
            inline const complex_value *get_operator_arg(const code_line &input)
            {
                if (input.value)
                {
                    return input.value.get();
                }

                return pop_stack().get();
            }

            template <typename T>
            inline const T *get_operator_arg(const code_line &input)
            {
                auto result = input.value;
                if (!input.value)
                {
                    result = pop_stack();
                }

                return dynamic_cast<const T *>(result.get());
            }
    };
} // stack_vm