using System.Diagnostics.Tracing;

namespace RayTracerChallenge.RayTracer;


/// <summary>
/// 形状父类
/// </summary>
public abstract class Shape
{
    private Matrix _inverseTransform;
    private Matrix _transform;
    public Matrix Transform  // 对物体的变换（应用中其实是对投射到物体上的光线进行逆变换）
    {
        get => _transform;
        set { _transform = value; _inverseTransform = value.Inverse(); }
    }

    public Matrix InverseTransform => _inverseTransform;
    public Material Material { get; set; } = new Material();  // 默认材质
    public bool CastsShadow { get; set; } = true;

    /// <summary>
    /// 所有形状都需要先将光线转换到本地坐标
    /// </summary>
    /// <param name="ray"></param>
    /// <returns></returns>
    public List<Intersection> Intersect(Ray ray)
    {
        Ray localRay = ray.Transform(InverseTransform);
        return LocalIntersect(localRay);
    }

    /// <summary>
    /// 所有形状共用的求世界坐标法向量的逻辑
    /// </summary>
    /// <param name="worldPoint"></param>
    /// <returns></returns>
    public Tuple4 NormalAt(Tuple4 worldPoint)
    {
        var localPoint = InverseTransform * worldPoint;  // 世界坐标系的点转换为物体坐标系点
        var localNormal = LocalNormalAt(localPoint);
        var worldNormal = InverseTransform.Transpose() * localNormal;
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
        Transform = Matrix.Identity(4);
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
    public Plane()
    {
        Transform = Matrix.Identity(4);
    }

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
    public Cube()
    {
        Transform = Matrix.Identity(4);
    }

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


/// <summary>
/// 圆柱体
/// </summary>
public class Cylinder : Shape
{
    /// <summary>
    /// 一开始圆柱的高是从负无穷到正无穷
    /// </summary>
    public double Minimum { get; set; } = double.NegativeInfinity;
    public double Maximum { get; set; } = double.PositiveInfinity;
    public bool Closed { get; set; } = false;

    public Cylinder()
    {
        Transform = Matrix.Identity(4);
    }

    /// <summary>
    /// 判断一个交点是否在圆柱体的上下面上
    /// </summary>
    /// <param name="ray">光线</param>
    /// <param name="t">交点t值</param>
    /// <returns></returns>
    private bool CheckCap(Ray ray, double t)
    {
        double x = ray.Origin.X + t * ray.Direction.X;
        double z = ray.Origin.Z + t * ray.Direction.Z;
        
        return (x * x + z * z) <= 1;
    }

    /// <summary>
    /// 将光线和上下两个面的交点加入交点列表中
    /// </summary>
    /// <param name="ray">光线</param>
    /// <param name="xs">交点列表</param>
    private void IntersectCaps(Ray ray, List<Intersection> xs)
    {
        // 判断是否存在上下两个面的交点，若没有闭合或光线的方向和xz轴平面平行则没有
        if (!Closed || MathUtils.AlmostEqual(ray.Direction.Y, 0.0))
            return;

        var t0 = (Minimum - ray.Origin.Y) / ray.Direction.Y;
        if (CheckCap(ray, t0))
        {
            xs.Add(new Intersection(t0, this));
        }

        var t1 = (Maximum - ray.Origin.Y) / ray.Direction.Y;
        if (CheckCap(ray, t1))
        {
            xs.Add(new Intersection(t1, this));
        }
    }

    /// <summary>
    /// 求圆柱体和视线相交的交点
    /// </summary>
    /// <param name="localRay">圆柱体坐标系中的光线</param>
    /// <returns>t值列表</returns>
    public override List<Intersection> LocalIntersect(Ray localRay)
    {
        var xs = new List<Intersection>();
        
        var a = localRay.Direction.X * localRay.Direction.X + 
                localRay.Direction.Z * localRay.Direction.Z;
        
        if (MathUtils.AlmostEqual(a, 0.0))
        {
            IntersectCaps(localRay, xs);
            return xs;
        }

        var b = 2 * localRay.Origin.X * localRay.Direction.X +
                2 * localRay.Origin.Z * localRay.Direction.Z;
        
        var c = localRay.Origin.X * localRay.Origin.X + 
                localRay.Origin.Z * localRay.Origin.Z - 1;

        var disc = b * b - 4 * a * c;
        if (disc < 0)
        {
            return xs;
        }

        var t0 = (-b - Math.Sqrt(disc)) / (2 * a);
        var t1 = (-b + Math.Sqrt(disc)) / (2 * a);

        if (t0 > t1)
        {
            (t0, t1) = (t1, t0);
        }
        
        // 核实两个交点的y坐标是否都在截断的部分内，不在的话就是交在了上下两个面
        var y0 = localRay.Origin.Y + t0 * localRay.Direction.Y;  // 第0个交点坐标的y坐标
        if (this.Minimum < y0 && y0 < this.Maximum)
        {
            xs.Add(new Intersection(t0, this));
        }
        
        var y1 = localRay.Origin.Y + t1 * localRay.Direction.Y;  // 第1个交点坐标的y坐标
        if (this.Minimum < y1 && y1 < this.Maximum)
        {
            xs.Add(new Intersection(t1, this));
        }

        IntersectCaps(localRay, xs);

        return xs;
    }

    public override Tuple4 LocalNormalAt(Tuple4 localPoint)
    {
        throw new NotImplementedException();
    }
}