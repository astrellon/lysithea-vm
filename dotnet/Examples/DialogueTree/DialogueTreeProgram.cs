using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimpleStackVM
{
    public static class DialogueTreeProgram
    {
        private static Random Rand = new Random();
        private static bool IsShopEnabled = false;

        private static List<IFunctionValue> ChoiceBuffer = new List<IFunctionValue>();
        private static readonly IReadOnlyScope CustomScope = CreateScope();

        #region Methods
        public static void Main(string[] args)
        {
            var assembler = new VirtualMachineAssembler();
            assembler.BuiltinScope.CombineScope(CustomScope);
            assembler.BuiltinScope.CombineScope(StandardOperators.Scope);
            assembler.BuiltinScope.CombineScope(StandardArrayLibrary.Scope);

            var script = assembler.ParseFromText(File.ReadAllText("../../../examples/testDialogue.lisp"));

            var vm = new VirtualMachine(8);

            try
            {
                vm.Execute(script);
            }
            catch (VirtualMachineException exp)
            {
                Console.WriteLine(exp.Message);
                var stackTrace = string.Join("", exp.VirtualMachineStackTrace.Select(t => $"\n- {t}"));
                Console.WriteLine($"VM Stack: {stackTrace}");
                Console.WriteLine(exp.StackTrace);
            }
        }

        private static Scope CreateScope()
        {
            var result = new Scope();

            result.Define("say", (vm, numArgs) =>
            {
                Say(vm.PopStack());
            });

            result.Define("getPlayerName", (vm, numArgs) =>
            {
                var name = Console.ReadLine()?.Trim() ?? "<Empty>";
                vm.GlobalScope.Define("playerName", new StringValue(name));
            });

            result.Define("randomSay", (vm, numArgs) =>
            {
                RandomSay(vm.PopStack<ArrayValue>());
            });

            result.Define("isShopEnabled", (vm, numArgs) =>
            {
                vm.PushStack(IsShopEnabled);
            });

            result.Define("moveTo", (vm, numArgs) =>
            {
                var label = vm.PopStack();
                var proc = vm.PopStack<FunctionValue>();

                vm.CallFunction(proc, 0, false);
                vm.Jump(label.ToString());
            });

            result.Define("choice", (vm, numArgs) =>
            {
                var choiceJumpLabel = vm.PopStack();
                var choiceText = vm.PopStack();
                if (choiceJumpLabel is IFunctionValue procValue)
                {
                    ChoiceBuffer.Add(procValue);
                }
                else
                {
                    throw new Exception("Choice call must be a function!");
                }
                SayChoice(choiceText);
            });

            result.Define("waitForChoice", (vm, numArgs) =>
            {
                if (!ChoiceBuffer.Any())
                {
                    throw new Exception("No choices to wait for!");
                }

                var choiceValid = false;
                do
                {
                    Console.WriteLine("Enter choice: ");
                    var choiceText = Console.ReadLine()?.Trim();
                    if (int.TryParse(choiceText ?? "-1", out var choiceIndex))
                    {
                        if (DoChoice(choiceIndex, vm))
                        {
                            choiceValid = true;
                        }
                        else
                        {
                            Console.WriteLine("Invalid choice");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Must be a number");
                    }

                } while (!choiceValid);
            });

            result.Define("openTheShop", (vm, numArgs) =>
            {
                IsShopEnabled = true;
            });

            result.Define("openShop", (vm, numArgs) =>
            {
                Console.WriteLine("Opening the shop to the player and quitting dialogue");
            });

            return result;
        }

        private static bool DoChoice(int index, VirtualMachine vm)
        {
            if (index < 1 || index > ChoiceBuffer.Count)
            {
                return false;
            }

            index--;

            var choice = ChoiceBuffer[index];
            ChoiceBuffer.Clear();
            vm.CallFunction(choice, 0, false);
            return true;
        }

        private static void Say(IValue input)
        {
            Console.WriteLine($"Say: {input.ToString()}");
        }

        private static void RandomSay(ArrayValue input)
        {
            var randIndex = Rand.Next(0, input.Value.Count);
            Say(input.Value[randIndex]);
        }

        private static void SayChoice(IValue input)
        {
            Console.WriteLine($"- {ChoiceBuffer.Count}: {input.ToString()}");
        }
        #endregion
    }
}