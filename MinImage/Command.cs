﻿namespace MinImage;

public class Command
{
    public enum CommandNameEnum
    {
        Input,
        Generate,
        // 3 additional generating commands
        
        Blur,
        RandomCircles,
        Room,
        ColorCorrection,
        GammaCorrection,
        // 3 additional processing commands
        
        Output
    }
    
    public CommandNameEnum CommandName { get; set; }
    public string[] Arguments { get; set; }
    
    public Command(CommandNameEnum commandName, string[] arguments)
    {
        CommandName = commandName;
        Arguments = arguments;
    }
}