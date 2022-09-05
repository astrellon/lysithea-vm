using System;
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

        #region Methods
        public static void Run()
        {
#if RELEASE
            var json = SimpleJSON.JSON.Parse(File.ReadAllText("../../examples/testDialogue.json"));
#else
            var json = SimpleJSON.JSON.Parse(File.ReadAllText("../examples/testDialogue.json"));
#endif
            var scopes = VirtualMachineAssembler.ParseScopes(json.AsArray);

            var vm = new VirtualMachine(64, OnRunCommand);
            vm.AddScopes(scopes);

            try
            {
                vm.SetCurrentScope("Main");
                vm.SetRunning(true);
                while (vm.IsRunning && !vm.IsPaused)
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

        private static void OnRunCommand(IValue command, VirtualMachine vm)
        {
            var commandName = command.ToString();
            if (commandName == "say")
            {
                Say(vm.PopStack());
            }
            else if (commandName == "getPlayerName")
            {
                PlayerName = Console.ReadLine()?.Trim() ?? "<Empty>";
            }
            else if (commandName == "randomSay")
            {
                RandomSay(vm.PopStack<ArrayValue>());
            }
            else if (commandName == "isShopEnabled")
            {
                vm.PushStack((BoolValue)IsShopEnabled);
            }
            else if (commandName == "choice")
            {
                var choiceJumpLabel = vm.PopStack();
                var choiceText = vm.PopStack();
                ChoiceBuffer.Add(choiceJumpLabel);
                SayChoice(choiceText);
            }
            else if (commandName == "waitForChoice")
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
            }
            else if (commandName == "openTheShop")
            {
                IsShopEnabled = true;
            }
            else if (commandName == "openShop")
            {
                Console.WriteLine("Opening the shop to the player and quitting dialogue");
            }
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
            vm.JumpToLabel(choice);
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