#include "scope.hpp"

#include <iostream>

namespace lysithea_vm
{
    scope::scope() { }
    scope::scope(std::shared_ptr<scope> parent): parent(parent) { }

    void scope::clear()
    {
        values.clear();
    }

    void scope::combine_scope(const scope &input)
    {
        for (auto iter : input.values.data())
        {
            values.set(iter.first, iter.second);
        }
    }

    void scope::define(const std::string &key, value input)
    {
        values.set(key, input);
    }

    void scope::define(const std::string &key, builtin_function_callback callback)
    {
        values.set(key, value::make_builtin(callback));
    }

    bool scope::try_set(const std::string &key, value input)
    {
        if (values.contains(key))
        {
            values.set(key, input);
            return true;
        }

        if (parent)
        {
            return parent->try_set(key, input);
        }

        return false;
    }

    bool scope::try_get_key(const std::string &key, value &result) const
    {
        if (values.try_get(key, result))
        {
            return true;
        }

        if (parent)
        {
            return parent->try_get_key(key, result);
        }

        return false;
    }

    bool scope::try_get_number(const std::string &key, double &result) const
    {
        value found;
        if (try_get_key(key, found))
        {
            if (found.is_number())
            {
                result = found.get_number();
                return true;
            }
        }

        return false;
    }

    bool scope::try_get_bool(const std::string &key, bool &result) const
    {
        value found;
        if (try_get_key(key, found))
        {
            if (found.is_bool())
            {
                result = found.get_bool();
                return true;
            }
        }

        return false;
    }
} // lysithea_vm