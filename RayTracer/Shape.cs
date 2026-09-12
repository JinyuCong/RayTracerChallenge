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
    public Group? Parent { get; set; }

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
        var localPoint = WorldToObject(worldPoint);  // 世界坐标系的点转换为物体坐标系点
        var localNormal = LocalNormalAt(localPoint);  // 计算物体本地法向量
        return NormalToWorld(localNormal);  // 本地法向量转换到世界坐标
    }

    public abstract List<Intersection> LocalIntersect(Ray localRay);
    public abstract Tuple4 LocalNormalAt(Tuple4 localPoint);

    /// <summary>
    /// 递归地将世界坐标中的点变换到物体本地坐标点
    /// </summary>
    /// <param name="point">世界坐标点</param>
    /// <returns></returns>
    public Tuple4 WorldToObject(Tuple4 point)
    {
        if (Parent is not null)
        {
            point = Parent.WorldToObject(point);
        }

        return InverseTransform * point;
    }

    /// <summary>
    /// 递归的将物体坐标系的法向量转换到世界坐标系
    /// </summary>
    /// <param name="normal">物体坐标法向量</param>
    /// <returns></returns>
    private Tuple4 NormalToWorld(Tuple4 normal)
    {
        normal = InverseTransform.Transpose() * normal;
        normal.W = 0;
        normal = normal.Normalize();

        if (Parent is not null)
        {
            normal = Parent.NormalToWorld(normal);
        }

        return normal;
    }
    
}


/// <summary>
/// 球体子类
/// </summary>
public class Sphere : Shape
{
    
    /// <summary>
    /// 初始化球
    /// </summary>
    public Sphere()
    {
        Transform = Matrix.Identity(4);
    }

    /// <summary>
    /// 计算球体坐标系光线和球体的交点（两个，一个穿入一个穿出）
    /// </summary>
    /// <param name="localRay">变换到球体坐标系的光线</param>
    /// <returns>包含Intersection类的数组，长度可为0，1，2</returns>
    public override List<Intersection> LocalIntersect(Ray localRay)
    {
        Tuple4 sphereToRay = localRay.Origin;
        
        double a = localRay.Direction.Dot(localRay.Direction);
        double b = 2 * localRay.Direction.Dot(sphereToRay);
        double c = sphereToRay.Dot(sphereToRay) - 1;

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
        return localPoint;  // 物体坐标系点减去物体坐标系球中心
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
    private static bool CheckCap(Ray ray, double t)
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
        if (Minimum < y0 && y0 < Maximum)
        {
            xs.Add(new Intersection(t0, this));
        }
        
        var y1 = localRay.Origin.Y + t1 * localRay.Direction.Y;  // 第1个交点坐标的y坐标
        if (Minimum < y1 && y1 < Maximum)
        {
            xs.Add(new Intersection(t1, this));
        }

        IntersectCaps(localRay, xs);

        return xs;
    }

    /// <summary>
    /// 交点法线
    /// </summary>
    /// <param name="localPoint">交点坐标</param>
    /// <returns></returns>
    public override Tuple4 LocalNormalAt(Tuple4 localPoint)
    {
        var dist = localPoint.X * localPoint.X + localPoint.Z * localPoint.Z;

        // 点在上平面上
        if (dist < 1 && localPoint.Y >= Maximum - MathUtils.Epsilon)
        {
            return Tuple4.Vector(0, 1, 0);
        }

        // 点在下平面下
        if (dist < 1 && localPoint.Y <= Minimum + MathUtils.Epsilon)
        {
            return Tuple4.Vector(0, -1, 0);
        }

        return Tuple4.Vector(localPoint.X, 0, localPoint.Z);
    }
}


/// <summary>
/// 圆锥子类
/// </summary>
public class Cone : Shape
{
    public double Minimum { get; set; } = double.NegativeInfinity;
    public double Maximum { get; set; } = double.PositiveInfinity;
    public bool Closed { get; set; } = false;
    
    public Cone()
    {
        Transform = Matrix.Identity(4);
    }
    
