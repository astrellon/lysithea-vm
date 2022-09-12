﻿using System;
using System.Linq;

namespace SimpleStackVM
{
    public static class Program
    {
        #region Methods
        public static void Main(string[] args)
        {
            Console.WriteLine("Choose program to run: ");
            Console.WriteLine("1: Perf Test");
            Console.WriteLine("2: Dialogue Tree");

            var input = Console.ReadLine()?.Trim();
            if (input == "1")
            {
                PerfTestProgram.Run();
            }
            else if (input == "2")
            {
                DialogueTreeProgram.Run();
            }
            else
            {
                Console.WriteLine("Invalid option");
            }
        }
        #endregion
    }
}