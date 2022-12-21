#pragma once

#include <istream>
#include <sstream>
#include <string>
#include <vector>
#include <memory>

#include "../values/value.hpp"
#include "../values/array_value.hpp"
#include "../code_location.hpp"
#include "./token.hpp"

namespace lysithea_vm
{
    class tokeniser
    {
        public:
            // Fields
            std::string current;

            // Constructor
            tokeniser(const std::vector<std::string> &input);

            // Methods
            bool move_next();
            code_location current_location() const;

            const std::vector<std::string> &input_data() const
            {
                return input;
            }

            int end_line_number() const { return line_number; }
            int end_column_number() const { return column_number; }

            static std::shared_ptr<std::vector<std::string>> split_stream(std::istream &input);
            static std::shared_ptr<std::vector<std::string>> split_text(const std::string &input);

        private:
            // Fields
            char in_quote;
            char return_symbol;
            bool escaped;
            bool in_comment;
            int line_number;
            int column_number;
            int start_line_number;
            int start_column_number;
            std::stringstream accumulator;
            int accumulator_size;
            const std::vector<std::string> &input;

            // Methods
            void append_char(char ch);
            void reset_accumulator();
    };
} // lysithea_vm