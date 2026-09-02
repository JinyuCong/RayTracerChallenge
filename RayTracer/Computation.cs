namespace RayTracerChallenge.RayTracer;

public class Computation
{
    public double T { get; }
    public Shape Object { get; }
    public Tuple4 Point { get; }
    public Tuple4 OverPoint { get; }
    public Tuple4 EyeV { get; }
    public Tuple4 NormalV { get; }
    public Tuple4 ReflectV { get; }
    public bool Inside { get; }

    /// <summary>
    /// 封装光线与物体交点所有相关的计算数值和向量
    /// </summary>
    /// <param name="t">光线与物体相交点t值</param>
    /// <param name="obj">物体</param>
    /// <param name="point">交点坐标</param>
    /// <param name="overPoint">交点坐标(向物体外偏移一点来消除噪点)</param>
    /// <param name="eyeV">视线向量</param>
    /// <param name="normalV">交点法向量</param>
    /// <param name="reflectV">交点的反射光线</param>
    /// <param name="inside">视线原点是否在物体内部</param>
    public Computation(double t, Shape obj, Tuple4 point,
        Tuple4 overPoint, Tuple4 eyeV, 
        Tuple4 normalV, Tuple4 reflectV, bool inside)
    {
        T = t;
        Object = obj;
        Point = point;
        OverPoint = overPoint;
        EyeV = eyeV;
        NormalV = normalV;
        ReflectV = reflectV;
        Inside = inside;
    }
}