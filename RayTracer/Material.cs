namespace RayTracerChallenge.RayTracer;

public class Material
{
    public Color Color { get; set; }
    public double Ambient { get; set; }
    public double Diffuse { get; set; }
    public double Specular { get; set; } 
    public double Shininess { get; set; }
    public Pattern? Pattern { get; set; }
    public double Reflective { get; set; }
    public double Transparency { get; set; }
    public double RefractiveIndex { get; set; }

    /// <summary>
    /// 初始化材质
    /// </summary>
    /// <param name="color">颜色</param>
    /// <param name="ambient">环境光强度</param>
    /// <param name="diffuse">漫反射强度</param>
    /// <param name="specular">镜面反射强度</param>
    /// <param name="shininess">光泽度</param>
    /// <param name="pattern">图案</param>
    /// <param name="reflective">反射率</param>
    /// <param name="transparency">透明度</param>
    /// <param name="refractiveIndex">折射率</param>
    public Material(
        Color? color = null, 
        double ambient = 0.1, 
        double diffuse = 0.9, 
        double specular = 0.9, 
        double shininess = 200.0,
        Pattern? pattern = null,
        double reflective = 0.0,
        double transparency = 0.0,
        double refractiveIndex = 1.0)
    {
        Color = color ?? new Color(1, 1, 1);
        Ambient = ambient;
        Diffuse = diffuse;
        Specular = specular;
        Shininess = shininess;
        Pattern = pattern;
        Reflective = reflective;
        Transparency = transparency;
        RefractiveIndex = refractiveIndex;
    }
}