using System.Numerics;

namespace RayTracerChallenge.RayTracer;

/// <summary>
/// 光线类，有一个原点（point）一个方向（vector）
/// </summary>
public class Ray
{
    public Tuple4 Origin { get; }
    public Tuple4 Direction { get; }

    public Ray(Tuple4 origin, Tuple4 direction)
    {
        Origin = origin;
        Direction = direction;
    }
    
    /// <summary>
    /// 计算一道光线经过t后到达的点，t为变量
    /// </summary>
    /// <param name="r">光线</param>
    /// <param name="t">时间</param>
    /// <returns>Tuple4: 到达的点</returns>
    public Tuple4 Position(double t)
    {
        return Origin + Direction * t;
    }

    /// <summary>
    /// 对光线做出变换（平移、旋转 ...）
    /// </summary>
    /// <param name="m">变换矩阵</param>
    /// <returns>变换后的光线</returns>
    public Ray Transform(Matrix m)
    {
        return new Ray(m * Origin, m * Direction);
    }
    
}