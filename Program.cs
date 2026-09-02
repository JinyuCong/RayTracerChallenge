namespace RayTracerChallenge;

using RayTracer;
using Tests;

public static class Program
{
    public static void Main(string[] args)
    {
        // var pattern = new StripePattern(new Color(1, 1, 1), new Color(0, 0, 0));
        // var m = new Material(pattern: pattern, ambient: 1, diffuse: 0, specular: 0);
        // var eyeV = Tuple4.Vector(0, 0, -1);
        // var normalV = Tuple4.Vector(0, 0, -1);
        // var light = new Light(Tuple4.Point(0, 0, -10), new Color(1, 1, 1));
        //
        // var c1 = World.Lighting(m, new List<Light> { light }, Tuple4.Point(0.9, 0, 0), eyeV, normalV,
        //     new List<bool> { false });
        // var c2 = World.Lighting(m, new List<Light> { light }, Tuple4.Point(1.1, 0, 0), eyeV, normalV,
        //     new List<bool> { false });
        //
        // Console.WriteLine(c1);
        // Console.WriteLine(c2);
        
        Challenges.PuttingPatterns();
    }
}