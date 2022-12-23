#include "scope.hpp"

#include <iostream>

namespace lysithea_vm
{
    scope::scope() { }
    scope::scope(std::shared_ptr<scope> parent): parent(parent) { }

    void scope::clear()
    {
        values.clear();
        constants.clear();
    }

    void scope::combine_scope(const scope &input)
    {
        for (auto iter : input.values)
        {
            values[iter.first] = iter.second;
        }

        for (auto iter : input.constants)
        {
            constants[iter.first] = iter.second;
        }
    }

    bool scope::has_key(const std::string &key) const
    {
        auto find = values.find(key);
        return find != values.cend();
    }

    bool scope::try_define(const std::string &key, value input)
    {
        if (is_constant(key))
        {
            return false;
        }

        values[key] = input;
        return true;
    }

    bool scope::try_define(const std::string &key, builtin_function_callback callback)
    {
        return try_define(key, value::make_builtin(callback));
    }

    bool scope::try_set_constant(const std::string &key, value input)
    {
        if (has_key(key))
        {
            return false;
        }

        try_define(key, input);
        set_constant(key);

        return true;
    }

    bool scope::try_set_constant(const std::string &key, builtin_function_callback callback)
    {
        return try_set_constant(key, value::make_builtin(callback));
    }

    bool scope::try_set(const std::string &key, value input)
    {
        if (is_constant(key))
        {
            return false;
        }

        auto find = values.find(key);
        if (find != values.end())
        {
            find->second = input;
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
        auto find = values.find(key);
        if (find != values.cend())
        {
            result = find->second;
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

    bool scope::is_constant(const std::string &key) const
    {
        auto find = constants.find(key);
        return find != constants.cend();
    }

    void scope::set_constant(const std::string &key)
    {
        constants.emplace(key, true);
    }
} // lysithea_vm