#pragma once

#include <istream>
#include <sstream>
#include <string>
#include "./values/array_value.hpp"

namespace stack_vm
{
    class parser
    {
        public:
            // Fields
            std::string current;

            // Constructor
            parser(std::istream &input);

            // Methods
            bool move_next();

            static array_value read_from_stream(std::istream &input);
            static array_value read_from_text(const std::string &input);
            static std::shared_ptr<ivalue> read_from_parser(parser &input);
            static std::shared_ptr<ivalue> atom(const std::string &input);

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