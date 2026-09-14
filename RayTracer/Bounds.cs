namespace RayTracerChallenge.RayTracer;

public struct Bounds
{
    public Tuple4 MinPoint { get; set; }
    public Tuple4 MaxPoint { get; set; }

    /// <summary>
    /// 初始化包围盒，包围盒有一个三个轴中的最小点，和一个三个轴中的最大点
    /// </summary>
    /// <param name="minPoint"></param>
    /// <param name="maxPoint"></param>
    public Bounds(Tuple4 minPoint, Tuple4 maxPoint)
    {
        MinPoint = minPoint;
        MaxPoint = maxPoint;
    }
}