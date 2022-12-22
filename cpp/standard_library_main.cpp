#include <iostream>

#include <random>
#include <fstream>
#include <chrono>

#include "src/virtual_machine.hpp"
#include "src/assembler/assembler.hpp"
#include "src/standard_library/standard_library.hpp"
#include "src/standard_library/standard_assert_library.hpp"

using namespace lysithea_vm;

int main()
{
    const char *filename = "../../examples/testStandardLibrary.lys";
    std::ifstream input_file;
    input_file.open(filename);
    if (!input_file)
    {
        std::cout << "Could not find file to open!\n";
        return -1;
    }

    lysithea_vm::assembler assembler;
    lysithea_vm::standard_library::add_to_scope(assembler.builtin_scope);
    assembler.builtin_scope.combine_scope(*lysithea_vm::standard_assert_library::library_scope);

    auto script = assembler.parse_from_stream(filename, input_file);

    lysithea_vm::virtual_machine vm(32);

    auto start = std::chrono::steady_clock::now();
    vm.execute(script);
    auto end = std::chrono::steady_clock::now();

    std::cout << "Time taken: " << std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count() << "ms\n";

    return 0;
}