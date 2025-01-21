using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace MinImage;

public class MyImage
{
    public NativeLibrary.MyColor[] Texture;
    public int TextureWidth;
    public int TextureHeight;

    public MyImage(int textureWidth, int textureHeight)
    {
        TextureWidth = textureWidth;
        TextureHeight = textureHeight;
        Texture = new NativeLibrary.MyColor[textureWidth * textureHeight]; 
    }

    public void Save(string filePath, NativeLibrary.TryReportCallback callback)
    {
        string? directoryPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directoryPath))
            Directory.CreateDirectory(directoryPath);

        using (var image = new Image<Rgba32>(TextureWidth, TextureHeight))
        {
            for (int y = 0; y < TextureHeight; y++)
            {
                for (int x = 0; x < TextureWidth; x++)
                {
                    NativeLibrary.MyColor color = Texture[y * TextureWidth + x];
                    image[x, y] = new Rgba32(color.R, color.G, color.B, color.A);
                }
            }
            
            image.Save(filePath);
        }
        
        callback(1.0f);
    }
}