#include <iostream>

#include "src/value.hpp"

using namespace stack_vm;

int main()
{
    Value value1(5.0);
    Value value2(true);
    Value value3("Hello there");

    object_value obj{{"name", Value("Priya")}};
    Value value4(obj);

    std::cout << "Value1: " << value1.to_string() << "\n";
    std::cout << "Value2: " << value2.to_string() << "\n";
    std::cout << "Value3: " << value3.to_string() << "\n";
    std::cout << "Value4: " << value4.to_string() << "\n";

    return 0;
}