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
    /// 从光线与物体的交点中取出和物体的第一个交点的t值
    /// </summary>
    /// <param name="intersections">包含 Intersection 类的数组，为光线和物体的所有潜在交点</param>
    /// <returns>Intersection 类，包含光线和物体第一个交点的 t 值和物体本身</returns>
    public static Intersection? Hit(List<Intersection> intersections)
    {
        Intersection? result = null;
        
        foreach (var x in intersections)
        {
            if (x.T < 0) continue;

            if (result is null || x.T < result.T)
            {
                result = x;
            }
        }

        return result;
    }
    
    /// <summary>
    /// 计算光线和这个物体交点的t，交点坐标，视线向量，法向量
    /// </summary>
    /// <param name="ray">光线</param>
    /// <param name="xs">这道光线和世界中所有物体的交点</param>
    /// <returns>t，物体，交点，视线向量，交点法向量，视线原点是否在物体内(Computation)</returns>
    public Computation PrepareComputations(Ray ray, List<Intersection> xs)
    {
        var hitPoint = ray.Position(T);  // 计算这个交点的世界坐标
        var eyeV = -ray.Direction;  // 计算视线向量
        var normalV = Object.NormalAt(hitPoint);  // 计算这个交点的法向量
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

        // 注意：偏移点必须在法向量翻转之后再计算，
        // 否则从物体内部射出时 overPoint / underPoint 会互相颠倒，
        // 折射光线会立刻再次打到同一个物体上（自相交），最终只剩黑色
        var overPoint = hitPoint + normalV * MathUtils.Epsilon;
        var underPoint = hitPoint - normalV * MathUtils.Epsilon;
        var reflectV = ray.Direction.Reflect(normalV);

        var comps = new Computation(
            T, Object, hitPoint, overPoint, underPoint, 
            eyeV, normalV, reflectV, inside);
        
        var containers = new List<Shape>();
        foreach (var i in xs)
        {
            if (i.Equals(this))
            {
                comps.N1 = containers.Count == 0 ? 1.0 : containers.Last().Material.RefractiveIndex;
            }

            if (containers.Contains(i.Object))
            {
                containers.Remove(i.Object);
            }
            else
            {
                containers.Add(i.Object);
            }

            if (i.Equals(this))
            {
                comps.N2 = containers.Count == 0 ? 1.0 : containers.Last().Material.RefractiveIndex;
                break;
            }
        }

        return comps;
    }
}