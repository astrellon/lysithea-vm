#pragma once

#include <string>
#include <vector>
#include <algorithm>

namespace lysithea_vm
{
    template <typename T>
    class small_string_map
    {
        public:
            // Methods
            void clear()
            {
                _data.clear();
            }

            bool contains(const std::string &key)
            {
                for (auto iter : _data)
                {
                    if (iter.first == key)
                    {
                        return true;
                    }
                }

                return false;
            }
            bool try_get(const std::string &key, T& value) const
            {
                for (auto iter : _data)
                {
                    if (iter.first == key)
                    {
                        value = iter.second;
                        return true;
                    }
                }

                return false;
            }

            void set(const std::string &key, T value)
            {
                for (auto iter = _data.begin(); iter != _data.end(); ++iter)
                {
                    if (iter->first == key)
                    {
                        iter->second = value;
                        return;
                    }
                }

                _data.emplace_back(key, value);
            }

            inline const std::vector<std::pair<std::string, T>> data() const
            {
                return _data;
            }

        private:
            // Fields
            std::vector<std::pair<std::string, T>> _data;

            // Methods
    };
} // lysithea_vm