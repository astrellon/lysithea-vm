#include "token.hpp"

namespace lysithea_vm
{
    std::shared_ptr<itoken> itoken::copy(value new_token_value) const
    {
        return std::make_shared<token>(location, new_token_value);
    }

    std::shared_ptr<itoken> itoken::to_empty() const
    {
        return std::make_shared<token>(location);
    }
} // lysithea_vm