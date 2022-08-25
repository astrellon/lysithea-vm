#include <iostream>

// #include "src/ivalue.hpp"
// #include "src/string_value.hpp"
// #include "src/bool_value.hpp"
#include "src/value.hpp"

using namespace stack_vm;

int main()
{
    // IValue *test1 = new StringValue("Hello there:");
    // IValue *test2 = new BoolValue(true);
    // StringValue *strTest1 = dynamic_cast<StringValue *>(test1);
    // StringValue *strTest2 = dynamic_cast<StringValue *>(test2);

    // if (strTest1 != nullptr)
    // {
    //     std::cout << "Test1 is str: " << strTest1->value << "\n";
    // }
    // if (strTest2 == nullptr)
    // {
    //     std::cout << "Test2 is not a string\n";
    // }

    Value value1(5.0);
    Value value2(true);
    Value value3("Hello there");

    std::cout << "Value1: " << value1.toString() << "\n";
    std::cout << "Value2: " << value2.toString() << "\n";
    std::cout << "Value3: " << value3.toString() << "\n";

    return 0;
}