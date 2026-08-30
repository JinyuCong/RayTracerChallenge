namespace RayTracerChallenge.RayTracer;

public class Camera
{
    public int HSize { get; set; }  // 画布水平宽度（像素）
    public int VSize { get; set; }  // 画布垂直高度（像素）
    public double FieldOfView { get; set; }  // FOV（角度）
    public Matrix Transform { get; set; } = Matrix.Identity(4); // 相机变换矩阵
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
    /// </summary>
    /// <param name="px">像素在画布上的横向位置</param>
    /// <param name="py">像素在画布上的纵向位置</param>
    /// <returns>穿过画布中这个像素的光线</returns>
    public Ray RayForPixel(int px, int py)
    {
        double xOffset = (px + 0.5) * PixelSize;  // 这个像素到画布左边的距离
        double yOffset = (py + 0.5) * PixelSize;  // 这个像素到画布上边的距离

        // 这个像素在世界中的坐标
        double worldX = HalfWidth - xOffset;  
        double worldY = HalfHeight - yOffset;
        
        // 像素在世界中坐标
        Tuple4 pixel = this.Transform.Inverse() * Tuple4.Point(worldX, worldY, -1);
        
        // 相机原点在世界中坐标
        Tuple4 origin = this.Transform.Inverse() * Tuple4.Point(0, 0, 0);
        
        // 光线方向
        Tuple4 direction = (pixel - origin).Normalize();

        return new Ray(origin, direction);

    }
}