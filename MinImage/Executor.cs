using System.Text;

namespace MinImage;

public static class Executor
{
    private static object _locker = new object();

    struct TaskProgress
    {
        public int TotalCommands;
        public int CurrentCommand;
        public float Progress;
        public bool IsDone => CurrentCommand == TotalCommands;
    }

    private static TaskProgress[] _taskProgress = new TaskProgress[0];
    private static int _imageCount;
        
    public static void ExecuteCommandChain(List<Command> commandChain)
    {
        int textureWidth;
        int textureHeight;
        
        if (commandChain[0].CommandName == Command.CommandNameEnum.Generate)
        {
            textureWidth = int.Parse(commandChain[0].Arguments[1]);
            textureHeight = int.Parse(commandChain[0].Arguments[2]);
            _imageCount = int.Parse(commandChain[0].Arguments[0]);
        }
        else
        {
            throw new Exception("First command must be 'generate'");
        }
        
        MyImage[] images = new MyImage[_imageCount];
        for (int i = 0; i < _imageCount; i++)
        {
            images[i] = new MyImage(textureWidth, textureHeight);
        }

        _taskProgress = new TaskProgress[_imageCount];
        for (int i = 0; i < _imageCount; i++)
        {
            _taskProgress[i] = new TaskProgress
            {
                TotalCommands = commandChain.Count,
                CurrentCommand = 0,
                Progress = 0.0f
            };
        }

        Parallel.For(0, _imageCount, (taskId) =>
        {
            NativeLibrary.TryReportCallback callback = CreateCallback(taskId);
            
            foreach (Command command in commandChain)
            {
                switch (command.CommandName)
                {
                    case Command.CommandNameEnum.Generate:
                        NativeLibrary.GenerateImage(images[taskId], callback);
                        break;
                    case Command.CommandNameEnum.Blur:
                        int w = int.Parse(command.Arguments[0]);
                        int h = int.Parse(command.Arguments[1]);
                        NativeLibrary.Blur(images[taskId], w, h, callback);
                        break;
                    case Command.CommandNameEnum.Output:
                        images[taskId].Save("images/" + command.Arguments[0] + $"{taskId+1}.png", callback);
                        break;
                    default:
                        throw new Exception("Unknown command");
                }

                _taskProgress[taskId].CurrentCommand++;
            }
        });

        var cursorPosition = Console.GetCursorPosition();
        Console.SetCursorPosition(0, cursorPosition.Top + _imageCount);
    }
    
    private static NativeLibrary.TryReportCallback CreateCallback(int taskId)
    {
        return (value) =>
        {
            _taskProgress[taskId].Progress = value;

            lock (_locker)
                RenderProgress();
            return true;
        };
    }

    private static void RenderProgress()
    {   
        var cursorPosition = Console.GetCursorPosition();
        
        for (int i = 0; i < _imageCount; i++)
        {
            if (_taskProgress[i].IsDone)
                continue; 
            Console.SetCursorPosition(0, cursorPosition.Top + i);
            Console.Write(GetProgressBarString(_taskProgress[i]));
        }
        
        Console.SetCursorPosition(cursorPosition.Left, cursorPosition.Top);
    }

    private static string GetProgressBarString(TaskProgress taskProgress)
    {
        int blockPerCommand = 5;
        
        var progressBar = new StringBuilder();
        progressBar.Append("[");
        
        for (int i = 0; i < taskProgress.CurrentCommand; i++)
        {
            progressBar.Append(new string('#', blockPerCommand));
            progressBar.Append("|");
        }
        
        progressBar.Append(new string('#', (int)(taskProgress.Progress * blockPerCommand)));
        progressBar.Append(new string('-', blockPerCommand - (int)(taskProgress.Progress * blockPerCommand)));
        
        for (int i = 0; i < taskProgress.TotalCommands - taskProgress.CurrentCommand - 1; i++)
        {
            progressBar.Append("|");
            progressBar.Append(new string('-', blockPerCommand));
        }
        
        float overallProgress = (taskProgress.CurrentCommand + taskProgress.Progress) / taskProgress.TotalCommands;
        progressBar.Append($"] {overallProgress*100:F0}%");
        
        return progressBar.ToString();
    }
}