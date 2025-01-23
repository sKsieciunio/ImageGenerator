using System.Globalization;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MinImage;
    
internal class Program
{
    static void Main(string[] args)
    {
        // var cmd1 = Parser.ParseCommand("generate 3 1024 1024"); 
        // var cmd2 = Parser.ParseCommand("generate 3 1024 1024 | blur 100 100 | output executor | blur 100 100 | output ");
        // var cmd3 = Parser.ParseCommand("");
        // var cmd4 = Parser.ParseCommand("generate 3 1024 1024 | input aaaa");
        // var cmd5 = Parser.ParseCommand("blur 100 100 | output executor");
       
        if (args.Length > 0)
        {
            Console.WriteLine("Invalid usage: MinImage does not accept command line arguments.");
            return;
        }
        
        // required for proper float parsing
        Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
        
        TerminalSession terminal = new();
        terminal.Run();
        
        // List<Command> commandChain = new List<Command> {
        //     new Command(Command.CommandNameEnum.Generate, new string[] { "3", "1024", "1024" }),
        //     new Command(Command.CommandNameEnum.Blur, new string[] { "100", "100" }),
        //     new Command(Command.CommandNameEnum.Output, new string[] { "executor" }),
        // };
        //
        // Executor.ExecuteCommandChain(commandChain);

    }
}