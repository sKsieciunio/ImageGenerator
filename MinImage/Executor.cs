namespace MinImage;

public class Executor
{
    public void ExecuteCommandChain(List<Command> commandChain)
    {
        MyImage image;
        
        if (commandChain[0].CommandName == Command.CommandNameEnum.Generate)
        {
            int textureWidth = int.Parse(commandChain[0].Arguments[1]);
            int textureHeight = int.Parse(commandChain[0].Arguments[2]);
            image = new MyImage(textureWidth, textureHeight);
        }
        else
        {
            throw new Exception("First command must be 'generate'");
        }

        int totalCommands = commandChain.Count;
        int currentCommand = 0;
        
        foreach (Command command in commandChain)
        {
            switch (command.CommandName)
            {
                case Command.CommandNameEnum.Generate:
                    NativeLibrary.GenerateImage(image, CreateCallback(totalCommands, currentCommand));
                    break;
                case Command.CommandNameEnum.Blur:
                    int w = int.Parse(command.Arguments[0]);
                    int h = int.Parse(command.Arguments[1]);
                    NativeLibrary.Blur(image, w, h, CreateCallback(totalCommands, currentCommand));
                    break;
                case Command.CommandNameEnum.Output:
                    image.Save("images/" + command.Arguments[0] + ".png", CreateCallback(totalCommands, currentCommand));
                    break;
                default:
                    throw new Exception("Unknown command");
            }
            
            currentCommand++;
        }
    }
    
    private NativeLibrary.TryReportCallback CreateCallback(int totalCommands, int currentCommand)
    {
        return (value) =>
        {
            ShowProgressBar(value, totalCommands, currentCommand);
            return true;
        };
    }
    
    private void ShowProgressBar(float progress, int totalCommands, int currentCommand)
    {
        // [#####|#####|##---|-----] 60%
        int blockPerCommand = 5;

        Console.CursorLeft = 0;
        Console.Write("[");

        for (int i = 0; i < currentCommand; i++)
        {
            Console.Write(new string('#', blockPerCommand));
            Console.Write("|");
        }
        
        Console.Write(new string('#', (int)(progress * blockPerCommand)));
        Console.Write(new string('-', blockPerCommand - (int)(progress * blockPerCommand)));
        
        for (int i = 0; i < totalCommands - currentCommand - 1; i++)
        {
            Console.Write("|");
            Console.Write(new string('-', blockPerCommand));
        }
       
        float overallProgress = (currentCommand + progress) / totalCommands;
        Console.Write($"] {overallProgress*100:F0}%");
        
        if (progress >= 1.0f && currentCommand == totalCommands - 1)
        {
            Console.WriteLine();
        }

        // int totalBlocks = 50;
        // int filledBlocks = (int)(progress * totalBlocks);
        //
        // Console.CursorLeft = 0;
        // Console.Write("[");
        // Console.Write(new string('#', filledBlocks));
        // Console.Write(new string('-', totalBlocks - filledBlocks));
        // Console.Write($"] {progress*100:F0}%");
        //
        // if (progress >= 1.0f)
        // {
        //     Console.WriteLine();
        // }
    }
}