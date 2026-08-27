namespace RayTracerChallenge.RayTracer;

public static class Transformations
{
    /// <summary>
    /// 平移矩阵
    /// </summary>
    /// <param name="x">x轴方向平移距离</param>
    /// <param name="y">y轴方向平移距离</param>
    /// <param name="z">z轴方向平移距离</param>
    /// <returns>平移矩阵</returns>
    public static Matrix Translation(double x, double y, double z)
    {
        double[,] t =
        {
            { 1, 0, 0, x },
            { 0, 1, 0, y },
            { 0, 0, 1, z },
            { 0, 0, 0, 1 }
        };
        return new Matrix(t);
    }

    /// <summary>
    /// 放缩矩阵
    /// </summary>
    /// <param name="x">x轴方向放缩系数</param>
    /// <param name="y">y轴方向放缩系数</param>
    /// <param name="z">z轴方向放缩系数</param>
    /// <returns>放缩矩阵</returns>
    public static Matrix Scaling(double x, double y, double z)
    {
        double[,] t =
        {
            { x, 0, 0, 0 },
            { 0, y, 0, 0 },
            { 0, 0, z, 0 },
            { 0, 0, 0, 1 }
        };
        return new Matrix(t);
    }

    /// <summary>
    /// 点或向量绕x轴旋转矩阵
    /// </summary>
    /// <param name="degree">度数</param>
    /// <returns>绕x轴旋转矩阵</returns>
    public static Matrix RotationX(double degree)
    {
        double r = MathUtils.DegToRad(degree);
        double[,] t =
        {
            { 1, 0, 0, 0 },
            { 0, Math.Cos(r), -Math.Sin(r), 0 },
            { 0, Math.Sin(r), Math.Cos(r), 0 },
            { 0, 0, 0, 1 }
        };
        return new Matrix(t);
    }
    
    /// <summary>
    /// 点或向量绕y轴旋转矩阵
    /// </summary>
    /// <param name="degree">度数</param>
    /// <returns>绕y轴旋转矩阵</returns>
    public static Matrix RotationY(double degree)
    {
        double r = MathUtils.DegToRad(degree);
        double[,] t =
        {
            { Math.Cos(r), 0, Math.Sin(r), 0 },
            { 0, 1, 0, 0 },
            { -Math.Sin(r), 0, Math.Cos(r), 0 },
            { 0, 0, 0, 1 }
        };
        return new Matrix(t);
    }
    
    /// <summary>
    /// 点或向量绕z轴旋转矩阵
    /// </summary>
    /// <param name="degree">度数</param>
    /// <returns>绕z轴旋转矩阵</returns>
    public static Matrix RotationZ(double degree)
    {
        double r = MathUtils.DegToRad(degree);
        double[,] t =
        {
            { Math.Cos(r), -Math.Sin(r), 0, 0 },
            { Math.Sin(r), Math.Cos(r), 0, 0 },
            { 0, 0, 1, 0 },
            { 0, 0, 0, 1 }
        };
        return new Matrix(t);
    }

    /// <summary>
    /// 偏斜矩阵
    /// </summary>
    /// <param name="xY">将x轴坐标按y轴坐标比例缩放</param>
    /// <param name="xZ">将x轴坐标按z轴坐标比例缩放</param>
    /// <param name="yX">将y轴坐标按x轴坐标比例缩放</param>
    /// <param name="yZ">将y轴坐标按z轴坐标比例缩放</param>
    /// <param name="zX">将z轴坐标按x轴坐标比例缩放</param>
    /// <param name="zY">将z轴坐标按y轴坐标比例缩放</param>
    /// <returns></returns>
    public static Matrix Shearing(double xY, double xZ, double yX, double yZ, double zX, double zY)
    {
        double[,] t =
        {
            { 1, xY, xZ, 0 },
            { yX, 1, yZ, 0 },
            { zX, zY, 1, 0 },
            { 0, 0, 0, 1 }
        };
        return new Matrix(t);
    }
}