#include <iostream>

#include <random>
#include <fstream>
#include <regex>

#include "src/virtual_machine.hpp"
#include "src/assembler.hpp"

using namespace stack_vm;

std::random_device _rd;
std::mt19937 _rand(_rd());

int counter = 0;
bool is_shop_enabled = false;
std::string player_name = "<Unset>";
std::vector<value> choice_buffer;

void say(const value &input)
{
    auto text = std::regex_replace(input.to_string(), std::regex("\\{playerName\\}"), player_name);
    std::cout << "Say: " << text << "\n";
}

void random_say(const value &input)
{
    const auto &array = *std::get<std::shared_ptr<array_value>>(input.data).get();
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
    vm.jump(choice);
    return true;
}

void runHandler(const value &input, virtual_machine &vm)
{
    if (!input.is_string())
    {
        return;
    }

    const auto &command = *std::get<std::shared_ptr<std::string>>(input.data).get();

    if (command == "say")
    {
        say(vm.pop_stack());
    }
    else if (command == "getPlayerName")
    {
        std::cin >> player_name;
    }
    else if (command == "randomSay")
    {
        random_say(vm.pop_stack());
    }
    else if (command == "isShopEnabled")
    {
        vm.push_stack(value(is_shop_enabled));
    }
    else if (command == "choice")
    {
        choice_buffer.push_back(vm.pop_stack());
        say_choice(vm.pop_stack());
    }
    else if (command == "waitForChoice")
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
    }
    else if (command == "openTheShop")
    {
        is_shop_enabled = true;
    }
    else if (command == "openShop")
    {
        std::cout << "Opening the shop to the player and quitting dialogue\n";
    }
}

int main()
{
    std::ifstream json_input;
    json_input.open("../../examples/testDialogue.json");
    if (!json_input)
    {
        std::cout << "Could not find file to open!\n";
        return -1;
    }

    nlohmann::json json;
    json_input >> json;

    auto parsed_scopes = assembler::parse_scopes(json);

    virtual_machine vm(64, runHandler);
    vm.add_scopes(parsed_scopes);

    vm.run("Main");

    return 0;
}