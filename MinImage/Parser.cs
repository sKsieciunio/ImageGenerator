namespace MinImage;

public class InvalidSyntaxException : Exception
{
    public InvalidSyntaxException(string message) : base(message) { }
}

public static class Parser
{
    public static List<Command> ParseCommand(string command)
    {
        List<Command> commandChain = new();
        
        string[] commands = command.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        
        if (commands.Length == 0)
            throw new InvalidSyntaxException("Invalid syntax: no command provided.");
        
        // process first command, make sure its generative type
        string[] parts = commands[0].Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        
        Command.CommandNameEnum commandName = parts[0].ToLower() switch
        {
            "generate" => Command.CommandNameEnum.Generate,
            "input" => Command.CommandNameEnum.Input,
            "generatecheckerboard" => Command.CommandNameEnum.GenerateCheckerboard,
            "generatelight" => Command.CommandNameEnum.GenerateLight,
            "generatefractal" => Command.CommandNameEnum.GenerateFractal,
            _ => throw new InvalidSyntaxException("Invalid syntax: first command must be generative type, see 'help' for more info.")
        };
        
        string[] arguments = parts.Length > 1 ? parts[1..] : Array.Empty<string>();
        commandChain.Add(new Command(commandName, arguments));
        
        // process the rest of the commands
        for (int i = 1; i < commands.Length; i++)
        {
            parts = commands[i].Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            
            commandName = parts[0].ToLower() switch
            {
                "blur" => Command.CommandNameEnum.Blur,
                "randomcircles" => Command.CommandNameEnum.RandomCircles,
                "room" => Command.CommandNameEnum.Room,
                "colorcorrection" => Command.CommandNameEnum.ColorCorrection,
                "gammacorrection" => Command.CommandNameEnum.GammaCorrection,
                "output" => Command.CommandNameEnum.Output,
                "invert" => Command.CommandNameEnum.Invert,
                "fancyshader" => Command.CommandNameEnum.FancyShader,
                "sepia" => Command.CommandNameEnum.Sepia,
                "generate" or "input" or "generatecheckerboard" or "generatelight" or "generatefractal"
                    => throw new InvalidSyntaxException("Invalid syntax: generative type command must be the first command."),
                _ => throw new InvalidSyntaxException("Invalid syntax: unknown command.")
            };
            
            // arguments = parts[1..];
            arguments = parts.Length > 1 ? parts[1..] : Array.Empty<string>();
            commandChain.Add(new Command(commandName, arguments));
        }

        return commandChain;
    }
}