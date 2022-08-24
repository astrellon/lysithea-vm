#include <iostream>

#include "src/ivalue.hpp"
#include "src/string_value.hpp"
#include "src/bool_value.hpp"

using namespace stack_vm;

int main()
{
    IValue *test1 = new StringValue("Hello there:");
    IValue *test2 = new BoolValue(true);
    StringValue *strTest1 = dynamic_cast<StringValue *>(test1);
    StringValue *strTest2 = dynamic_cast<StringValue *>(test2);

    if (strTest1 != nullptr)
    {
        std::cout << "Test1 is str: " << strTest1->value << "\n";
    }
    if (strTest2 == nullptr)
    {
        std::cout << "Test2 is not a string\n";
    }

    return 0;
}