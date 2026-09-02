using System.Diagnostics.Tracing;

namespace RayTracerChallenge.RayTracer;


/// <summary>
/// 形状父类
/// </summary>
public abstract class Shape
{
    public Matrix Transform { get; set; } = Matrix.Identity(4);  // 对物体的变换（应用中其实是对投射到物体上的光线进行逆变换）
    public Material Material { get; set; } = new Material();  // 默认材质

    /// <summary>
    /// 所有形状都需要先将光线转换到本地坐标
    /// </summary>
    /// <param name="ray"></param>
    /// <returns></returns>
    public List<Intersection> Intersect(Ray ray)
    {
        Ray localRay = ray.Transform(Transform.Inverse());
        return LocalIntersect(localRay);
    }

    /// <summary>
    /// 所有形状共用的求世界坐标法向量的逻辑
    /// </summary>
    /// <param name="worldPoint"></param>
    /// <returns></returns>
    public Tuple4 NormalAt(Tuple4 worldPoint)
    {
        var localPoint = Transform.Inverse() * worldPoint;  // 世界坐标系的点转换为物体坐标系点
        var localNormal = LocalNormalAt(localPoint);
        var worldNormal = Transform.Inverse().Transpose() * localNormal;
        worldNormal.W = 0;
        return worldNormal.Normalize();
    }

    public abstract List<Intersection> LocalIntersect(Ray localRay);
    public abstract Tuple4 LocalNormalAt(Tuple4 localPoint);
    
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
}


/// <summary>
/// 球体子类
/// </summary>
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
    /// 计算球体坐标系光线和球体的交点（两个，一个穿入一个穿出）
    /// </summary>
    /// <param name="localRay">变换到球体坐标系的光线</param>
    /// <returns>包含Intersection类的数组，长度可为0，1，2</returns>
    public override List<Intersection> LocalIntersect(Ray localRay)
    {
        Tuple4 sphereToRay = localRay.Origin - Center;
        
        double a = localRay.Direction.Dot(localRay.Direction);
        double b = 2 * localRay.Direction.Dot(sphereToRay);
        double c = sphereToRay.Dot(sphereToRay) - Radius * Radius;

        double discriminant = b * b - 4 * a * c;
        if (discriminant < 0)
        {
            return new List<Intersection> ();
        }

        double t1 = (-b - Math.Sqrt(discriminant)) / (2 * a);
        double t2 = (-b + Math.Sqrt(discriminant)) / (2 * a);

        return new List<Intersection> { new Intersection(t1, this), new Intersection(t2, this) };
    }

    /// <summary>
    /// 球体本地坐标系的一点的法向量
    /// </summary>
    /// <param name="localPoint">球体坐标系中一点</param>
    /// <returns>球体坐标系中这个点的法向量</returns>
    public override Tuple4 LocalNormalAt(Tuple4 localPoint)
    {
        return localPoint - Center;  // 物体坐标系点减去物体坐标系球中心
    }
}


/// <summary>
/// 平面子类
/// </summary>
public class Plane : Shape
{
    public Plane() {}

    public override List<Intersection> LocalIntersect(Ray localRay)
    {
        // 若光线和平面平行则没有交点
        if (Math.Abs(localRay.Direction.Y) < MathUtils.Epsilon)
        {
            return new List<Intersection>();
        }

        double t = -localRay.Origin.Y / localRay.Direction.Y;
        return new List<Intersection> { new Intersection(t, this) };
    }

    public override Tuple4 LocalNormalAt(Tuple4 localPoint)
    {
        return Tuple4.Vector(0, 1, 0);
    }
}