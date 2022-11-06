#include "scope.hpp"

namespace stack_vm
{
    scope::scope() { }
    scope::scope(std::shared_ptr<scope> parent): parent(parent) { }

    void scope::clear()
    {
        values.clear();
    }

    void scope::combine_scope(const scope &input)
    {
        for (auto iter : input.values)
        {
            values[iter.first] = iter.second;
        }
    }

    void scope::define(const std::string &key, value input)
    {
        values[key] = input;
    }

    void scope::define(const std::string &key, builtin_function_callback callback)
    {
        values[key] = value::make_builtin(callback);
    }

    bool scope::try_set(const std::string &key, value input)
    {
        auto find = values.find(key);
        if (find != values.end())
        {
            values[key] = input;
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
        if (find != values.end())
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
} // stack_vm