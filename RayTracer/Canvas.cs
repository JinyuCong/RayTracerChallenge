using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace RayTracerChallenge.RayTracer;

public class Canvas
{
    public int Width { get; }
    public int Height { get; }
    private Color[,] C { get; set; }

    public Color this[int x, int y]
    {
        get => C[x, y];
    } 
    
    public Canvas(int w, int h)
    {
        Width = w;
        Height = h;
        C = new Color[w, h];
        
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                C[x, y] = new Color(0.0, 0.0, 0.0);
            }
        }
    }

    public void WritePixel(int i, int j, Color c)
    {
        C[i, j] = c;
    }

    // 将画布写入ppm文件
    public string CanvasToPpm()
    {
        var sb = new StringBuilder();

        sb.Append("P3\n");
        sb.Append($"{Width} {Height}\n");
        sb.Append("255\n");

        for (int y = 0; y < Height; y++)
        {
            // 存储画布中一行的值
            var lineValues = new List<string>();
            for (int x = 0; x < Width; x++)
            {
                Color c = C[x, y];
                lineValues.Add(ScaleColor(c.Red).ToString());
                lineValues.Add(ScaleColor(c.Green).ToString());
                lineValues.Add(ScaleColor(c.Blue).ToString());
            }

            // 写入ppm文件时一行的值
            var currentLine = new StringBuilder();

            foreach (var val in lineValues)
            {
                // 再加上这个值就会超过 70 个字符，先把当前行输出，另起一行
                if (currentLine.Length > 0 && currentLine.Length + 1 + val.Length > 70)
                {
                    sb.Append(currentLine).Append('\n');
                    currentLine.Clear();
                }

                if (currentLine.Length > 0)
                {
                    currentLine.Append(' ');
                }

                currentLine.Append(val);
            }

            if (currentLine.Length > 0)
            {
                sb.Append(currentLine).Append('\n');
            }
        }

        if (sb.Length == 0 || sb[sb.Length - 1] != '\n')
        {
            sb.Append('\n');
        }

        return sb.ToString();
    }
    
    private static int ScaleColor(double value)
    {
        int scaled = (int)Math.Round(value * 255);
        return Math.Clamp(scaled, 0, 255);
    }

    // 保存画布到path
    public void SavePpm(string path)
    {
        string ppmContent = CanvasToPpm();
        File.WriteAllText(path, ppmContent);
    }

    public void SavePng(string path)
    {
        using var image = new Image<Rgb24>(Width, Height);
    
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var c = this[x, y];
                byte r = (byte)Math.Clamp((int)Math.Round(c.Red * 255), 0, 255);
                byte g = (byte)Math.Clamp((int)Math.Round(c.Green * 255), 0, 255);
                byte b = (byte)Math.Clamp((int)Math.Round(c.Blue * 255), 0, 255);
                image[x, y] = new Rgb24(r, g, b);
            }
        }
    
        image.Save(path); // 根据扩展名自动判断格式，比如 output.png
    }
}