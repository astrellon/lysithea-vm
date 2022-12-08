#include "token.hpp"

namespace lysithea_vm
{
    token itoken::copy(value new_token_value) const
    {
        return token(location, new_token_value);
    }

    token itoken::to_empty() const
    {
        return token(location);
    }
} // lysithea_vm