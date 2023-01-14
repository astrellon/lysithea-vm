#pragma once

#include <string>
#include <sstream>
#include <vector>
#include <cmath>

#include "../code_location.hpp"

namespace lysithea_vm
{
    std::string create_error_log_at(const std::string &source_name, const code_location &location, const std::vector<std::string> &full_text);

} // lysithea_vm