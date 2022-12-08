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
                auto hash = hash_string(key.c_str());
                for (auto iter : _data)
                {
                    if (iter.first == hash)
                    {
                        return true;
                    }
                }

                return false;
            }
            bool try_get(const std::string &key, T& value) const
            {
                auto hash = hash_string(key.c_str());
                for (auto iter : _data)
                {
                    if (iter.first == hash)
                    {
                        value = iter.second;
                        return true;
                    }
                }

                return false;
            }

            void set(const std::string &key, T value)
            {
                auto hash = hash_string(key.c_str());
                for (auto iter = _data.begin(); iter != _data.end(); ++iter)
                {
                    if (iter->first == hash)
                    {
                        iter->second = value;
                        return;
                    }
                }

                _data.emplace_back(hash, value);
            }

            inline const std::vector<std::pair<std::uint32_t, T>> data() const
            {
                return _data;
            }

        private:
            // Fields
            std::vector<std::pair<std::uint32_t, T>> _data;

            // Methods
            static uint32_t hash_string(const char * s)
            {
                uint32_t hash = 0;

                for (; *s; ++s)
                {
                    hash += *s;
                    hash += (hash << 10);
                    hash ^= (hash >> 6);
                }

                hash += (hash << 3);
                hash ^= (hash >> 11);
                hash += (hash << 15);

                return hash;
            }
    };
} // lysithea_vm