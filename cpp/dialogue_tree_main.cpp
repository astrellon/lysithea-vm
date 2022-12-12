#include <iostream>

#include <random>
#include <fstream>

#include "src/values/values.hpp"
#include "src/virtual_machine.hpp"
#include "src/assembler/assembler.hpp"
#include "src/assembler/parser.hpp"
#include "src/standard_library/standard_array_library.hpp"

using namespace lysithea_vm;

std::random_device _rd;
std::mt19937 _rand(_rd());

bool is_shop_enabled = false;
std::vector<value> choice_buffer;

void say(const value &input)
{
    std::cout << "Say: " << input.to_string() << "\n";
}

void random_say(const array_value &input)
{
    const auto &array = input.data;
    std::uniform_real_distribution<float> dist(0.0f, static_cast<float>(array.size()));
    auto rand_index = static_cast<int>(dist(_rand));
    say(array[rand_index]);
}

void say_choice(const value &input)
{
    std::cout << "- " << choice_buffer.size() << ": " << input.to_string() << "\n";
}

bool do_choice(int index, virtual_machine &vm)
{
    if (index < 1 || index > choice_buffer.size())
    {
        return false;
    }

    index--;

    auto choice = choice_buffer[index];
    choice_buffer.clear();
    vm.call_function(*choice.get_complex(), 0, false);
    return true;
}

std::shared_ptr<scope> create_dialogue_scope()
{
    auto result = std::make_shared<scope>();

    result->define("say", [](virtual_machine &vm, const array_value &args) -> void
    {
        say(args.get_index(0));
    });

    result->define("getPlayerName", [](virtual_machine &vm, const array_value &args) -> void
    {
        std::string player_name;
        std::cin >> player_name;
        vm.global_scope->define("playerName", value(player_name));
    });

    result->define("randomSay", [](virtual_machine &vm, const array_value &args) -> void
    {
        random_say(*args.get_index<const array_value>(0));
    });

    result->define("isShopEnabled", [](virtual_machine &vm, const array_value &args) -> void
    {
        vm.push_stack(is_shop_enabled);
    });

    result->define("moveTo", [](virtual_machine &vm, const array_value &args) -> void
    {
        auto proc = args.get_index(0).get_complex();
        auto label = args.get_index(1);

        vm.call_function(*proc, 0, false);
        vm.jump(label.to_string());
    });

    result->define("choice", [](virtual_machine &vm, const array_value &args) -> void
    {
        auto choice_text = args.get_index(0);
        auto choice_jump = args.get_index(1);
        choice_buffer.push_back(choice_jump);
        say_choice(choice_text);
    });

    result->define("waitForChoice", [](virtual_machine &vm, const array_value &args) -> void
    {
        if (choice_buffer.size() == 0)
        {
            throw std::runtime_error("No choices to wait for!");
        }

        auto choice_valid = false;
        do
        {
            std::cout << "Enter choice: ";
            std::string choice_text;
            std::cin >> choice_text;

            auto choice_index = std::stoi(choice_text);
            if (do_choice(choice_index, vm))
            {
                choice_valid = true;
            }
            else
            {
                std::cout << "Invalid choice\n";
            }
        } while (!choice_valid);
    });

    result->define("openTheShop", [](virtual_machine &vm, const array_value &args) -> void
    {
        is_shop_enabled = true;
    });

    result->define("openShop", [](virtual_machine &vm, const array_value &args) -> void
    {
        std::cout << "Opening the shop to the player and quitting dialogue\n";
    });

    return result;
}

int main()
{
    const char *filename = "../../examples/testDialogue.lys";

    std::ifstream input_file;
    input_file.open(filename);
    if (!input_file)
    {
        std::cout << "Could not find file to open!\n";
        return -1;
    }

    auto custom_scope = create_dialogue_scope();

    lysithea_vm::assembler assembler;
    assembler.builtin_scope.combine_scope(*custom_scope);
    assembler.builtin_scope.combine_scope(*standard_array_library::library_scope);

    auto script = assembler.parse_from_stream(filename, input_file);

    lysithea_vm::virtual_machine vm(16);
    vm.execute(script);

    return 0;
}