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
        Console.WriteLine("Available commands:");
        Console.WriteLine("  Generative commands:");
        Console.WriteLine("    generate <number of images> <width> <height>");
        Console.WriteLine("    generatecheckerboard <number of images> <width> <height> <tilesX> <tilesY>");
        Console.WriteLine("        tilesX and tilesY are optional, by default they are randomized");
        Console.WriteLine("    generatelight <number of images> <width> <height>");
        Console.WriteLine("    generatefractal <number of images> <width> <height>");
        Console.WriteLine("    input <filePath>");
        Console.WriteLine("  Processing commands:");
        Console.WriteLine("    blur <w> <h>");
        Console.WriteLine("    randomcircles <count> <radius>");
        Console.WriteLine("        radius in number of pixels");
        Console.WriteLine("    room <x1> <y1> <x2> <y2>");
        Console.WriteLine("    colorcorrection <red> <green> <blue>");
        Console.WriteLine("    gammacorrection <gamma>");
        Console.WriteLine("    invert");
        Console.WriteLine("    fancyshader");
        Console.WriteLine("    sepia");
        Console.WriteLine("  Output command:");
        Console.WriteLine("    output <filePath>");
    }
}