﻿
using System;
using JoTPK_MonogamePort;


public class MainClass {
    // [DllImport("kernel32.dll", SetLastError = true)]
    // [return: MarshalAs(UnmanagedType.Bool)]
    // private static extern bool AllocConsole();
    //
    // private const bool ConsoleOn = true;
    
    static void Main() {
        try {
            // AllocConsole();
            using var game = new Game1();
            game.Run();
        } catch (Exception e) {
            Console.WriteLine(e.ToString());
            //Console.WriteLine(e.StackTrace);
        }

        // Console.WriteLine("Press any key to exit...");
        // Console.ReadKey();
    }
}
