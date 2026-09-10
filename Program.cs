namespace RayTracerChallenge;

using RayTracer;
using Tests;

public static class Program
{
    public static void Main(string[] args)
    {
        var cyl = new Cylinder();
        cyl.Minimum = 1;
        cyl.Maximum = 2;
        cyl.Closed = true;

        var directions = new List<Tuple4>
        {
            Tuple4.Vector(0, -1, 0), Tuple4.Vector(0, -1, 2), Tuple4.Vector(0, -1, 1),
            Tuple4.Vector(0, 1, 2), Tuple4.Vector(0, 1, 1)
        };

        var points = new List<Tuple4>
        {
            Tuple4.Point(0, 3, 0), Tuple4.Point(0, 3, -2), Tuple4.Point(0, 4, -2), 
            Tuple4.Point(0, 0, -2), Tuple4.Point(0, -1, -2) 
        };

        foreach (var (p, d) in Enumerable.Zip(points, directions))
        {
            var normedD = d.Normalize();
            var ray = new Ray(p, normedD);
            var xs = cyl.LocalIntersect(ray);
            Console.WriteLine(xs.Count);
        }

    }
}