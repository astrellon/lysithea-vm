#include <iostream>

#include <random>
#include <fstream>
#include <chrono>

#include "src/virtual_machine.hpp"
#include "src/assembler.hpp"
#include "src/parser.hpp"
#include "src/standard_library/standard_library.hpp"
#include "src/standard_library/standard_assert_library.hpp"

using namespace stack_vm;

int main()
{
    std::ifstream input_file;
    // input_file.open("../../examples/benchmark1.lisp");
    // input_file.open("../../examples/testStandardLibrary.lisp");
    input_file.open("../../examples/readmeExamples.lisp");
    if (!input_file)
    {
        std::cout << "Could not find file to open!\n";
        return -1;
    }

    auto parsed = stack_vm::parser::read_from_stream(input_file);
    stack_vm::assembler assembler;
    stack_vm::standard_library::add_to_scope(assembler.builtin_scope);
    assembler.builtin_scope.combine_scope(*stack_vm::standard_assert_library::library_scope);

    auto script = assembler.parse_from_value(parsed);

    stack_vm::virtual_machine vm(32);

    auto start = std::chrono::steady_clock::now();
    vm.execute(script);
    auto end = std::chrono::steady_clock::now();

    std::cout << "Time taken: " << std::chrono::duration_cast<std::chrono::milliseconds>(end - start).count() << "ms\n";

    return 0;
}