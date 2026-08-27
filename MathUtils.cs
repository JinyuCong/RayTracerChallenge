using RayTracerChallenge.MyExceptions;

namespace RayTracerChallenge;
using RayTracerChallenge.RayTracer;

public static class MathUtils
{
    private const double Epsilon = 0.00001;
    
    public static bool AlmostEqual(double a, double b)
    {
        return Math.Abs(a - b) < Epsilon;
    }

    /// <summary>
    /// 将角度转换为radius
    /// </summary>
    /// <param name="degree"></param>
    /// <returns></returns>
    public static double DegToRad(double degree)
    {
        return degree / 180 * Math.PI;
    }
}