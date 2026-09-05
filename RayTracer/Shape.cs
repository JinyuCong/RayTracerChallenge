using System.Diagnostics.Tracing;

namespace RayTracerChallenge.RayTracer;


/// <summary>
/// 形状父类
/// </summary>
public abstract class Shape
{
    public Matrix Transform { get; set; } = Matrix.Identity(4);  // 对物体的变换（应用中其实是对投射到物体上的光线进行逆变换）
    public Material Material { get; set; } = new Material();  // 默认材质
    public bool CastsShadow { get; set; } = true;

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

    public static Sphere GlassSphere()
    {
        var s = new Sphere(Tuple4.Point(0, 0, 0), 1);
        s.Material.Transparency = 1.0;
        s.Material.RefractiveIndex = 1.5;
        return s;
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

/// <summary>
/// 方体子类
/// </summary>
public class Cube : Shape
{
    public Cube() {}

    private Tuple<double, double> CheckAxis(double origin, double direction)
    {
        double tMinNumerator = -1 - origin;
        double tMaxNumerator = 1 - origin;

        double tMin;
        double tMax;
        if (Math.Abs(direction) >= MathUtils.Epsilon)
        {
            tMin = tMinNumerator / direction;
            tMax = tMaxNumerator / direction;
        }
        else
        {
            tMin = tMinNumerator * double.PositiveInfinity;
            tMax = tMaxNumerator * double.PositiveInfinity;
        }

        if (tMin > tMax)
        {
            double temp;
            temp = tMin;
            tMin = tMax;
            tMax = temp;
        }

        return new Tuple<double, double>(tMin, tMax);
    }
    
    public override List<Intersection> LocalIntersect(Ray localRay)
    {
        var (xTMin, xTMax) = CheckAxis(localRay.Origin.X, localRay.Direction.X);
        var (yTMin, yTMax) = CheckAxis(localRay.Origin.Y, localRay.Direction.Y);
        var (zTMin, zTMax) = CheckAxis(localRay.Origin.Z, localRay.Direction.Z);

        var tMin = new List<double> { xTMin, yTMin, zTMin }.Max();
        var tMax = new List<double> { xTMax, yTMax, zTMax }.Min();

        if (tMin > tMax)
            return new List<Intersection>();
        
        return new List<Intersection> {new Intersection(tMin, this), new Intersection(tMax, this)};
    }

    public override Tuple4 LocalNormalAt(Tuple4 localPoint)
    {
        var maxC = new List<double> { Math.Abs(localPoint.X), Math.Abs(localPoint.Y), Math.Abs(localPoint.Z) }.Max();

        if (MathUtils.AlmostEqual(maxC, Math.Abs(localPoint.X)))
        {
            return Tuple4.Vector(localPoint.X, 0, 0);
        }

        if (MathUtils.AlmostEqual(maxC, Math.Abs(localPoint.Y)))
        {
            return Tuple4.Vector(0, localPoint.Y, 0);
        }

        return Tuple4.Vector(0, 0, localPoint.Z);
    }
}