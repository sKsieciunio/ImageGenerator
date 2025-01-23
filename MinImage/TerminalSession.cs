namespace MinImage;

public class TerminalSession
{
    public void Run()
    {
        Console.WriteLine("Welcome to MinImage!");
        Console.WriteLine("Type 'help' for a list of commands. Type 'exit' to quit.");
        
        while (true)
        {
            Console.Write(">> ");
            string input = Console.ReadLine() ?? "";
            input = input.Trim();
            
            if (input == "help")
            {
                PrintHelp();
            }
            else if (input == "exit")
            {
                Console.WriteLine("Exiting...");
                break;
            }
            else
            {
                try
                {
                    List<Command> commandChain = Parser.ParseCommand(input);
                    Executor.ExecuteCommandChain(commandChain);
                }
                catch (IndexOutOfRangeException)
                {
                    Console.WriteLine("Invalid syntax: propably wrong amount of arguments.");
                }
                catch (InvalidSyntaxException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (AggregateException e)
                {
                    foreach (var innerEx in e.InnerExceptions)
                        if (innerEx is IndexOutOfRangeException)
                            Console.WriteLine("Invalid syntax: propably wrong amount of arguments.");
                        else
                            Console.WriteLine("An error occurred: " + innerEx.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                }
            }
        }
    }

    private void PrintHelp()
    {
        // TODO: Complete the list of available commands
        Console.WriteLine("Available commands:");
        Console.WriteLine("  generate <count> <width> <height>");
        Console.WriteLine("  blur <radius> <passes>");
        Console.WriteLine("  output <filename>"); 
    }
}