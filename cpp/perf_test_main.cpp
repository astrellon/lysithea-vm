#include <iostream>

#include <random>
#include <fstream>
#include <chrono>

#include "src/assembler/assembler.hpp"
#include "src/errors/virtual_machine_error.hpp"
#include "src/values/values.hpp"
#include "src/virtual_machine.hpp"

std::random_device _rd;
std::mt19937 _rand(_rd());
std::uniform_real_distribution<double> dist(0.0, 1.0);

std::shared_ptr<lysithea_vm::scope> create_custom_scope()
{
    auto result = std::make_shared<lysithea_vm::scope>();

    result->try_set_constant("rand", [](lysithea_vm::virtual_machine &vm, const lysithea_vm::array_value &args) -> void
    {
        vm.push_stack(dist(_rand));
    });

    result->try_set_constant("print", [](lysithea_vm::virtual_machine &vm, const lysithea_vm::array_value &args) -> void
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
    const char *filename = "../../examples/perfTest.lys";

    std::ifstream input_file;
    input_file.open(filename);
    if (!input_file)
    {
        std::cout << "Could not find file to open!\n";
        return -1;
    }

    auto custom_scope = create_custom_scope();

    lysithea_vm::assembler assembler;
    assembler.builtin_scope.combine_scope(*custom_scope);

    auto script = assembler.parse_from_stream(filename, input_file);

    lysithea_vm::virtual_machine vm(16);

    try
    {
        auto start = std::chrono::steady_clock::now();
        vm.execute(script);
        auto end = std::chrono::steady_clock::now();

        std::cout << "Time taken: " << std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count() << "ms\n";
    }
    catch (const lysithea_vm::virtual_machine_error &exp)
    {
        std::cerr << exp.what() << "\n";
        for (const auto &line : exp.stack_trace)
        {
            std::cerr << line << "\n";
        }
    }

    return 0;
}