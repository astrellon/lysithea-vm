#include <iostream>

#include <random>
#include <fstream>
#include <chrono>

#include "src/assembler/assembler.hpp"
#include "src/assembler/parser.hpp"
#include "src/values/values.hpp"
#include "src/virtual_machine.hpp"

std::random_device _rd;
std::mt19937 _rand(_rd());
std::uniform_real_distribution<double> dist(0.0, 1.0);

std::shared_ptr<lysithea_vm::scope> create_custom_scope()
{
    auto result = std::make_shared<lysithea_vm::scope>();

    result->define("rand", [](lysithea_vm::virtual_machine &vm, const lysithea_vm::array_value &args) -> void
    {
        vm.push_stack(dist(_rand));
    });

    result->define("print", [](lysithea_vm::virtual_machine &vm, const lysithea_vm::array_value &args) -> void
    {
        for (auto iter : args.data)
        {
            std::cout << iter.to_string();
        }
        std::cout << "\n";
    });

    return result;
}

int main()
{
    std::ifstream input_file;
    input_file.open("../../examples/perfTest.lys");
    if (!input_file)
    {
        std::cout << "Could not find file to open!\n";
        return -1;
    }

    auto custom_scope = create_custom_scope();

    // auto parsed = lysithea_vm::parser::read_from_(input_file);
    lysithea_vm::assembler assembler;
    assembler.builtin_scope.combine_scope(*custom_scope);

    auto script = assembler.parse_from_stream("perfTest.lys", input_file);

    lysithea_vm::virtual_machine vm(16);

    auto start = std::chrono::steady_clock::now();
    vm.execute(script);
    auto end = std::chrono::steady_clock::now();

    std::cout << "Time taken: " << std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count() << "ms\n";

    return 0;
}