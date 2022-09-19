#pragma once

#include <vector>
#include <exception>

namespace stack_vm
{
    template <typename T>
    class fixed_stack
    {
        public:
            // Fields

            // Constructor
            fixed_stack(int size) : data(size), index(-1) { }

            // Methods
            inline void clear()
            {
                index = -1;
            }

            inline bool pop()
            {
                if (index < 0)
                {
                    return false;
                }

                --index;
                return true;
            }

            inline bool pop(T* result)
            {
                if (index < 0)
                {
                    return false;
                }

                *result = data[index--];
                return true;
            }

            inline bool push(T value)
            {
                if (index >= data.capacity())
                {
                    return false;
                }

                data[++index] = value;
                return true;
            }

            inline bool peek(T* result)
            {
                if (index < 0)
                {
                    return false;
                }

                *result = data[index];
                return true;
            }

            inline int stack_size() const { return data.capacity(); }

        private:
            // Fields
            const std::vector<T> data;
            int index;

            // Methods
    };
} // stack_vm