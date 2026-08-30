namespace RayTracerChallenge.RayTracer;

public class World
{
    public Light Light { get; set; }
    public List<Shape> Shapes { get; set; }
    public Camera Camera { get; set; }
    
    public World(Light light, List<Shape> shapes, Camera camera)
    {
        Light = light;
        Shapes = shapes;
        Camera = camera;
    }

    /// <summary>
    /// 构建默认世界（用于测试）
    /// </summary>
    /// <returns>有两个球s1和s2的默认世界</returns>
    public static World Default()
    {
        // 光源
        var light = new Light(
            Tuple4.Point(-10, 10, -10), 
            new Color(1, 1, 1)
            );
        
        // 物体
        var s1 =  Sphere.Default();
        s1.Material = new Material(
            color:new Color(0.8, 1.0, 0.6), 
            diffuse:0.7, 
            specular:0.2);
        
        var s2 = new Sphere(Tuple4.Point(0, 0, 0),0.5);
        
        //相机
        var camera = new Camera(11, 11, Math.PI / 2);
        var from = Tuple4.Point(0, 0, -5);
        var to = Tuple4.Point(0, 0, 0);
        var up = Tuple4.Vector(0, 1, 0);
        camera.Transform = Transformations.ViewTransformation(from, to, up);
        
        return new World(light, new List<Shape> { s1, s2 }, camera);
    }

    /// <summary>
    /// 计算世界中所有物体与一道光线的交点
    /// </summary>
    /// <param name="ray">光线(Ray)</param>
    /// <returns>光线与世界中所有物体的交点</returns>
    public List<Intersection> IntersectWorld(Ray ray)
    {
        List<Intersection> xs = new List<Intersection>();
        
        foreach (var shape in Shapes)
        {
            Intersection[] localIntersections = shape.Intersect(ray);
            foreach (var intersection in localIntersections)
            {
                xs.Add(intersection);
            }
        }

        xs.Sort((x1, x2) => x1.T.CompareTo(x2.T));
        return xs;
    }

    /// <summary>
    /// 通过对一个交点的计算得到这个点的颜色
    /// </summary>
    /// <param name="comps">Computation</param>
    /// <returns>点的颜色(Color)</returns>
    public Color ShadeHit(Computation comps)
    {
        return Lighting(
            comps.Object.Material,
            this.Light,
            comps.Point,
            comps.EyeV,
            comps.NormalV);
    }

    /// <summary>
    /// 通过一个光线计算光线打到的世界上的一个点的颜色
    /// </summary>
    /// <param name="r">光线</param>
    /// <returns>光线与世界第一个交点的颜色(Color)</returns>
    public Color ColorAt(Ray r)
    {
        var xs = IntersectWorld(r);  // 已经按交点的t值从小到大排好的交点列表
        
        if (xs.Count == 0)
        {
            return new Color(0, 0, 0);  // 无交点返回黑色
        }

        xs.RemoveAll(i => i.T < 0);  // 删除所有t小于0的交点
        var compsHit = xs[0].PrepareComputations(r);
        var c = ShadeHit(compsHit);
        return c;
    }
    
    /// <summary>
    /// Phong 光照模型
    /// </summary>
    /// <param name="material">材质(Material)</param>
    /// <param name="light">光源(Light)</param>
    /// <param name="point">光线接触到的点(Tuple4)</param>
    /// <param name="eyeV">视线向量(Tuple4)</param>
    /// <param name="normalV">点对应的法向量(Tuple4)</param>
    /// <returns></returns>
    public static Color Lighting(
        Material material, 
        Light light, 
        Tuple4 point, 
        Tuple4 eyeV, 
        Tuple4 normalV)
    {
        Color black = new Color(0, 0, 0);
        Color diffuse;
        Color specular;
        
        // 结合材质颜色和光源颜色
        Color effectiveColor = material.Color * light.Intensity;

        // 找到交点到光源的向量
        Tuple4 lightV = (light.Position - point).Normalize();

        // 计算环境光对颜色的贡献
        Color ambient = effectiveColor * material.Ambient;

        // 入射光线和表面法向量的余弦相似度
        double lightDotNormal = lightV.Dot(normalV);
        
        // 相似度为负表示法向量和入射光方向相反（这个面不能被光照到），颜色为黑色
        if (lightDotNormal < 0)
        {
            diffuse = black;
            specular = black;
        }
        else  // 相似度不为负的情况
        {
            // 表面颜色乘上散射强度和余弦相似度，相似度越大表明这个面越靠近光源
            diffuse = effectiveColor * material.Diffuse * lightDotNormal;

            // reflect_dot_eye represents the cosine of the angle between the
            // reflection vector and the eye vector. A negative number means the
            // light reflects away from the eye.
            Tuple4 reflectV = (-lightV).Reflect(normalV);
            double reflectDotEye = reflectV.Dot(eyeV);

            if (reflectDotEye <= 0)
            {
                specular = black;
            }
            else
            {
                // compute the specular contribution
                double factor = Math.Pow(reflectDotEye, material.Shininess);
                specular = light.Intensity * material.Specular * factor;
            }
        }

        return ambient + diffuse + specular;
    }

    public Canvas Render()
    {
        Canvas canvas = new Canvas(Camera.HSize, Camera.VSize);

        Parallel.For(0, Camera.VSize, y => 
        {
            for (int x = 0; x < Camera.HSize; x++)
            {
                Ray ray = Camera.RayForPixel(x, y);
                Color color = ColorAt(ray);
                canvas.WritePixel(x, y, color);
            }
        });

        return canvas;
    }
}