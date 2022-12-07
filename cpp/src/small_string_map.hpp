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
            // #define vector_item std::pair<std::string, T>
            // #define vector_data std::vector<vector_item>
            using data_iter = typename std::vector<std::pair<std::string, T>>::iterator;
            using data_const_iter = typename std::vector<std::pair<std::string, T>>::const_iterator;
            // Fields

            // Constructor

            // Methods
            void clear()
            {
                _data.clear();
            }

            data_iter find(const std::string &key)
            {
                for (auto iter = _data.begin(); iter != _data.end(); ++iter)
                {
                    if (iter->first == key)
                    {
                        return iter;
                    }
                }

                return _data.end();
                // return std::find_if(_data.begin(), _data.end(), [&key](const std::pair<std::string, T> &i)
                // {
                //     return i.first == key;
                // });
            }

            data_const_iter find(const std::string &key) const
            {
                for (auto iter = _data.cbegin(); iter != _data.cend(); ++iter)
                {
                    if (iter->first == key)
                    {
                        return iter;
                    }
                }

                return _data.cend();
                // return std::find_if(_data.cbegin(), _data.cend(), [&key](const std::pair<std::string, T> &i)
                // {
                //     return i.first == key;
                // });
            }

            void set(const std::string &key, T value)
            {
                auto iter = find(key);
                if (iter == _data.end())
                {
                    _data.emplace_back(key, value);
                }
                else
                {
                    iter->second = value;
                }
            }

            // inline std::vector<std::pair<std::string, T>>::iterator begin() { return _data.begin(); }
            // inline std::vector<std::pair<std::string, T>>::iterator end() { return _data.end(); }

            // inline std::vector<std::pair<std::string, T>>::const_iterator cbegin() const { return _data.begin(); }
            // inline std::vector<std::pair<std::string, T>>::const_iterator cend() const { return _data.end(); }

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