#include "standard_assert_library.hpp"

#include <sstream>

#include "../virtual_machine.hpp"
#include "../utils.hpp"

namespace stack_vm
{
    const std::string &standard_assert_library::handle_name = "assert";

    void standard_assert_library::add_handler(virtual_machine &vm)
    {
        vm.add_run_handler(handle_name, handler);
    }

    void standard_assert_library::handler(const std::string &command, virtual_machine &vm)
    {
        switch (hash(command))
        {
            case hash("true"):
                {
                    const auto &top = vm.pop_stack();
                    if (!top.is_true())
                    {
                        vm.running = false;
                        std::cout << "Assert expected true\n";
                    }
                    break;
                }
            case hash("false"):
                {
                    const auto &top = vm.pop_stack();
                    if (!top.is_false())
                    {
                        vm.running = false;
                        std::cout << "Assert expected false\n";
                    }
                    break;
                }
            case hash("equals"):
                {
                    const auto &to_compare = vm.pop_stack();
                    const auto &top = vm.peek_stack();
                    if (top.compare(to_compare) != 0)
                    {
                        vm.running = false;
                        std::cout << "Assert expected equals:"
                            << "\nExpected: " << to_compare.to_string()
                            << "\nActual: " << top.to_string()
                            << "\n";
                    }
                    break;
                }
            case hash("notEquals"):
                {
                    const auto &to_compare = vm.pop_stack();
                    const auto &top = vm.peek_stack();
                    if (top.compare(to_compare) == 0)
                    {
                        vm.running = false;
                        std::cout << "Assert expected not equals:"
                            << "\nExpected: " << to_compare.to_string()
                            << "\nActual: " << top.to_string()
                            << "\n";
                    }
                    break;
                }
        }
    }
}