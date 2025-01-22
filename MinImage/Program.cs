using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MinImage;
    
internal class Program
{
    static void Main()
    {
        // var cmd1 = Parser.ParseCommand("generate 3 1024 1024"); 
        // var cmd2 = Parser.ParseCommand("generate 3 1024 1024 | blur 100 100 | output executor | blur 100 100 | output ");
        // var cmd3 = Parser.ParseCommand("");
        // var cmd4 = Parser.ParseCommand("generate 3 1024 1024 | input aaaa");
        // var cmd5 = Parser.ParseCommand("blur 100 100 | output executor");
        
        
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