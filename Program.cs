using System.Drawing;

namespace RayTracerChallenge;
using RayTracer;
using Tests;
using MyExceptions;

public static class Program
{
    public static void Main(string[] args)
    {
        double[,] a =
        {
            {8, -5, 9, 2},
            {7, 5, 6, 1},
            {-6, 0, 9, 6},
            {-3, 0, -9, -4}
        };
        
        var A = new Matrix(a);

        Matrix invTrans = A.Inverse().Transpose();
        Matrix transInv = A.Transpose().Inverse();
        Console.WriteLine(invTrans);
        Console.WriteLine();
        Console.WriteLine(transInv);
    }
}