#include <map>
#include <unordered_map>
#include <sstream>
#include <string>
#include <iostream>
#include <random>
#include <chrono>

#include "./src/small_string_map.hpp"

std::random_device _rd;
std::mt19937 _rand(_rd());

int randInt(int num)
{
    std::uniform_int_distribution<int> dist(0, num);
    return dist(_rand);
}

uint32_t hash_string(const char * s)
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

void testSmallMap(int num)
{
    lysithea_vm::small_string_map<int> map;

    for (auto i = 0; i < num; i++)
    {
        std::stringstream ss;
        ss << "index_" << i;
        map.set(ss.str(), i);
    }

    auto start = std::chrono::steady_clock::now();
    auto total = 0;
    for (auto i = 0; i < 1000000; i++)
    {
        auto randNum = randInt(num);
        std::stringstream ss;
        ss << "index_" << randNum;

        int found;
        if (map.try_get(ss.str(), found))
        {
            total += found;
        }
    }
    auto end = std::chrono::steady_clock::now();

    std::cout << "Small Map Done: " << num << ": total: " << total << "\n";
    std::cout << "- Time taken: " << std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count() << "ms\n";
}

void testMap(int num)
{
    std::map<std::uint32_t, int> map;

    for (auto i = 0; i < num; i++)
    {
        std::stringstream ss;
        ss << "index_" << i;
        auto hash = hash_string(ss.str().c_str());
        map.insert(std::pair<std::uint32_t, int>(hash, i));
    }

    auto start = std::chrono::steady_clock::now();
    auto total = 0;
    for (auto i = 0; i < 1000000; i++)
    {
        auto randNum = randInt(num);
        std::stringstream ss;
        ss << "index_" << randNum;
        auto hash = hash_string(ss.str().c_str());

        int found;
        auto find = map.find(hash);
        if (find != map.end())
        {
            total += find->second;
        }
    }
    auto end = std::chrono::steady_clock::now();

    std::cout << "Map Done: " << num << ": total: " << total << "\n";
    std::cout << "- Time taken: " << std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count() << "ms\n";

}
void testUnorderedMap(int num)
{
    std::unordered_map<std::uint32_t, int> map;

    for (auto i = 0; i < num; i++)
    {
        std::stringstream ss;
        ss << "index_" << i;
        auto hash = hash_string(ss.str().c_str());
        map.insert(std::pair<std::uint32_t, int>(hash, i));
    }

    auto start = std::chrono::steady_clock::now();
    auto total = 0;
    for (auto i = 0; i < 1000000; i++)
    {
        auto randNum = randInt(num);
        std::stringstream ss;
        ss << "index_" << randNum;
        auto hash = hash_string(ss.str().c_str());


        int found;
        auto find = map.find(hash);
        if (find != map.end())
        {
            total += find->second;
        }
    }
    auto end = std::chrono::steady_clock::now();

    std::cout << "Unordered Map Done: " << num << ": total: " << total << "\n";
    std::cout << "- Time taken: " << std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count() << "ms\n";
}

int main()
{
    std::vector<int> nums{2, 10, 100, 1000 };

    for (auto num : nums)
    {
        std::cout << "---- For Nums: " << num << " ----\n";
        testSmallMap(num);
        testMap(num);
        testUnorderedMap(num);
        std::cout << "\n";
    }

    return 0;
}