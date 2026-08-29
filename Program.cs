using System.Drawing;
using System.Xml;

namespace RayTracerChallenge;
using RayTracer;
using Tests;
using MyExceptions;

public static class Program
{
    public static void Main(string[] args)
    {
        var from = Tuple4.Point(1, 3, 2);
        var to = Tuple4.Point(4, -2, 8);
        var up = Tuple4.Vector(1, 1, 0);
        Console.WriteLine(Transformations.ViewTransformation(from, to, up));
    }
}