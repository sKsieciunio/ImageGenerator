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

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate bool TryReportCallback(float value);
    
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
}