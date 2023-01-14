#include "error_common.hpp"

namespace lysithea_vm
{
    std::string create_error_log_at(const std::string &source_name, const code_location &location, const std::vector<std::string> &full_text)
    {
        std::stringstream ss;
        ss << source_name << ':' << (location.start_line_number + 1) << ':' << (location.start_column_number + 1) << '\n';

        auto from_line_index = std::max(0, location.start_line_number - 1);
        auto to_line_index = std::min(static_cast<int>(full_text.size()), location.start_line_number + 2);

        for (auto i = from_line_index; i < to_line_index; i++)
        {
            std::stringstream line_number_ss;
            line_number_ss << (i + 1);

            auto line_number = line_number_ss.str();

            ss << line_number << ": " << full_text[i] << '\n';

            if (i == location.start_line_number)
            {
                ss << std::string(location.start_column_number + line_number.size() + 1, ' ') << '^';
                auto diff = location.end_column_number - location.start_column_number;
                if (location.end_line_number > location.start_line_number)
                {
                    ss << std::string(full_text[i].size() - location.start_column_number, '-') << '^';
                }
                else if (diff > 0)
                {
                    ss << std::string(diff - 1, '-') << '^';
                }
                ss << '\n';
            }
        }

        return ss.str();
    }
} // lysithea_vm