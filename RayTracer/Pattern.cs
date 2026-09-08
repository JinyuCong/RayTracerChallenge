namespace RayTracerChallenge.RayTracer;

/// <summary>
/// 图案父类
/// </summary>
public abstract class Pattern
{
    public Color ColorA { get; set; } = new Color(0, 0, 0);
    public Color ColorB { get; set; } = new Color(1, 1, 1);
    private Matrix _inverseTransform;
    private Matrix _transform;
    public Matrix Transform
    {
        get => _transform;
        set { _transform = value; _inverseTransform = value.Inverse(); }
    }

    public Matrix InverseTransform => _inverseTransform;
    /// <summary>
    /// 有图案的物体在世界坐标中的一个点的颜色
    /// </summary>
    /// <param name="obj">这个物体</param>
    /// <param name="worldPoint">世界坐标点</param>
    /// <returns></returns>
    public Color PatternAtShape(Shape obj, Tuple4 worldPoint)
    {
        // 将世界坐标转换为物体坐标
        Tuple4 objectPoint = obj.InverseTransform * worldPoint;
        
        // 将物体坐标转换为图案坐标
        Tuple4 patternPoint = this.InverseTransform * objectPoint;
        return LocalColorAt(patternPoint);
    }

    protected abstract Color LocalColorAt(Tuple4 localPoint);
}

/// <summary>
/// 竖条带状图案
/// </summary>
public class StripePattern : Pattern
{
    public StripePattern(Color colorA, Color colorB)
    {
        ColorA = colorA;
        ColorB = colorB;
        Transform = Matrix.Identity(4);
    }

    /// <summary>
    /// 竖条带状图案本地坐标系的一个点的颜色
    /// </summary>
    /// <param name="patternPoint">图案本地坐标系点</param>
    /// <returns>这个点的颜色</returns>
    protected override Color LocalColorAt(Tuple4 patternPoint)
    {
        return (Math.Floor(patternPoint.X) % 2 == 0) ? ColorA : ColorB;
    }
}


/// <summary>
/// 双色渐变图案
/// </summary>
public class GradientPattern : Pattern
{
    public GradientPattern(Color colorA, Color colorB)
    {
        ColorA = colorA;
        ColorB = colorB;
        Transform = Matrix.Identity(4);
    }

    protected override Color LocalColorAt(Tuple4 localPoint)
    {
        return ColorA + (ColorB - ColorA) * (localPoint.X - Math.Floor(localPoint.X));
    }
}


/// <summary>
/// 交替圆圈图案
/// </summary>
public class RingPattern : Pattern
{
    public RingPattern(Color colorA, Color colorB)
    {
        ColorA = colorA;
        ColorB = colorB;
        Transform = Matrix.Identity(4);
    }

    protected override Color LocalColorAt(Tuple4 localPoint)
    {
        return (Math.Floor(
            Math.Sqrt(localPoint.X * localPoint.X + localPoint.Z * localPoint.Z)
        ) % 2 == 0)
            ? ColorA
            : ColorB;
    }
}


/// <summary>
/// 象棋格图案
/// </summary>
public class CheckerPattern : Pattern
{
    public CheckerPattern(Color colorA, Color colorB)
    {
        ColorA = colorA;
        ColorB = colorB;
        Transform = Matrix.Identity(4);
    }

    protected override Color LocalColorAt(Tuple4 localPoint)
    {
        return (Math.Floor(localPoint.X) + Math.Floor(localPoint.Y) + Math.Floor(localPoint.Z)) % 2 == 0 
            ? ColorA 
            : ColorB;
    }
}


/// <summary>
/// 交替圆圈渐变图案
/// </summary>
public class RadialGradientPattern : Pattern
{
    public RadialGradientPattern(Color colorA, Color colorB)
    {
        ColorA = colorA;
        ColorB = colorB;
        Transform = Matrix.Identity(4);
    }

    protected override Color LocalColorAt(Tuple4 localPoint)
    {
        Color gradientColorX = ColorA + (ColorB - ColorA) * (localPoint.X - Math.Floor(localPoint.X));
        Color gradientColorZ = ColorA + (ColorB - ColorA) * (localPoint.Z - Math.Floor(localPoint.Z));
        
        return (Math.Floor(
            Math.Sqrt(localPoint.X * localPoint.X + localPoint.Z * localPoint.Z)
        ) % 2 == 0)
            ? gradientColorX
            : gradientColorZ;
    }
}