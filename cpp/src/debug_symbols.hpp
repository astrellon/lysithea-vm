#pragma once

#include <string>
#include <memory>
#include <vector>

#include "./code_location.hpp"

namespace lysithea_vm
{
    class debug_symbols
    {
        public:
            // Fields
            std::string source_name;
            std::shared_ptr<std::vector<std::string>> full_text;
            std::vector<code_location> code_line_to_text;

            // Constructor
            debug_symbols(const std::string &source_name, std::shared_ptr<std::vector<std::string>> full_text, const std::vector<code_location> &code_line_to_text):
                source_name(source_name), full_text(full_text), code_line_to_text(code_line_to_text)
            {

            }

            // Methods
            bool try_get_location(int line, code_location &result)
            {
                if (line >= 0 && line < code_line_to_text.size())
                {
                    result = code_line_to_text[line];
                    return true;
                }

                return false;
            }
    };
} // lysithea_vm