    /// <summary>
    /// 判断一个交点是否在圆锥的上下面上
    /// </summary>
    /// <param name="ray">光线</param>
    /// <param name="t">交点t值</param>
    /// <returns></returns>
    private static bool CheckCap(Ray ray, double t)
    {
        double x = ray.Origin.X + t * ray.Direction.X;
        double y = ray.Origin.Y + t * ray.Direction.Y;
        double z = ray.Origin.Z + t * ray.Direction.Z;
        
        return x * x + z * z <= y * y;  // 默认圆锥上下面的半径就等于圆锥的高
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
    
    public override List<Intersection> LocalIntersect(Ray localRay)
    {
        var xs = new List<Intersection>();
        
        var a = localRay.Direction.X * localRay.Direction.X -
                localRay.Direction.Y * localRay.Direction.Y +
                localRay.Direction.Z * localRay.Direction.Z;
        
        var b = 2 * localRay.Origin.X * localRay.Direction.X -
                2 * localRay.Origin.Y * localRay.Direction.Y +
                2 * localRay.Origin.Z * localRay.Direction.Z;
        
        var c = localRay.Origin.X * localRay.Origin.X -
                localRay.Origin.Y * localRay.Origin.Y +
                localRay.Origin.Z * localRay.Origin.Z;

        // 判别式小于0无实根
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
        if (Minimum < y0 && y0 < Maximum)
        {
            xs.Add(new Intersection(t0, this));
        }
        
        var y1 = localRay.Origin.Y + t1 * localRay.Direction.Y;  // 第1个交点坐标的y坐标
        if (Minimum < y1 && y1 < Maximum)
        {
            xs.Add(new Intersection(t1, this));
        }
        
        // a = 0 但是 b != 0 意味着光线平行于圆锥的斜面
        if (MathUtils.AlmostEqual(a, 0.0) && !MathUtils.AlmostEqual(b, 0.0))
        {
            var t2 = -c / (2 * b);
            xs.Add(new Intersection(t2, this));
        }
        
        IntersectCaps(localRay, xs);

        return xs;
    }

    /// <summary>
    /// 圆锥交点法线
    /// </summary>
    /// <param name="localPoint">交点坐标</param>
    /// <returns></returns>
    public override Tuple4 LocalNormalAt(Tuple4 localPoint)
    {
        var dist = localPoint.X * localPoint.X + localPoint.Z * localPoint.Z;

        if (dist < 1 && localPoint.Y >= Maximum - MathUtils.Epsilon)
        {
            return Tuple4.Vector(0, 1, 0);
        }

        if (dist < 1 && localPoint.Y <= Minimum + MathUtils.Epsilon)
        {
            return Tuple4.Vector(0, -1, 0);
        }
        
        var y = Math.Sqrt(dist);
        if (localPoint.Y > 0)
            y = -y;

        return Tuple4.Vector(localPoint.X, y, localPoint.Z);
    }
}


/// <summary>
/// 形状组合
/// </summary>
public class Group : Shape
{
    private readonly List<Shape> _children = new List<Shape>();

    public Group()
    {
        Transform = Matrix.Identity(4);
    }

    /// <summary>
    /// 向形状组中加入一个形状
    /// </summary>
    /// <param name="s">子形状</param>
    public void AddChild(Shape s)
    {
        s.Parent = this;
        _children.Add(s);
    }
    
    public override List<Intersection> LocalIntersect(Ray localRay)
    {
        var xs = new List<Intersection>();

        foreach (var child in _children)
        {
            var childXs = child.Intersect(localRay);
            foreach (var childIntersection in childXs)
            {
                xs.Add(childIntersection);
            }
        }

        xs.Sort((a, b) => a.T.CompareTo(b.T));
        return xs;
    }

    public override Tuple4 LocalNormalAt(Tuple4 localPoint)
    {
        throw new NotImplementedException();
    }
}


public class Triangle : Shape
{
    public Tuple4 P1 { get; set; }
    public Tuple4 P2 { get; set; }
    public Tuple4 P3 { get; set; }
    public Tuple4 E1 { get; set; }
    public Tuple4 E2 { get; set; }
    public Tuple4 Normal { get; set; }

    /// <summary>
    /// 初始化三角形
    /// </summary>
    /// <param name="p1">顶点1</param>
    /// <param name="p2">顶点2</param>
    /// <param name="p3">顶点3</param>
    public Triangle(Tuple4 p1, Tuple4 p2, Tuple4 p3)
    {
        P1 = p1;
        P2 = p2;
        P3 = p3;
        E1 = p2 - p1;
        E2 = p3 - p1;
        Normal = E2.Cross(E1).Normalize();  // 法向量与两个边向量正交
    }

    /// <summary>
    /// 三角形交点
    /// </summary>
    /// <param name="localRay">三角形本地光线</param>
    /// <returns></returns>
    public override List<Intersection> LocalIntersect(Ray localRay)
    {
        var dirCrossE2 = localRay.Direction.Cross(E2);
        var det = E1.Dot(dirCrossE2);
        if (Math.Abs(det) < MathUtils.Epsilon)
        {
            return new List<Intersection>();
        }

        var f = 1.0 / det;
        var p1ToOrigin = localRay.Origin - P1;
        var u = f * p1ToOrigin.Dot(dirCrossE2);
        if (u < 0 || u > 1)
        {
            return new List<Intersection>();
        }

        var originCrossE1 = p1ToOrigin.Cross(E1);
        var v = f * localRay.Direction.Dot(originCrossE1);
        if (v < 0 || u + v > 1)
        {
            return new List<Intersection>();
        }

        var t = f * E2.Dot(originCrossE1);
        
        return new List<Intersection> { new Intersection(t, this) };
    }

    public override Tuple4 LocalNormalAt(Tuple4 localPoint)
    {
        return Normal;
    }
}