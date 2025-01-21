namespace MinImage;

public class Terminal
{
    // private List<Command> _commands;
    // private bool _isRunning;
    //
    // public Terminal()
    // {
    //     _commands = new List<Command>();
    //     _isRunning = false;
    // }
    //
    // public void Start()
    // {
    //     _isRunning = true;
    //     RunSession();
    // }
    //
    // private void RunSession()
    // {
    //     Console.WriteLine("MinImage Terminal");
    //     Console.WriteLine("Type 'exit' to close the terminal");
    //     Console.WriteLine("Type 'help' to see the available commands");
    //         
    //     while (_isRunning)
    //     {
    //         Console.Write(">> ");
    //         string input = Console.ReadLine() ?? "";
    //         input = input.Trim();
    //         
    //         if (input == "exit")
    //         {
    //             _isRunning = false;
    //             break;
    //         }
    //
    //         if (input == "help")
    //         {
    //             WriteHelp();
    //             continue;
    //         }
    //
    //         Command cmd = Parser.ParseCommand(input);
    //         _commands.Add(cmd);
    //     } 
    // }
    //
    // private void WriteHelp()
    // {
    //     throw new NotImplementedException();
    // }
}