namespace RayTracerChallenge.RayTracer;

public class Light
{
    public Tuple4 Position { get; set; }  // 光源位置
    public Color Intensity { get; set; }  // 光源颜色

    public Light(Tuple4 position, Color intensity)
    {
        Position = position;
        Intensity = intensity;
    }
}