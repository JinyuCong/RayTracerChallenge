namespace RayTracerChallenge;

using RayTracer;
using Tests;

public static class Program
{
    public static void Main(string[] args)
    {
        var a = Sphere.Default();
        a.Transform = Transformations.Scaling(2, 2, 2);
        a.Material.RefractiveIndex = 1.5;
        
        var b = Sphere.Default();
        b.Transform = Transformations.Translation(0, 0, -0.25);
        b.Material.RefractiveIndex = 2.0;
        
        var c = Sphere.Default();
        c.Transform = Transformations.Translation(0, 0, 0.25);
        c.Material.RefractiveIndex = 2.5;

        var r = new Ray(Tuple4.Point(0, 0, -4), Tuple4.Vector(0, 0, 1));
        var xs = new List<Intersection>
        {
            a.Intersect(r)[0], b.Intersect(r)[0], c.Intersect(r)[0], b.Intersect(r)[1], c.Intersect(r)[1],
            a.Intersect(r)[1]
        };
        for (int i = 0; i < xs.Count; i++)
        {
            var comps = xs[i].PrepareComputations(r);
            Console.WriteLine(comps.Point);
        }
    }
}