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
    class parser
    {
        public:
            // Fields
            std::string current;

            // Constructor
            parser(const std::vector<std::string> &input);

            // Methods
            bool move_next();
            code_location current_location() const;

            const std::vector<std::string> &input_data() const
            {
                return input;
            }

            static std::shared_ptr<token_list> read_from_text(const std::vector<std::string> &input_lines);
            static std::shared_ptr<itoken> read_from_parser(parser &input);
            static value atom(const std::string &input);

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