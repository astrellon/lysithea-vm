#pragma once

#include <memory>
#include <string>

namespace stack_vm
{
    class ivalue;

    class scope
    {
        public:
            // Fields

            // Constructor
            scope();
            scope(std::shared_ptr<scope> parent);

            // Methods
            void clear();
            void combine_scope(const scope &input);

            void define(const std::string &key, std::shared_ptr<ivalue> value);

        private:
            // Fields
            std::shared_ptr<scope> parent;

            // Methods
    };
} // stack_vm