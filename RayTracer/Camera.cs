namespace RayTracerChallenge.RayTracer;

public class Camera
{
    public int Hsize { get; set; }  // 画布水平宽度
    public int Vsize { get; set; }  // 画布垂直高度
    public int FieldOfView { get; set; }  // FOV
    public Matrix Transform { get; set; }  // 相机变换矩阵
    
    
}