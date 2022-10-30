#pragma once

#include <sstream>
#include <string>

namespace stack_vm
{
    class parser
    {
        public:
            // Fields

            // Constructor
            parser(std::istream &input);

            // Methods
            bool move_next(std::string &output);

        private:
            // Fields
            char in_quote;
            char return_symbol;
            bool escaped;
            bool in_comment;
            std::stringstream accumulator;
            std::istream &input;

            // Methods
    };
} // stack_vm