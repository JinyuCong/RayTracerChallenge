namespace RayTracerChallenge.RayTracer;

public class Material
{
    public Color Color { get; set; }  // 材质颜色
    public double Ambient { get; set; }  // 环境光
    public double Diffuse { get; set; }  // 漫反射
    public double Specular { get; set; }  // 镜面反射
    public double Shininess { get; set; }  // 光泽度

    public Material(
        Color? color = null, 
        double ambient = 0.1, 
        double diffuse = 0.9, 
        double specular = 0.9, 
        double shininess = 200.0
        )
    {
        Color = color ?? new Color(1, 1, 1);
        Ambient = ambient;
        Diffuse = diffuse;
        Specular = specular;
        Shininess = shininess;
    }
}