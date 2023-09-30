
using System;
using JoTPK_MonogamePort;


public class MainClass {
    // [DllImport("kernel32.dll", SetLastError = true)]
    // [return: MarshalAs(UnmanagedType.Bool)]
    // private static extern bool AllocConsole();
    //
    // private const bool ConsoleOn = true;

    public static readonly string BaseDirectory =
        "C:/Users/Andrej/OneDrive - Žilinská univerzita v Žiline/Documents/GitHub/C#/JourneyOfThePraireKing_MonoGamePort/JoTPK_MonogamePort/JoTPK_MonogamePort";

    static void Main() {
        try {
            // AllocConsole();
            Console.WriteLine("test");
            Console.WriteLine("test2");
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
