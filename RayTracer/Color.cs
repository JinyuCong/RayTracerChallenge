namespace RayTracerChallenge.RayTracer;

public struct Color : IEquatable<Color>
{
    public double Red { get; }
    public double Green { get; }
    public double Blue { get; }
    
    public Color() {}
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

    public bool Equals(Color other)
    {
        return MathUtils.AlmostEqual(Red, other.Red) &&
               MathUtils.AlmostEqual(Green, other.Green) &&
               MathUtils.AlmostEqual(Blue, other.Blue);
    }

    public override bool Equals(object? obj)
    {
        return obj is Color other && Equals(other);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Red, Green, Blue);
    }
    
    public static bool operator ==(Color a, Color b)
    {
        return a.Equals(b);
    }
    
    public static bool operator !=(Color a, Color b)
    {
        return !a.Equals(b);
    }
    
    public override string ToString()
    {
        return $"(r:{Red}, g:{Green}, b:{Blue})";
    }

}