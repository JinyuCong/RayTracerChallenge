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

    /// <summary>
    /// 相机变换矩阵，将世界中的物体移到相机前面
    /// </summary>
    /// <param name="from">相机所在点</param>
    /// <param name="to">相机朝向的点</param>
    /// <param name="up">一个模糊的相机的“上方”</param>
    /// <returns></returns>
    public static Matrix ViewTransformation(Tuple4 from, Tuple4 to, Tuple4 up)
    {
        Tuple4 forward = (to - from).Normalize();  //  相机朝向向量
        Tuple4 upn = up.Normalize();  // 模糊的相机的“上”方向
        Tuple4 left = forward.Cross(upn);  // 和模糊“上”方向和朝向方向正交的左
        Tuple4 trueUp = left.Cross(forward);  // 用左和朝向修正上方向

        // 转动方向矩阵
        Matrix orientation = new Matrix(new [,]
        {
            { left.X, left.Y, left.Z, 0 },
            { trueUp.X, trueUp.Y, trueUp.Z, 0 },
            { -forward.X, -forward.Y, -forward.Z, 0 },
            { 0, 0, 0, 1 }
        });
        
        // 物体平移到相机坐标系之后再进行转动
        return orientation * Translation(-from.X, -from.Y, -from.Z);
    }
}