#pragma once

#include <string>
#include <vector>

#include "../values/value.hpp"
#include "../errors/parser_error.hpp"
#include "./token.hpp"

namespace lysithea_vm
{
    class tokeniser;

    class lexer
    {
        public:
            // Fields

            // Methods
            static token read_from_text(const std::string &source_name, const std::vector<std::string> &input_lines);
            static token read_from_parser(const std::string &source_name, tokeniser &input);
            static value parse_constant(const std::string &input);

            static token parse_list(const std::string &source_name, tokeniser &input, bool is_expression, const std::string &end_token);
            static token parse_map(const std::string &source_name, tokeniser &input);

        private:
            // Methods
            static parser_error make_error(const std::string &source_name, const tokeniser &tokeniser, const std::string &at_token, const std::string &message);

    };
} // lysithea_vm