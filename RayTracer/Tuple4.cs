using RayTracerChallenge.MyExceptions;

namespace RayTracerChallenge.RayTracer;

public class Tuple4
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double W { get; set; }

    public Tuple4(){}
    
    public Tuple4(double x, double y, double z, double w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public bool IsPoint()
    {
        return MathUtils.AlmostEqual(W, 1.0);
    }

    public bool IsVector()
    {
        return MathUtils.AlmostEqual(W, 0.0);
    }

    public static Tuple4 Point(double x, double y, double z)
    {
        return new Tuple4(x, y, z, 1.0);
    }

    public static Tuple4 Vector(double x, double y, double z)
    {
        return new Tuple4(x, y, z, 0.0);
    }
    
    // 加法：点+向量=点，向量+向量=向量
    public static Tuple4 operator +(Tuple4 a, Tuple4 b)
    {
        if (a.W != 0.0 && b.W != 0.0)
        {
            throw new PointPlusException("cannot add between two points");
        }
        return new Tuple4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    }
    
    // 减法：点-点=向量，点-向量=点
    public static Tuple4 operator -(Tuple4 a, Tuple4 b)
    {
        return new Tuple4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
    }
    
    
    // 取负号
    public static Tuple4 operator -(Tuple4 a)
    {
        return new Tuple4(-a.X, -a.Y, -a.Z, -a.W);
    }
    
    // 乘标量
    public static Tuple4 operator *(Tuple4 a, double scalar)
    {
        return new Tuple4(scalar * a.X, scalar * a.Y, scalar * a.Z, scalar * a.W);
    }
    
    // 除标量
    public static Tuple4 operator /(Tuple4 a, double scalar)
    {
        return new Tuple4(a.X / scalar, a.Y / scalar, a.Z / scalar, a.W / scalar);
    }
    
    
    
    // 相等比较：必须用 Epsilon 容差
    public static bool operator ==(Tuple4 a, Tuple4 b)
    {
        return MathUtils.AlmostEqual(a.X, b.X) && 
               MathUtils.AlmostEqual(a.Y, b.Y) && 
               MathUtils.AlmostEqual(a.Z, b.Z) && 
               MathUtils.AlmostEqual(a.W, b.W);
            
    }

    public static bool operator !=(Tuple4 a, Tuple4 b)
    {
        return !(a == b);
    }
    
    // 重写 Equals(object) —— 消除警告
    public override bool Equals(object? obj)
    {
        return obj is Tuple4 other && Equals(other);
    }
    
    // 必须同步重写 GetHashCode
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, W);
    }
    
    // 模长
    public double Magnitude()
    {
        return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2) + Math.Pow(W, 2));
    }
    
    // 长度归一化
    public Tuple4 Normalize()
    {
        var norm = Magnitude();
        return this / norm;
    }
    
    // 向量点积
    public double Dot(Tuple4 other)
    {
        return X * other.X +
               Y * other.Y +
               Z * other.Z +
               W * other.W;
    }
    
    // 向量叉积
    public Tuple4 Cross(Tuple4 other)
    {
        return new Tuple4(
            Y * other.Z - Z * other.Y,
            Z * other.X - X * other.Z,
            X * other.Y - Y * other.X,
            0
        );
    }

    /// <summary>
    /// 求入射向量关于法线的反射向量
    /// </summary>
    /// <param name="normal">法线</param>
    /// <returns>反射向量</returns>
    public Tuple4 Reflect(Tuple4 normal)
    {
        return this - normal * 2 * Dot(normal);
    }

    public override string ToString()
    {
        return $"({X}, {Y}, {Z}, {W})";
    }
}