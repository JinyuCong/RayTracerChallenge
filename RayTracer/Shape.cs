namespace RayTracerChallenge.RayTracer;

public abstract class Shape
{
    public Matrix Transform { get; set; } = Matrix.Identity(4);  // 对物体的变换（应用中其实是对投射到物体上的光线进行逆变换）
    public Material Material { get; set; } = new Material();  // 默认材质

    public abstract List<Intersection> Intersect(Ray ray);
    public abstract Tuple4 NormalAt(Tuple4 worldPoint);
}


public class Sphere : Shape
{
    public Tuple4 Center { get; }  // 球心坐标
    public double Radius { get; }  // 球的半径
    
    /// <summary>
    /// 初始化球
    /// </summary>
    /// <param name="center">球的中心点（Tuple4）</param>
    /// <param name="radius">球的半径（double）</param>
    public Sphere(Tuple4 center, double radius)
    {
        Center = center;
        Radius = radius;
    }

    public static Sphere Default()
    {
        return new Sphere(Tuple4.Point(0, 0, 0), 1);
    }

    /// <summary>
    /// 计算光线和球体的交点（两个，一个穿入一个穿出）
    /// </summary>
    /// <param name="ray">光线类</param>
    /// <returns>包含Intersection类的数组，长度可为0，1，2</returns>
    public override List<Intersection> Intersect(Ray ray)
    {
        Ray ray2 = ray.Transform(Transform.Inverse());  // 对物体做变换相当于对光线做逆变换（逆矩阵乘光线的原点和方向）
        
        Tuple4 sphereToRay = ray2.Origin - Center;
        
        double a = ray2.Direction.Dot(ray2.Direction);
        double b = 2 * ray2.Direction.Dot(sphereToRay);
        double c = sphereToRay.Dot(sphereToRay) - Radius * Radius;

        double discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            return new List<Intersection> {};
        }

        double t1 = (-b - Math.Sqrt(discriminant)) / (2 * a);
        double t2 = (-b + Math.Sqrt(discriminant)) / (2 * a);

        return new List<Intersection> { new Intersection(t1, this), new Intersection(t2, this) };
    }
    
    /// <summary>
    /// 从光线与物体的交点中取出和物体的第一个交点的t值
    /// </summary>
    /// <param name="intersections">包含 Intersection 类的数组，为光线和物体的所有潜在交点</param>
    /// <returns>Intersection 类，包含光线和物体第一个交点的 t 值和物体本身</returns>
    public Intersection? Hit(List<Intersection> intersections)
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
    /// 求空间中任意一点与球的法向量
    /// </summary>
    /// <param name="worldPoint">空间中任意一点</param>
    /// <returns>归一化法向量</returns>
    public override Tuple4 NormalAt(Tuple4 worldPoint)
    {
        var objectPoint = Transform.Inverse() * worldPoint;  // 世界坐标系的点转换为物体坐标系点
        var objectNormal = objectPoint - Center;  // 物体坐标系点减去物体坐标系球中心
        var worldNormal = Transform.Inverse().Transpose() * objectNormal;  // 物体坐标系重新转回世界坐标系
        worldNormal.W = 0;  // W归0
        return worldNormal.Normalize();  // 向量归一化
    }
}