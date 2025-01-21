using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MinImage;
    
internal partial class Program
{
    static void Main()
    {
        // int textureWidth = 256; 
        // int textureHeight = 256;
        //
        // NativeLibrary.MyColor[] texture = new NativeLibrary.MyColor[textureWidth * textureHeight];
        //
        // NativeLibrary.TryReportCallback callback = (value) =>
        // {
        //     Console.WriteLine($"Callback invoked with value: {value}");
        //     return true;
        // };
        //
        // NativeLibrary.GenerateImage(texture, textureWidth, textureHeight, callback);
        //
        // NativeLibrary.SaveImage(texture, textureWidth, textureHeight, "images/image1.png");
        //
        // NativeLibrary.Blur(texture, textureWidth, textureHeight, 50, 5, callback);
        //
        // NativeLibrary.SaveImage(texture, textureWidth, textureHeight, "images/image2.png");
        //
        // Console.WriteLine("Done!");

        // ---------------------------------------------------------
        
        List<Command> commandChain = new List<Command> {
            new Command(Command.CommandNameEnum.Generate, new string[] { "1", "2048", "2048" }),
            new Command(Command.CommandNameEnum.Output, new string[] { "executor1" }),
            new Command(Command.CommandNameEnum.Blur, new string[] { "100", "100" }),
            new Command(Command.CommandNameEnum.Output, new string[] { "executor2" }),
            new Command(Command.CommandNameEnum.Blur, new string[] { "100", "100" }),
            new Command(Command.CommandNameEnum.Output, new string[] { "executor3" }),
        };
        
        Executor executor = new();
        executor.ExecuteCommandChain(commandChain);


        // while (true)
        // {
        //     string? input = Console.ReadLine();
        //     if (input == null)
        //     {
        //         Console.WriteLine("Exiting...");
        //     }
        //     Console.WriteLine(input);
        // }

        /*
        const int width = 1024;
        const int height = 1024;

        ImSh.Image<ImSh::PixelFormats.Rgba32> image = new(width, height);


        image.DangerousTryGetSinglePixelMemory(out Memory<ImSh::PixelFormats.Rgba32> memory);
        var span = memory.Span;

        for (int i = 0; i < width; i++)
        {
            int red = (255 * i) / width;
            for (int j = 0; j < height; j++)
            {
                int blue = (255 * j) / height;
                span[i * width + j].R = (byte)red;
                span[i * width + j].G = (byte)((red * blue) / 255);
                span[i * width + j].B = (byte)blue;
                span[i * width + j].A = 255;
            }
        }

        ImSh.Formats.Jpeg.JpegEncoder encoder = new();
        FileStream fs = new($"./Image_0.jpeg", FileMode.OpenOrCreate, FileAccess.Write);
        encoder.Encode(image, fs);
        image.Dispose();
        */
    }
}