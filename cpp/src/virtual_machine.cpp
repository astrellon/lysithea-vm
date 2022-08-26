#include "virtual_machine.hpp"

namespace stack_vm
{
    VirtualMachine::VirtualMachine(int stackSize, RunHandler runHandler) : stackSize(stackSize), programCounter(0), running(false), paused(false), runHandler(runHandler)
    {
        stack.reserve(stackSize);
        stackTrace.reserve(stackSize);
    }

    void VirtualMachine::addScope(std::shared_ptr<Scope> scope)
    {
        scopes[scope->name] = scope;
    }

    void VirtualMachine::run(const std::string &startScopeName)
    {
        if (startScopeName.size() > 0)
        {
            auto find = scopes.find(startScopeName);
            if (find == scopes.end())
            {
                throw std::runtime_error("Unable to find start scope");
            }
            else
            {
                currentScope = find->second;
            }
        }

        running = true;
        paused = false;

        while (running && !paused)
        {
            step();
        }
    }

    void VirtualMachine::stop()
    {
        running = false;
    }

    void VirtualMachine::pause(bool value)
    {
        paused = value;
    }

    void VirtualMachine::step()
    {
        if (programCounter >= currentScope->code.size())
        {
            stop();
            return;
        }

        const auto &codeLine = currentScope->code[programCounter++];

        switch (codeLine.op)
        {
            default:
            {
                throw std::runtime_error("Unknown operator");
            }
            case Operator::Push:
            {
                if (!codeLine.value.has_value())
                {
                    throw std::runtime_error("Push code line requires input");
                }

                stack.push_back(codeLine.value.value());
                break;
            }
            case Operator::Pop:
            {
                stack.pop_back();
                break;
            }
            case Operator::Jump:
            {
                const auto &label = popStack();
                jump(label);
                break;
            }
            case Operator::JumpTrue:
            {
                const auto &label = popStack();
                const auto &top = popStack();
                if (top.is_true())
                {
                    jump(label);
                }
                break;
            }
            case Operator::JumpFalse:
            {
                const auto &label = popStack();
                const auto &top = popStack();
                if (top.is_false())
                {
                    jump(label);
                }
                break;
            }
            case Operator::Call:
            {
                const auto &label = popStack();
                call(label);
                break;
            }
            case Operator::Return:
            {
                callReturn();
                break;
            }
            case Operator::Run:
            {
                const auto &top = popStack();
                runHandler(top, *this);
                break;
            }
        }
    }

    void VirtualMachine::call(const Value &input)
    {
        stackTrace.emplace_back(programCounter, currentScope);
        jump(input);
    }

    void VirtualMachine::jump(const Value &input)
    {
        if (input.is_string())
        {
            jump(std::get<std::string>(input.data), "");
        }
        else if (input.is_array())
        {
            const auto &list = std::get<array_value>(input.data);
            if (list.size() == 0)
            {
                throw std::runtime_error("Cannot jump to empty array");
            }

            const auto &label = list[0].to_string();
            if (list.size() > 1)
            {
                jump(label, list[1].to_string());
            }
            else
            {
                jump(label, "");
            }
        }
    }

    void VirtualMachine::jump(const std::string &label, const std::string &scopeName)
    {
        if (scopeName.size() > 0)
        {
            auto find = scopes.find(scopeName);
            if (find == scopes.end())
            {
                throw std::runtime_error("Unable to find scope to jump to");
            }
            else
            {
                currentScope = find->second;
            }
        }

        if (label.size() == 0)
        {
            programCounter = 0;
            return;
        }

        auto findLabel = currentScope->labels.find(label);
        if (findLabel == currentScope->labels.end())
        {
            throw std::runtime_error("Unable to find label in current scope to jump to");
        }

        programCounter = findLabel->second;
    }

    void VirtualMachine::callReturn()
    {
        if (stackTrace.size() == 0)
        {
            throw std::runtime_error("Unable to return, stack trace empty");
        }

        const auto last = *stackTrace.rbegin();
        stackTrace.pop_back();
        currentScope = last.scope;
        programCounter = last.lineCounter;
    }

    Value VirtualMachine::popStack()
    {
        auto end = *stack.rbegin();
        stack.erase(stack.end());
        return end;
    }

    void VirtualMachine::pushStack(Value input)
    {
        stack.push_back(input);
    }
} // namespace stack_vm
