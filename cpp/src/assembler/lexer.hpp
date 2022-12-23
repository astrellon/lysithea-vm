#pragma once

#include <string>
#include <vector>

#include "../values/value.hpp"
#include "./token.hpp"

namespace lysithea_vm
{
    class tokeniser;

    class lexer
    {
        public:
            // Fields

            // Methods
            static token read_from_text(const std::vector<std::string> &input_lines);
            static token read_from_parser(tokeniser &input);
            static value parse_constant(const std::string &input);

            static token parse_list(tokeniser &input, bool is_expression, const std::string &end_token);
            static token parse_map(tokeniser &input);

    };
} // lysithea_vm