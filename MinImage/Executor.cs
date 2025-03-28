﻿using System.Text;

namespace MinImage;

public static class Executor
{
    private static object _locker = new object();
    private static CancellationTokenSource _cts;

    struct TaskProgress
    {
        public int TotalCommands;
        public int CurrentCommand;
        public float Progress;
        public bool IsDone => CurrentCommand >= TotalCommands;
    }

    private static TaskProgress[] _taskProgress = new TaskProgress[0];
    private static int _imageCount;
    private static bool _tooManyImages;
        
    public static void ExecuteCommandChain(List<Command> commandChain)
    {
        _cts = new CancellationTokenSource();
        
        int textureWidth = 0;
        int textureHeight = 0;

        switch (commandChain[0].CommandName)
        {
            case Command.CommandNameEnum.Generate:
            case Command.CommandNameEnum.GenerateCheckerboard:
            case Command.CommandNameEnum.GenerateLight:
            case Command.CommandNameEnum.GenerateFractal:
                _imageCount = int.Parse(commandChain[0].Arguments[0]);
                textureWidth = int.Parse(commandChain[0].Arguments[1]);
                textureHeight = int.Parse(commandChain[0].Arguments[2]);
                break;
            case Command.CommandNameEnum.Input:
                _imageCount = 1; 
                break;
            default:
                throw new Exception("Invalid syntax: first command must be generative type, see 'help' for more info.");
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
        
        // Ogółem to logika renderowania tych progress barów jest nieźle poroniona i już się poddałem żeby rozwiązywać
        // edge case kiedy jest ich więcej niż wysokość konsoli. Pozdrawiam czytającego :)
        _tooManyImages = false;
        if (_imageCount >= Console.BufferHeight)
        {
            Console.WriteLine("Too many images to display progress bars. Alternative progress display will be used.");
            _tooManyImages = true;
        }
        
        var cancellationToken = _cts.Token;
        Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.X)
                    {
                        _cts.Cancel();
                        break;
                    }
                }
                Thread.Sleep(100);
            }
        }, cancellationToken);
        
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
                    case Command.CommandNameEnum.GenerateCheckerboard:
                        if (command.Arguments.Length == 5)
                        {
                            int tilesX = int.Parse(command.Arguments[3]);
                            int tilesY = int.Parse(command.Arguments[4]);
                            NativeLibrary.GenerateCheckerboard(images[taskId], tilesX, tilesY, callback);
                        }
                        else 
                            NativeLibrary.GenerateCheckerboard(images[taskId], -1, -1, callback);
                        break;
                    case Command.CommandNameEnum.GenerateLight:
                        NativeLibrary.GenerateLight(images[taskId], callback);
                        break;
                    case Command.CommandNameEnum.GenerateFractal:
                        NativeLibrary.GenerateFractal(images[taskId], callback);
                        break;
                    case Command.CommandNameEnum.Input:
                        images[taskId].Load(command.Arguments[0], callback);
                        break;
                    case Command.CommandNameEnum.Blur:
                        int w = int.Parse(command.Arguments[0]);
                        int h = int.Parse(command.Arguments[1]);
                        NativeLibrary.Blur(images[taskId], w, h, callback);
                        break;
                    case Command.CommandNameEnum.ColorCorrection:
                        float red = float.Parse(command.Arguments[0]);
                        float green = float.Parse(command.Arguments[1]);
                        float blue = float.Parse(command.Arguments[2]);
                        NativeLibrary.ColorCorrection(images[taskId], red, green, blue, callback);
                        break;
                    case Command.CommandNameEnum.GammaCorrection:
                        float gamma = float.Parse(command.Arguments[0]);
                        NativeLibrary.GammaCorrection(images[taskId], gamma, callback);
                        break;
                    case Command.CommandNameEnum.RandomCircles:
                        int circleCount = int.Parse(command.Arguments[0]);
                        int circleRadius = int.Parse(command.Arguments[1]);
                        NativeLibrary.RandomCircles(images[taskId], circleCount, circleRadius, callback);
                        break;
                    case Command.CommandNameEnum.Room:
                        float x1 = float.Parse(command.Arguments[0]);
                        float y1 = float.Parse(command.Arguments[1]);
                        float x2 = float.Parse(command.Arguments[2]);
                        float y2 = float.Parse(command.Arguments[3]);
                        NativeLibrary.Room(images[taskId], x1, y1, x2, y2, callback);
                        break;
                    case Command.CommandNameEnum.Invert:
                        NativeLibrary.Invert(images[taskId], callback);
                        break;
                    case Command.CommandNameEnum.FancyShader:
                        NativeLibrary.FancyShader(images[taskId], callback);
                        break;
                    case Command.CommandNameEnum.Sepia:
                        NativeLibrary.Sepia(images[taskId], callback);
                        break;
                    case Command.CommandNameEnum.Output:
                        string savePath = "images/" + command.Arguments[0] + (_imageCount > 1 ? $"{taskId+1}.png" : ".png");
                        images[taskId].Save(savePath, callback);
                        break;
                    default:
                        throw new Exception("Unknown command");
                }

                if (!_cts.IsCancellationRequested)
                    _taskProgress[taskId].CurrentCommand++;
            }
        });

        if (!_tooManyImages)
        {
            var cursorPosition = Console.GetCursorPosition();
            Console.SetCursorPosition(0, cursorPosition.Top + _imageCount);
        }
        else
        {
            Console.SetCursorPosition(0, Console.CursorTop + 1);
        }
    }
    
    private static NativeLibrary.TryReportCallback CreateCallback(int taskId)
    {
        return (value) =>
        {
            if (_cts.IsCancellationRequested)
                return false;
            
            _taskProgress[taskId].Progress = value;

            lock (_locker)
                if (_tooManyImages)
                    RenderProgressAlternative();
                else
                    RenderProgress();

            return true;
        };
    }

    private static void RenderProgressAlternative()
    {
        int blocks = 20;
        int overallProgress = 0;
        int howManyDone = 0;
        for (int i = 0; i < _imageCount; i++)
        {
            overallProgress += _taskProgress[i].CurrentCommand;
            if (_taskProgress[i].IsDone || (_taskProgress[i].CurrentCommand == _taskProgress[i].TotalCommands - 1 && _taskProgress[i].Progress >= 1.0f))
                howManyDone++;
        }
        
        float percentage = (float)overallProgress / (_imageCount * _taskProgress[0].TotalCommands); 
        if (howManyDone == _imageCount)
            percentage = 1.0f;
       
        Console.Write("["); 
        Console.Write(new string('#', (int)(percentage * blocks)));
        Console.Write(new string('-', blocks - (int)(percentage * blocks)));
        Console.Write($"] {percentage*100:F0}% ({howManyDone}/{_imageCount})");
        Console.WriteLine();
        
        Console.SetCursorPosition(0, Console.CursorTop - 1);
    }

    private static void RenderProgress()
    {   
        var cursorPosition = Console.GetCursorPosition();
        int linesToScroll = _imageCount - (Console.BufferHeight - cursorPosition.Top) + 1;

        if (linesToScroll > 0)
        {
            for (int i = 0; i < _imageCount; i++)
                Console.WriteLine();
            cursorPosition.Top -= linesToScroll;
        }
        
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