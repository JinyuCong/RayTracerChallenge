namespace RayTracerChallenge.RayTracer;

public class Camera
{
    public int HSize { get; set; }  // 画布水平宽度（像素）
    public int VSize { get; set; }  // 画布垂直高度（像素）
    public double FieldOfView { get; set; }  // FOV（角度）
    private Matrix _inverseTransform;
    private Matrix _transform;
    public Matrix Transform
    {
        get => _transform;
        set { _transform = value; _inverseTransform = value.Inverse(); }
    }

    public Matrix InverseTransform => _inverseTransform;

    public double HalfWidth { get; set; }  // 画布宽度的一半（世界坐标）
    public double HalfHeight { get; set; }  // 画布高度的一半（世界坐标）
    public double PixelSize { get; set; }  // 像素点的大小（世界坐标）
    

    /// <summary>
    /// 初始化相机
    /// </summary>
    /// <param name="hSize">画布水平宽度（像素）</param>
    /// <param name="vSize">画布垂直高度（像素）</param>
    /// <param name="fov">视野角度</param>
    public Camera(int hSize, int vSize, double fov)
    {
        HSize = hSize;
        VSize = vSize;
        FieldOfView = fov;
        ComputeSize();
        Transform = Matrix.Identity(4);
    }

    private void ComputeSize()
    {
        double halfView = Math.Tan(FieldOfView / 2);
        double aspect = (double)HSize / (double)VSize;

        double halfWidth;
        double halfHeight;
        
        if (aspect > 1)
        {
            halfWidth = halfView;
            halfHeight = halfView / aspect;
        }
        else
        {
            halfWidth = halfView * aspect;
            halfHeight = halfView;
        }

        HalfWidth = halfWidth;
        HalfHeight = halfHeight;
        PixelSize = (halfWidth * 2) / HSize;
    }

    /// <summary>
    /// 返回穿过画布中某个像素的光线（原点，方向）
    /// 使用上采样抗锯齿，将像素分为 n * n 网格，向每个网格中心坐标发射光线
    /// </summary>
    /// <param name="px">像素在画布上的横向位置</param>
    /// <param name="py">像素在画布上的纵向位置</param>
    /// <param name="n">将这个像素分为多少行和列子像素</param>
    /// <returns>穿过画布中这个像素的光线</returns>
    public Ray[] RayForPixel(int px, int py, int n = 2)
    {
        double subPixelSize = PixelSize / n;  // 子像素边长

        Ray[] rays = new Ray[n * n];
        
        // 相机原点在世界中坐标
        Tuple4 origin = InverseTransform * Tuple4.Point(0, 0, 0);

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                double xOffset = px * PixelSize + (j + 0.5) * subPixelSize;
                double yOffset = py * PixelSize + (i + 0.5) * subPixelSize;
                
                // 这个子像素中心在世界中的坐标
                double middleX = HalfWidth - xOffset;  
                double middleY = HalfHeight - yOffset;
                
                Tuple4 subPixelMiddle = InverseTransform * Tuple4.Point(middleX, middleY, -1);

                Tuple4 direction = (subPixelMiddle - origin).Normalize();
                Ray subPixelRay = new Ray(origin, direction);

                rays[i * n + j] = subPixelRay;
            }
        }

        return rays;
    }
}