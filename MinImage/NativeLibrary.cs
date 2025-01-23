using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MinImage;

public static partial class NativeLibrary
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MyColor
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;
    }
    
    // struct for circle 3 floats
    [StructLayout(LayoutKind.Sequential)]
    struct Circle
    {
        public float x;
        public float y;
        public float radius;
    }
    

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool TryReportCallback(float value);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate MyColor ModifyColor(float normalizedX, float normalizedY, MyColor oldColor);
    
    [LibraryImport("ImageGenerator", EntryPoint = "GenerateImage")]
    private static partial void GenerateImage(
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] MyColor[] texture,
        int textureWidth,
        int textureHeight,
        TryReportCallback tryReportCallback
    );
    
    internal static void GenerateImage(MyImage image, TryReportCallback tryReportCallback)
    {
        GenerateImage(image.Texture, image.TextureWidth, image.TextureHeight, tryReportCallback);
    }
    
    [LibraryImport("ImageGenerator", EntryPoint = "Blur")]
    private static partial void Blur(
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] MyColor[] texture,
        int textureWidth,
        int textureHeight,
        int w,
        int h,
        TryReportCallback tryReportCallback
    );
    
    internal static void Blur(MyImage image, int w, int h, TryReportCallback tryReportCallback)
    {
        Blur(image.Texture, image.TextureWidth, image.TextureHeight, w, h, tryReportCallback);
    }

    [LibraryImport("ImageGenerator", EntryPoint = "ColorCorrection")]
    private static partial void ColorCorrection(
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] MyColor[] texture,
        int textureWidth,
        int textureHeight,
        float red,
        float green,
        float blue,
        TryReportCallback tryReportCallback
    );
    
    internal static void ColorCorrection(MyImage image, float red, float green, float blue, TryReportCallback tryReportCallback)
    {
        ColorCorrection(image.Texture, image.TextureWidth, image.TextureHeight, red, green, blue, tryReportCallback);
    }
    
    [LibraryImport("ImageGenerator", EntryPoint = "GammaCorrection")]
    private static partial void GammaCorrection(
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] MyColor[] texture,
        int textureWidth,
        int textureHeight,
        float gamma,
        TryReportCallback tryReportCallback
    );
    
    internal static void GammaCorrection(MyImage image, float gamma, TryReportCallback tryReportCallback)
    {
        GammaCorrection(image.Texture, image.TextureWidth, image.TextureHeight, gamma, tryReportCallback);
    }
    
    [LibraryImport("ImageGenerator", EntryPoint = "DrawCircles")]
    private static partial void RandomCircles(
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] MyColor[] texture,
        int textureWidth,
        int textureHeight,
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex =  1)] Circle[] circles,
        int circleCount,
        TryReportCallback tryReportCallback
    );
    
    internal static void RandomCircles(MyImage image, int circleCount, int circleRadius, TryReportCallback tryReportCallback)
    {
        Circle[] circles = new Circle[circleCount];
        Random random = new Random();
        for (int i = 0; i < circleCount; i++)
        {
            circles[i] = new Circle
            {
                x = (float)random.NextDouble(),
                y = (float)random.NextDouble() ,
                radius = (float)circleRadius / image.TextureWidth,
            };
        }
        
        RandomCircles(image.Texture, image.TextureWidth, image.TextureHeight, circles, circles.Length, tryReportCallback);
    }
    
    [LibraryImport("ImageGenerator", EntryPoint = "ProcessPixels_Custom")]
    private static partial void ProcessPixels_Custom(
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] MyColor[] texture,
        int textureWidth,
        int textureHeight,
        ModifyColor modifyColor,
        TryReportCallback tryReportCallback
    );

    internal static void Room(MyImage image, float x1, float y1, float x2, float y2,
        TryReportCallback tryReportCallback)
    {
        ModifyColor modifyColor = (normalizedX, normalizedY, oldColor) =>
        {
            if (normalizedX >= x1 && normalizedX <= x2 && normalizedY >= y1 && normalizedY <= y2)
            {
                float cx = (x1 + x2) / 2;
                float cy = (y1 + y2) / 2;

                float maxDist = Math.Max((x2 - x1) / 2, (y2 - y1) / 2); 
                
                float dist = Math.Max(Math.Abs(normalizedX - cx), Math.Abs(normalizedY - cy));

                byte value = (byte)(255 * (dist / maxDist));
                
                return new MyColor { R = 0, G = 0, B = 0, A = Math.Max((byte)0, Math.Min((byte)255, value)) };
            }
            return oldColor;
        };
        
        ProcessPixels_Custom(image.Texture, image.TextureWidth, image.TextureHeight, modifyColor, tryReportCallback); 
    }
}