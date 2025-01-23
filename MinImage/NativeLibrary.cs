using System.Numerics;
using System.Runtime.InteropServices;

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

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate MyColor GetColor(float normX, float normY);
    
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
    
    [LibraryImport("ImageGenerator", EntryPoint = "GenerateImage_Custom")]
    private static partial void GenerateImage_Custom(
        [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] MyColor[] texture,
        int textureWidth,
        int textureHeight,
        GetColor getColor,
        TryReportCallback tryReportCallback
    );

    internal static void GenerateCheckerboard(MyImage image, int tilesX, int tilesY, TryReportCallback tryReportCallback)
    {
        Random random = new Random();

        MyColor colorA = new MyColor
        {
            R = (byte)random.Next(256),
            G = (byte)random.Next(256),
            B = (byte)random.Next(256),
            A = 255
        };
        
        MyColor colorB = new MyColor
        {
            R = (byte)random.Next(256),
            G = (byte)random.Next(256),
            B = (byte)random.Next(256),
            A = 255
        };

        if (tilesX <= 0 || tilesY <= 0)
        {
            tilesX = random.Next(5, 15);
            tilesY = random.Next(5, 15);
        }
        
        GetColor getColor = (normX, normY) =>
        {
            int x = (int)(normX * tilesX);
            int y = (int)(normY * tilesY);
            return (x + y) % 2 == 0 ? colorA : colorB;
        };
        
        GenerateImage_Custom(image.Texture, image.TextureWidth, image.TextureHeight, getColor, tryReportCallback);    
    }

    // credits to: Danilo Guanabara
    // https://www.shadertoy.com/view/XsXXDn
    internal static void GenerateLight(MyImage image, TryReportCallback tryReportCallback)
    {
        Random random = new Random();
        float time = (float)random.NextDouble() * 3;
        float aspectRatio = (float)image.TextureWidth / image.TextureHeight;
        
        GetColor getColor = (x, y) =>
        {
            Vector2 p = new Vector2(x - 0.5f, y - 0.5f);
            p.X *= aspectRatio;

            float l = p.Length();
            Vector3 color = Vector3.Zero;
            float z = time;

            for (int i = 0; i < 3; i++)
            {
                z += 0.07f;
                Vector2 uv = new Vector2(x, y);

                if (l > 0)
                {
                    Vector2 direction = p / l;
                    float sinZ = MathF.Sin(z);
                    float sinTerm = MathF.Abs(MathF.Sin(l * 9 - z * 2));
                    uv += direction * (sinZ + 1) * sinTerm;
                }

                Vector2 modUv = Mod(uv, 1.0f);
                modUv -= new Vector2(0.5f);
                float dist = modUv.Length();

                color[i] = dist > 0 ? 0.01f / dist : 0.0f;
            }

            if (l > 0) color /= l;

            byte r = (byte)(Math.Clamp(color.X, 0, 1) * 255);
            byte g = (byte)(Math.Clamp(color.Y, 0, 1) * 255);
            byte b = (byte)(Math.Clamp(color.Z, 0, 1) * 255);
            
            return new MyColor { R = r, G = g, B = b, A = 255 };
        };
        
        GenerateImage_Custom(image.Texture, image.TextureWidth, image.TextureHeight, getColor, tryReportCallback);
    }

    private static Vector2 Mod(Vector2 v, float divisor)
    {
        return new Vector2 (
            v.X - divisor * MathF.Floor(v.X / divisor),
            v.Y - divisor * MathF.Floor(v.Y / divisor)
        );
    }

    // nie działa tak jakbym chciał ale coś tam rysuje xdd
    internal static void GenerateFractal(MyImage image, TryReportCallback tryReportCallback)
    {
        Random random = new Random();
        float seed = (float)random.NextDouble();
        float aspectRatio = (float)image.TextureWidth / image.TextureHeight;

        GetColor getColor = (x, y) =>
        {
            Vector2 uv = new Vector2(x - 0.5f, (y - 0.5f) * aspectRatio);
            Vector3 color = Vector3.Zero;

            float angle = Hash(seed) * MathF.PI * 2;
            float scale = 1.8f + Hash(seed + 1) * 0.4f;
            Vector2 offset = new Vector2(Hash(seed + 2), Hash(seed + 3)) * 0.2f - new Vector2(1.0f);

            Vector2 z = uv;
            float minDist = float.MaxValue;

            for (int i = 0; i < 32; i++)
            {
                z = new Vector2(
                    z.X * z.X - z.Y * z.Y + uv.X * Hash(seed + i) * 0.1f,
                    2 * z.X * z.Y + uv.Y + Hash(seed + i + 1) * 0.1f
                ) * scale + offset;

                z = new Vector2(
                    z.X * MathF.Cos(angle) - z.Y * MathF.Sin(angle),
                    z.X * MathF.Sin(angle) + z.Y * MathF.Cos(angle)
                );

                float dist = z.LengthSquared();
                if (dist < minDist) minDist = dist;
            }

            color.X = MathF.Sin(minDist * 40 + Hash(seed) * 10);
            color.Y = MathF.Sin(minDist * 35 + Hash(seed + 1) * 10);
            color.Z = MathF.Sin(minDist * 30 + Hash(seed + 2) * 10);

            color = Vector3.Abs(color) * 2.0f;
            color = Vector3.Clamp(color, Vector3.Zero, Vector3.One);
            
            byte r = (byte)(Math.Clamp(color.X, 0, 1) * 255);
            byte g = (byte)(Math.Clamp(color.Y, 0, 1) * 255);
            byte b = (byte)(Math.Clamp(color.Z, 0, 1) * 255);
            
            return new MyColor { R = r, G = g, B = b, A = 255 };
        };
        
        GenerateImage_Custom(image.Texture, image.TextureWidth, image.TextureHeight, getColor, tryReportCallback);
    }

    private static float Hash(float seed)
    {
        return MathF.Abs(MathF.Sin(seed * 112.9898f + seed * 43758.5453f)) % 1.0f;
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

    internal static void Invert(MyImage image, TryReportCallback tryReportCallback)
    {
        ModifyColor modifyColor = (_, _, oldColor) =>
        {
            return new MyColor
            {
                R = (byte)(255 - oldColor.R),
                G = (byte)(255 - oldColor.G),
                B = (byte)(255 - oldColor.B),
                A = oldColor.A
            };
        };
        
        ProcessPixels_Custom(image.Texture, image.TextureWidth, image.TextureHeight, modifyColor, tryReportCallback); 
    }
    
    internal static void FancyShader(MyImage image, TryReportCallback tryReportCallback)
    {
        ModifyColor modifyColor = (normalizedX, normalizedY, oldColor) =>
        {
            float oldR = oldColor.R / 255f;
            float oldG = oldColor.G / 255f;
            float oldB = oldColor.B / 255f;
            
            float luminance = (0.2126f * oldR) + (0.7152f * oldG) + (0.0722f * oldB);
            
            Vector2 center = new Vector2(0.5f, 0.5f);
            Vector2 uv = new Vector2(normalizedX, normalizedY);
            
            Vector2 delta = uv - center;
            float angle = MathF.Atan2(delta.Y, delta.X);
            float distance = delta.Length();
            
            float patternScale = 25f + (oldR * 15f);
            float spiralDensity = 8f * (1f - luminance);
            float colorShift = oldB * 3f;
            
            float pattern = MathF.Sin(
                angle * spiralDensity + 
                MathF.Cos(distance * patternScale) * 5f + 
                MathF.Sin(uv.X * 20f + uv.Y * 15f)
            );
            
            float r = MathF.Abs(pattern + colorShift) * oldR;
            float g = MathF.Abs(pattern - colorShift) * oldG;
            float b = MathF.Abs(pattern * 1.5f) * oldB;
            
            float vignette = 1f - MathF.Pow(distance * 1.4f, 2f);
            
            return new MyColor
            {
                R = (byte)(Math.Clamp(r * vignette * 255, 0, 255)),
                G = (byte)(Math.Clamp(g * vignette * 255, 0, 255)),
                B = (byte)(Math.Clamp(b * vignette * 255, 0, 255)),
                A = 255
            };
        };
        
        ProcessPixels_Custom(image.Texture, image.TextureWidth, image.TextureHeight, modifyColor, tryReportCallback);
    }
    
    internal static void Sepia(MyImage image, TryReportCallback tryReportCallback)
    {
        ModifyColor modifyColor = (_, _, color) => 
        {
            byte r = (byte)Math.Clamp(color.R * 0.393 + color.G * 0.769 + color.B * 0.189, 0, 255);
            byte g = (byte)Math.Clamp(color.R * 0.349 + color.G * 0.686 + color.B * 0.168, 0, 255);
            byte b = (byte)Math.Clamp(color.R * 0.272 + color.G * 0.534 + color.B * 0.131, 0, 255);
            return new MyColor{ R = r, G = g, B = b, A = 255 };
        };
        
        ProcessPixels_Custom(image.Texture, image.TextureWidth, image.TextureHeight, modifyColor, tryReportCallback);
    }
}