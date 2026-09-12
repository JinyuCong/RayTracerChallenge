using System.Text;
using RayTracerChallenge.Exceptions;

namespace RayTracerChallenge.RayTracer;

/// <summary>
/// 定义矩阵类
/// </summary>
public struct Matrix : IEquatable<Matrix>
{
    private readonly double[,] _values;
    public int NumRows { get; }
    public int NumCols { get; }
    
    /// <summary>
    /// 初始化矩阵
    /// </summary>
    /// <param name="array">数组形式矩阵</param>
    public Matrix(double[,] array)
    {
        _values = array;
        NumRows = array.GetLength(0);
        NumCols = array.GetLength(1);
    }
    
    // 索引器
    public double this[int row, int col]  
    {                                     
        get => _values[row, col];         
        set => _values[row, col] = value; 
    } 
    
    public static Matrix operator +(Matrix a, Matrix b)
    {
        double[,] res = new double[a.NumRows, a.NumCols];
        
        for (int i = 0; i < a.NumRows; i++)
        {
            for (int j = 0; j < a.NumCols; j++)
            {
                res[i, j] = a[i, j] + b[i, j];
            }
        }

        return new Matrix(res);
    }
    
    public static Matrix operator -(Matrix a, Matrix b)
    {
        double[,] res = new double[a.NumRows, a.NumCols];
        
        for (int i = 0; i < a.NumRows; i++)
        {
            for (int j = 0; j < a.NumCols; j++)
            {
                res[i, j] = a[i, j] - b[i, j];
            }
        }

        return new Matrix(res);
    }
    
    // 矩阵乘矩阵
    public static Matrix operator *(Matrix a, Matrix b)
    {
        if (a.NumCols != b.NumRows)
        {
            throw new MatrixCannotMultiplyException("number of rows of matrix A and number of " +
                                                    "columns of matrix B are not equal");
        }
        
        var c = new double[a.NumRows, b.NumCols];

        for (int row = 0; row < a.NumRows; row++)
        {
            for (int col = 0; col < b.NumCols; col++)
            {
                for (int i = 0; i < a.NumCols; i++)
                {
                    c[row, col] += a[row, i] * b[i, col];
                }
            }
        }

        return new Matrix(c);
    }
    
    // 矩阵乘向量
    public static Tuple4 operator *(Matrix m, Tuple4 t)
    {
        if (m.NumRows != 4 | m.NumCols != 4)
        {
            throw new MatrixCannotMultiplyException("matrix M is not a 4 * 4 matrix, " +
                                                    "which cannot multiply by a tuple4 " +
                                                    "and return a tuple4");
        }
        double x = m[0, 0] * t.X + m[0, 1] * t.Y + m[0, 2] * t.Z + m[0, 3] * t.W;
        double y = m[1, 0] * t.X + m[1, 1] * t.Y + m[1, 2] * t.Z + m[1, 3] * t.W;
        double z = m[2, 0] * t.X + m[2, 1] * t.Y + m[2, 2] * t.Z + m[2, 3] * t.W;
        double w = m[3, 0] * t.X + m[3, 1] * t.Y + m[3, 2] * t.Z + m[3, 3] * t.W;

        return new Tuple4(x, y, z, w);
    }

    public static Matrix operator /(Matrix m, double a)
    {
        double[,] res = new double[m.NumRows, m.NumCols];
        
        for (int i = 0; i < m.NumRows; i++)
        {
            for (int j = 0; j < m.NumCols; j++)
            {
                res[i, j] = m[i, j] / a;
            }
        }

        return new Matrix(res);
    }

    public bool Equals(Matrix other)
    {
        if (NumRows != other.NumRows | NumCols != other.NumCols)
        {
            return false;
        }
        for (int i = 0; i < NumRows; i++)
        {
            for (int j = 0; j < NumCols; j++)
            {
                if (!MathUtils.AlmostEqual(this[i, j], other[i, j]))
                {
                    return false;
                }
            }
        }
        return true;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is Matrix other && Equals(other);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(_values, NumRows, NumCols);
    }
    
    public static bool operator ==(Matrix a, Matrix b)
    {
        return a.Equals(b);
    }
    
    public static bool operator !=(Matrix a, Matrix b)
    {
        return !a.Equals(b);
    }
    
    // 单位矩阵
    public static Matrix Identity(int dim)
    {
        double[,] a = new double[dim, dim];
        for (int i = 0; i < dim; i++)
        {
            for (int j = 0; j < dim; j++)
            {
                if (i == j)
                {
                    a[i, j] = 1.0;
                }
                else
                {
                    a[i, j] = 0.0;
                }
            }
        }
        return new Matrix(a);
    }
    
    // 转置
    public Matrix Transpose()
    {
        double[,] res = new double[NumCols, NumRows];
        Matrix mT = new Matrix(res);

        for (int i = 0; i < NumRows; i++)
        {
            for (int j = 0; j < NumCols; j++)
            {
                mT[j, i] = _values[i, j];
            }
        }

        return mT;
    }
    
    // 矩阵A去掉第x行和第y列后的子矩阵
    private Matrix SubMatrix(int x, int y)
    {
        double[,] subMatrix = new double[NumRows - 1, NumCols - 1];

        int targetRow = 0;  // 子矩阵中要复制进去元素的目标行index
        int targetCol = 0;  // 子矩阵中要复制进去元素的目标列index
        
        for (int i = 0; i < NumRows; i++)
        {
            if (i == x) continue;
            
            for (int j = 0; j < NumCols; j++)
            {
                if (j == y) continue;
                subMatrix[targetRow, targetCol] = this[i, j];
                targetCol += 1;
            }

            targetCol = 0;
            targetRow += 1;
        }

        return new Matrix(subMatrix);
    }
    
    // 行列式
    public double Determinant()
    {
        switch (NumRows, NumCols)
        {
            case (1, 1):
                return this[0, 0];
            case (2, 2):
                return this[0, 0] * this[1, 1] - this[0, 1] * this[1, 0];
        }

        double det = 0.0;

        for (int col = 0; col < NumCols; col++)
        {
            Matrix subMatrix = SubMatrix(0, col);
            det += Math.Pow(-1, col) * this[0, col] * subMatrix.Determinant();
        }

        return det;
    }

    // 伴随矩阵
    private Matrix Adjugate()
    {
        double[,] res = new double[NumRows, NumCols];

        for (int i = 0; i < NumRows; i++)
        {
            for (int j = 0; j < NumCols; j++)
            {
                Matrix subMatrix = SubMatrix(i, j);  // 子矩阵
                double cofactor = Math.Pow(-1, i + j) * subMatrix.Determinant();  // A_ij代数余子式
                res[i, j] = cofactor;
            }
        }

        return new Matrix(res).Transpose();
    }

    // 逆矩阵
    public Matrix Inverse()
    {
        return Adjugate() / Determinant();
    }

    // 打印矩阵
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append('(');
        
        for (int i = 0; i < NumRows; i++)
        {
            StringBuilder line = new StringBuilder();
            line.Append('(');
            for (int j = 0; j < NumCols; j++)
            {
                // 如果列数为最后一列就不加空格
                line.Append(j == NumCols - 1 ? $"{_values[i, j]}" : $"{_values[i, j]}, ");
            }
            // 如果行数为最后一行就不加换行
            line.Append(i == NumRows - 1 ? ')' : "),\n");
            sb.Append(line);
        }
        
        sb.Append(')');
        return sb.ToString();
    }
}