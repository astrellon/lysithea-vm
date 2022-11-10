#pragma once

#include <string>
#include <memory>
#include "../values/value.hpp"

namespace lysithea_vm
{
    class scope;

    class standard_string_library
    {
        public:
            // Fields
            static std::shared_ptr<const scope> library_scope;

            // Methods
            static std::shared_ptr<scope> create_scope();

            static value length(const std::string &target);
            static value set(const std::string &target, int index, const std::string &input);
            static value get(const std::string &target, int index);
            static value insert(const std::string &target, int index, const std::string &input);
            static value substring(const std::string &target, int index, int length);
            static value remove_at(const std::string &target, int index);
            static value remove_all(const std::string &target, const std::string &values);
            static value join(const std::string &separator, const std::vector<value>::const_iterator begin, const std::vector<value>::const_iterator end);

            inline static int get_index(const std::string &input, int index)
            {
                if (index < 0)
                {
                    return input.size() + index;
                }

                return index;
            }

        private:
            // Constructor
            standard_string_library() { };
    };
} // lysithea_vm