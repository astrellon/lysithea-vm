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
            fixed_stack(int size) : max_size(size) { }

            // Methods
            inline void clear()
            {
                data.clear();
            }

            inline bool pop(T &result)
            {
                if (stack_size() < max_size)
                {
                    result = data.back();
                    data.pop_back();
                    return true;
                }

                return false;
            }

            inline bool push(T value)
            {
                if (stack_size() < max_size)
                {
                    data.emplace_back(value);
                    return true;
                }

                return false;
            }

            inline bool peek(T& result) const
            {
                if (data.size() == 0)
                {
                    return false;
                }

                result = data.back();
                return true;
            }

            inline int stack_size() const { return static_cast<int>(data.size()); }

            const std::vector<T> stack_data() const
            {
                return data;
            }

        private:
            // Fields
            std::vector<T> data;
            int max_size;

            // Methods
    };
} // stack_vm