namespace RayTracerChallenge.RayTracer;

public abstract class Light
{
    public Tuple4 Position { get; set; }  // 光源位置
    public Color Intensity { get; set; }  // 光源颜色
    
    public virtual int Samples => 1;

    public virtual Tuple4 PointOnLight(int u, int v) => Position;
    public virtual int USteps => 1;
    public virtual int VSteps => 1;
}

public class PointLight : Light
{
    /// <summary>
    /// 初始化点状光源
    /// </summary>
    /// <param name="position">点光源位置</param>
    /// <param name="intensity">点光源颜色</param>
    public PointLight(Tuple4 position, Color intensity)
    {
        Position = position;
        Intensity = intensity;
    }
}

public class AreaLight : Light
{
    public Tuple4 Corner { get; }
    public Tuple4 UVec { get; }
    public Tuple4 VVec { get; }
    public override int USteps { get; }
    public override int VSteps { get; }
    public override int Samples => USteps * VSteps;
    private readonly Random _rng = new Random();

    /// <summary>
    /// 面状光源，取四个点
    /// </summary>
    /// <param name="position">面光源的中心</param>
    /// <param name="intensity">面光源颜色</param>
    /// <param name="uVec">面光源的一条边向量</param>
    /// <param name="uSteps">这个方向切几份</param>
    /// <param name="vVec">面光源的另一条边向量</param>
    /// <param name="vSteps">这个方向切几份</param>
    public AreaLight(
        Tuple4 position, Color intensity, 
        Tuple4 uVec, int uSteps,
        Tuple4 vVec, int vSteps)
    {
        Position = position;
        Intensity = intensity;
        USteps = uSteps;
        VSteps = vSteps;

        // 由中心反推角点：从中心往回退半条 u 边、半条 v 边
        Corner = position - uVec * 0.5 - vVec * 0.5;

        UVec = uVec / uSteps;  // 存的是一格的边长
        VVec = vVec / vSteps;
    }
    
    public override Tuple4 PointOnLight(int u, int v)
    {
        return Corner + UVec * (u + _rng.NextDouble()) + VVec * (v + _rng.NextDouble());
    }
}