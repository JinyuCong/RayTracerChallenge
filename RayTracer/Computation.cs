namespace RayTracerChallenge.RayTracer;

public class Computation
{
    public double T { get; set; }
    public Shape Object { get; set; }
    public Tuple4 Point { get; set; }
    public Tuple4 OverPoint { get; set; }
    public Tuple4 UnderPoint { get; set; }
    public Tuple4 EyeV { get; set; }
    public Tuple4 NormalV { get; set; }
    public Tuple4 ReflectV { get; set; }
    public bool Inside { get; set; }
    public double N1 { get; set; }
    public double N2 { get; set; }

    /// <summary>
    /// 封装光线与物体交点所有相关的计算数值和向量
    /// </summary>
    /// <param name="t">光线与物体相交点t值</param>
    /// <param name="obj">物体</param>
    /// <param name="point">交点坐标</param>
    /// <param name="overPoint">交点坐标(向物体外偏移一点来消除噪点)</param>
    /// <param name="underPoint">交点坐标(向物体内偏移一点)</param>
    /// <param name="eyeV">视线向量</param>
    /// <param name="normalV">交点法向量</param>
    /// <param name="reflectV">交点的反射光线</param>
    /// <param name="inside">视线原点是否在物体内部</param>
    /// <param name="n1">第一个物体的折射率</param>
    /// <param name="n2">第二个物体的折射率</param>
    public Computation(double t, Shape obj, Tuple4 point,
        Tuple4 overPoint, Tuple4 underPoint, Tuple4 eyeV, Tuple4 normalV, 
        Tuple4 reflectV, bool inside, double n1 = 1.0, double n2 = 1.0)
    {
        T = t;
        Object = obj;
        Point = point;
        OverPoint = overPoint;
        UnderPoint = underPoint;
        EyeV = eyeV;
        NormalV = normalV;
        ReflectV = reflectV;
        Inside = inside;
        N1 = n1;
        N2 = n2;
    }
}