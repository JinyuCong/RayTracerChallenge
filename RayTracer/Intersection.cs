namespace RayTracerChallenge.RayTracer;

public class Intersection
{
    public double T { get; }
    public Shape Object { get; }
    
    /// <summary>
    /// 光线和一个物体相交点的类
    /// </summary>
    /// <param name="t">交点的t值</param>
    /// <param name="shape">物体</param>
    public Intersection(double t, Shape shape)
    {
        T = t;
        Object = shape;
    }
    
    /// <summary>
    /// 计算光线和这个物体交点的t，交点坐标，视线向量，法向量
    /// </summary>
    /// <param name="ray">光线</param>
    /// <returns>t，物体，交点，视线向量，交点法向量，视线原点是否在物体内(Computation)</returns>
    public Computation PrepareComputations(Ray ray)
    {
        var hitPoint = ray.Position(T);  // 计算这个交点的世界坐标
        var eyeV = -ray.Direction;  // 计算视线向量
        var normalV = Object.NormalAt(hitPoint);  // 计算这个交点的法向量
        var overPoint = hitPoint + normalV * MathUtils.Epsilon;
        bool inside;  // 判断视线原点是否在物体内
        
        if (eyeV.Dot(normalV) < 0)
        {
            inside = true;
            normalV = -normalV;
        }
        else
        {
            inside = false;
        }

        return new Computation(
            T,
            Object,
            hitPoint,
            overPoint,
            eyeV, 
            normalV,
            inside);
    }
}