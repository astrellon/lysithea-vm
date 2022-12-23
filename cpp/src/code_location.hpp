#pragma once

#include <string>
#include <sstream>

namespace lysithea_vm
{
    class code_location
    {
        public:
            // Fields
            int start_line_number;
            int end_line_number;
            int start_column_number;
            int end_column_number;

            // Constructor
            code_location(): code_location(0, 0, 0, 0) { }
            code_location(int line_number, int column_number): code_location(line_number, column_number, line_number, column_number) { }
            code_location(int start_line_number, int start_column_number, int end_line_number, int end_column_number):
                start_line_number(start_line_number), start_column_number(start_column_number),
                end_line_number(end_line_number), end_column_number(end_column_number) { }

            // Methods
            std::string to_string() const
            {
                std::stringstream ss;
                ss << start_line_number << ':' << start_column_number << " -> " << end_line_number << ':' << end_column_number;
                return ss.str();
            }
    };
} // lysithea_vm