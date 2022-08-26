#pragma once

#include <functional>
#include <vector>
#include <map>
#include <string>
#include <memory>

#include "operator.hpp"
#include "code_line.hpp"
#include "scope.hpp"
#include "value.hpp"

namespace stack_vm
{
    class VirtualMachine;

    using RunHandler = std::function<void(const Value &, VirtualMachine &)>;

    class ScopeFrame
    {
        public:
            // Fields
            const int lineCounter;
            const std::shared_ptr<Scope> scope;

            // Constructor
            ScopeFrame(int lineCounter, std::shared_ptr<Scope> scope) : lineCounter(lineCounter), scope(scope) { }

            // Methods
    };

    class VirtualMachine
    {
        public:
            // Fields

            // Constructor
            VirtualMachine(int stackSize, RunHandler runHandler);

            // Methods
            void addScope(std::shared_ptr<Scope> scope);
            void run(const std::string &startScopeName);
            void stop();
            void pause(bool value);
            void step();

            void call(const Value &label);
            void jump(const Value &label);
            void jump(const std::string &label, const std::string &scopeName);
            void callReturn();

            Value popStack();
            void pushStack(Value input);

        private:
            // Fields
            std::vector<Value> stack;
            std::map<std::string, std::shared_ptr<Scope>> scopes;
            std::vector<ScopeFrame> stackTrace;
            std::shared_ptr<Scope> currentScope;
            int programCounter;
            int stackSize;
            bool running;
            bool paused;
            RunHandler runHandler;

            // Methods
    };
} // stack_vm