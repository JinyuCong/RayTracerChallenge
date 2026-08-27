namespace RayTracerChallenge.RayTracer;

public class Intersection
{
    public double T { get; }
    public Sphere Obj { get; }
    /// <summary>
    /// 光线和物体相交点的类
    /// </summary>
    /// <param name="t">交点的t值</param>
    /// <param name="s">物体</param>
    public Intersection(double t, Sphere s)
    {
        T = t;
        Obj = s;
    }
}