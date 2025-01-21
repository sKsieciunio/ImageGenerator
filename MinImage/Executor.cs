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
        
        NativeLibrary.TryReportCallback callback = (value) =>
        {
            // Console.WriteLine($"Callback invoked with value: {value:F2}");
            ShowProgressBar(value);
            return true;
        };
        
        foreach (Command command in commandChain)
        {
            switch (command.CommandName)
            {
                case Command.CommandNameEnum.Generate:
                    NativeLibrary.GenerateImage(image, callback);
                    break;
                case Command.CommandNameEnum.Blur:
                    int w = int.Parse(command.Arguments[0]);
                    int h = int.Parse(command.Arguments[1]);
                    NativeLibrary.Blur(image, w, h, callback);
                    break;
                case Command.CommandNameEnum.Output:
                    image.Save("images/" + command.Arguments[0] + ".png");
                    break;
                default:
                    throw new Exception("Unknown command");
            }
        }
    }
    
    private void ShowProgressBar(float value)
    {
        int totalBlocks = 50;
        int filledBlocks = (int)(value * totalBlocks);

        Console.CursorLeft = 0;
        Console.Write("[");
        Console.Write(new string('#', filledBlocks));
        Console.Write(new string('-', totalBlocks - filledBlocks));
        Console.Write($"] {value*100:F0}%");
        
        if (value >= 1.0f)
        {
            Console.WriteLine();
        }
    }
}