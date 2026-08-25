namespace RayTracerChallenge.RayTracer;

public class Color
{
    public double Red { get; }
    public double Green { get; }
    public double Blue { get; }

    public Color(double red, double green, double blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    public static Color operator +(Color a, Color b)
    {
        return new Color(a.Red + b.Red, a.Green + b.Green, a.Blue + b.Blue);
    }
    
    public static Color operator -(Color a, Color b)
    {
        return new Color(a.Red - b.Red, a.Green - b.Green, a.Blue - b.Blue);
    }
    
    public static Color operator *(Color a, double b)
    {
        return new Color(a.Red * b, a.Green * b, a.Blue * b);
    }

    public static Color operator *(Color a, Color b)
    {
        return new Color(a.Red * b.Red, a.Green * b.Green, a.Blue * b.Blue);
    }

    public override string ToString()
    {
        return $"(r:{Red}, g:{Green}, b:{Blue})";
    }

}