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
        private static string PlayerName = "<Unset>";

        private static List<IValue> ChoiceBuffer = new List<IValue>();
        private static readonly IReadOnlyScope CustomScope = CreateScope();

        #region Methods
        public static void Main(string[] args)
        {
            var assembler = new VirtualMachineLispAssembler();
            var sw1 = Stopwatch.StartNew();
            var code = assembler.ParseFromText(File.ReadAllText("../examples/testDialogue.lisp"));
            sw1.Stop();
            Console.WriteLine($"Time to parse: {sw1.ElapsedMilliseconds}ms");

            var vm = new VirtualMachine(64);
            vm.AddBuiltinScope(CustomScope);
            vm.SetGlobalCode(code);

            try
            {
                vm.Running = true;
                while (vm.Running && !vm.Paused)
                {
                    vm.Step();
                }
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

            result.Define("say", vm =>
            {
                Say(vm.PopStack());
            });

            result.Define("getPlayerName", vm =>
            {
                PlayerName = Console.ReadLine()?.Trim() ?? "<Empty>";
                // PlayerName = "Alan";
            });

            result.Define("randomSay", vm =>
            {
                RandomSay(vm.PopStack<ArrayValue>());
            });

            result.Define("isShopEnabled", vm =>
            {
                vm.PushStack((BoolValue)IsShopEnabled);
            });

            result.Define("choice", vm =>
            {
                var choiceJumpLabel = vm.PopStack();
                var choiceText = vm.PopStack();
                ChoiceBuffer.Add(choiceJumpLabel);
                SayChoice(choiceText);
            });

            result.Define("waitForChoice", vm =>
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

            result.Define("openTheShop", vm =>
            {
                IsShopEnabled = true;
            });

            result.Define("openShop", vm =>
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
            vm.Jump(choice);
            return true;
        }

        private static void Say(IValue input)
        {
            var text = input.ToString();
            text = text.Replace("{playerName}", PlayerName);
            Console.WriteLine($"Say: {text}");
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