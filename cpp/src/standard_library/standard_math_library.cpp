#include "standard_math_library.hpp"

#include <math.h>

#include "../virtual_machine.hpp"
#include "../utils.hpp"

#define M_DEG_TO_RAD M_PI / 180.0

namespace stack_vm
{
    const std::string &standard_math_library::handle_name = "math";

    void standard_math_library::add_handler(virtual_machine &vm)
    {
        vm.add_run_handler(handle_name, handler);
    }

    void standard_math_library::handler(const std::string &command, virtual_machine &vm)
    {
        switch (hash(command))
        {
            case hash("sin"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(sin(top.get_number()));
                break;
            }
            case hash("cos"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(cos(top.get_number()));
                break;
            }
            case hash("tan"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(tan(top.get_number()));
                break;
            }
            case hash("sinDeg"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(sin(M_DEG_TO_RAD * top.get_number()));
                break;
            }
            case hash("cosDeg"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(cos(M_DEG_TO_RAD * top.get_number()));
                break;
            }
            case hash("tanDeg"):
            {
                const auto &top = vm.pop_stack();
                vm.push_stack(tan(M_DEG_TO_RAD * top.get_number()));
                break;
            }
            case hash("E"):
            {
                vm.push_stack(M_E);
                break;
            }
            case hash("PI"):
            {
                vm.push_stack(M_PI);
                break;
            }

            case hash("pow"):
            {
                const auto &y = vm.pop_stack();
                const auto &x = vm.pop_stack();
                vm.push_stack(pow(x.get_number(), y.get_number()));
                break;
            }
            case hash("exp"):
            {
                const auto &x = vm.pop_stack();
                vm.push_stack(exp(x.get_number()));
                break;
            }
            case hash("floor"):
            {
                const auto &x = vm.pop_stack();
                vm.push_stack(floor(x.get_number()));
                break;
            }
            case hash("ceil"):
            {
                const auto &x = vm.pop_stack();
                vm.push_stack(ceil(x.get_number()));
                break;
            }
            case hash("round"):
            {
                const auto &x = vm.pop_stack();
                vm.push_stack(round(x.get_number()));
                break;
            }
            case hash("isNaN"):
            {
                const auto &x = vm.pop_stack();
                vm.push_stack(std::isnan(x.get_number()));
                break;
            }
            case hash("isFinite"):
            {
                const auto &x = vm.pop_stack();
                vm.push_stack(std::isfinite(x.get_number()));
                break;
            }
            case hash("parse"):
            {
                const auto &check = vm.peek_stack();
                if (check.is_number())
                {
                    break;
                }

                const auto &x = vm.pop_stack();
                vm.push_stack(std::stod(x.to_string()));
                break;
            }

            case hash("log"):
            {
                const auto &x = vm.pop_stack();
                vm.push_stack(log(x.get_number()));
                break;
            }
            case hash("log2"):
            {
                const auto &x = vm.pop_stack();
                vm.push_stack(log2(x.get_number()));
                break;
            }
            case hash("log10"):
            {
                const auto &x = vm.pop_stack();
                vm.push_stack(log10(x.get_number()));
                break;
            }
            case hash("abs"):
            {
                const auto &x = vm.pop_stack();
                vm.push_stack(abs(x.get_number()));
                break;
            }
            case hash("max"):
            {
                const auto &right = vm.pop_stack();
                const auto &left = vm.pop_stack();
                if (left.compare(right) > 0)
                {
                    vm.push_stack(left);
                }
                break;
            }
            case hash("min"):
            {
                const auto &right = vm.pop_stack();
                const auto &left = vm.pop_stack();
                if (left.compare(right) > 0)
                {
                    vm.push_stack(right);
                }
                break;
            }

            case hash("+"):
            case hash("add"):
            {
                const auto &right = vm.pop_stack();
                const auto &left = vm.pop_stack();
                vm.push_stack(left.get_number() + right.get_number());
                break;
            }
            case hash("-"):
            case hash("sub"):
            {
                const auto &right = vm.pop_stack();
                const auto &left = vm.pop_stack();
                vm.push_stack(left.get_number() - right.get_number());
                break;
            }
            case hash("*"):
            case hash("mul"):
            {
                const auto &right = vm.pop_stack();
                const auto &left = vm.pop_stack();
                vm.push_stack(left.get_number() * right.get_number());
                break;
            }
            case hash("/"):
            case hash("command"):
            {
                const auto &right = vm.pop_stack();
                const auto &left = vm.pop_stack();
                vm.push_stack(left.get_number() / right.get_number());
                break;
            }
        }
    }
} // stack_vm