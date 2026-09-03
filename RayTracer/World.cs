namespace RayTracerChallenge.RayTracer;

public class World
{
    public List<Light> Lights { get; set; }
    public List<Shape> Shapes { get; set; }
    public Camera Camera { get; set; }
    
    public World(List<Light> lights, List<Shape> shapes, Camera camera)
    {
        Lights = lights;
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
            color: new Color(0.8, 1.0, 0.6), 
            diffuse: 0.7, 
            specular: 0.2);
        
        var s2 = new Sphere(Tuple4.Point(0, 0, 0),0.5);
        
        //相机
        var camera = new Camera(11, 11, Math.PI / 2);
        var from = Tuple4.Point(0, 0, -5);
        var to = Tuple4.Point(0, 0, 0);
        var up = Tuple4.Vector(0, 1, 0);
        camera.Transform = Transformations.ViewTransformation(from, to, up);
        
        return new World(new List<Light> { light }, new List<Shape> { s1, s2 }, camera);
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
            List<Intersection> localIntersections = shape.Intersect(ray);
            foreach (var intersection in localIntersections)
            {
                xs.Add(intersection);
            }
        }

        xs.Sort((x1, x2) => x1.T.CompareTo(x2.T));
        return xs;
    }

    /// <summary>
    /// 拿所有和世界的交点中第一个和世界的交点
    /// </summary>
    /// <param name="xs">所有交点（已经按交点的t值从小到大排好）</param>
    /// <returns>第一个交点</returns>
    public Intersection? HitWorld(List<Intersection> xs)
    {
        xs.RemoveAll(i => i.T < 0);  // 删除所有t小于0的交点（在光线背后不算）
        
        if (xs.Count == 0)
        {
            return null;  // 无交点
        }
        
        return xs[0];  // 返回列表中第一个就是第一个交点
    }
    
    /// <summary>
    /// Phong 光照模型
    /// </summary>
    /// <param name="material">材质(Material)</param>
    /// <param name="obj">这道视线照到的物体</param>
    /// <param name="lights">所有光源(List[Light])</param>
    /// <param name="point">视线接触到的点(Tuple4)</param>
    /// <param name="eyeV">视线向量(Tuple4)</param>
    /// <param name="normalV">点对应的法向量(Tuple4)</param>
    /// <param name="shadowFlags">每个光源对应一个"是否被遮挡"的标记</param>
    /// <returns></returns>
    public static Color Lighting(
        Material material, 
        Shape obj,
        List<Light> lights, 
        Tuple4 point, 
        Tuple4 eyeV, 
        Tuple4 normalV,
        List<bool> shadowFlags)
    {
        Color black = new Color(0, 0, 0);
        Color totalDiffuseSpecular = new Color(0, 0, 0);
        Color color = (material.Pattern is not null) ? material.Pattern.PatternAtShape(obj, point) : material.Color;

        Color ambient = color * material.Ambient;

        for (int i = 0; i < lights.Count; i++)
        {
            var light = lights[i];
            bool inShadow = shadowFlags[i];

            if (inShadow)
            {
                continue;
            }
            
            // 结合材质颜色和光源颜色
            Color effectiveColor = color * light.Intensity;

            // 找到交点到光源的向量
            Tuple4 lightV = (light.Position - point).Normalize();

            // 入射光线和表面法向量的余弦相似度
            double lightDotNormal = lightV.Dot(normalV);
            
            Color diffuse;
            Color specular;
        
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

            totalDiffuseSpecular += diffuse + specular;
        }
        
        return ambient + totalDiffuseSpecular;
    }
    
    /// <summary>
    /// 用于判断世界中的一个点是否被物体遮挡（在阴影中）
    /// </summary>
    /// <param name="light">世界中的一个光源</param>
    /// <param name="point">世界中的一个点</param>
    /// <returns>在阴影中 — true，不在阴影中 — false</returns>
    public bool IsShadowed(Light light, Tuple4 point)
    {
        // 光线向量
        Tuple4 v = light.Position - point;
        
        // 光源到这个点的距离
        double distance = v.Magnitude();
        
        // 光线方向为向量归一化
        Tuple4 direction = v.Normalize();
        
        Ray lightRay = new Ray(point, direction);
        
        // 计算光线和世界交点
        List<Intersection> intersections = IntersectWorld(lightRay);
        Intersection? hit = HitWorld(intersections);

        return (hit is not null && hit.T < distance);
    }

    /// <summary>
    /// 计算视线打到的世界上的一个点的颜色
    /// </summary>
    /// <param name="r">视线</param>
    /// <param name="remaining">还剩多少次反射机会，防止两个镜面无限反射</param>
    /// <returns>视线与世界第一个交点的颜色(Color)</returns>
    public Color ColorAt(Ray r, int remaining)
    {
        var xs = IntersectWorld(r);  // 已经按交点的t值从小到大排好的交点列表

        Intersection? hitPoint = HitWorld(xs);
        
        if (hitPoint is null)
        {
            return new Color(0, 0, 0);  // 无交点返回黑色
        }
        
        var compsHit = hitPoint.PrepareComputations(r);
        return ShadeHit(compsHit, remaining);
    }
    
    /// <summary>
    /// 计算交点反光能在这个点上叠加的颜色
    /// </summary>
    /// <param name="comps"></param>
    /// <param name="remaining">还剩多少次反射机会，防止两个镜面无限反射</param>
    /// <returns></returns>
    public Color ReflectedColor(Computation comps, int remaining)
    {
        if (remaining <= 0)
        {
            return new Color(0, 0, 0);
        }
        
        if (comps.Object.Material.Reflective == 0)
        {
            return new Color(0, 0, 0);
        }

        // 计算视线和物体交点的反射光线
        var reflectRay = new Ray(comps.OverPoint, comps.ReflectV);
        
        // 计算反射光线在交点上叠加的颜色
        var color = ColorAt(reflectRay, remaining - 1);
        return color * comps.Object.Material.Reflective;
    }
    
    /// <summary>
    /// 通过对一个交点的计算得到这个点的颜色
    /// </summary>
    /// <param name="comps">Computation</param>
    /// <param name="remaining">还剩多少次反射机会，防止两个镜面无限反射</param>
    /// <returns>点的颜色(Color)</returns>
    public Color ShadeHit(Computation comps, int remaining)
    {
        List<bool> shadowFlags = new List<bool> ();  // 这个点对于世界中所有光源是否处在阴影中的列表

        foreach (var light in Lights)
        {
            bool inShadow = IsShadowed(light, comps.OverPoint);
            shadowFlags.Add(inShadow);
        }
            
        Color surface = Lighting(  // 计算这个像素物体表面本来的颜色
            comps.Object.Material, comps.Object, 
            this.Lights, comps.OverPoint, comps.EyeV, 
            comps.NormalV, shadowFlags);
        Color reflected = ReflectedColor(comps, remaining);  // 计算反光给这个像素叠加的颜色
        return surface + reflected;
    }

    /// <summary>
    /// 渲染世界
    /// </summary>
    /// <returns>画布</returns>
    public Canvas Render()
    {
        Canvas canvas = new Canvas(Camera.HSize, Camera.VSize);
        
        Parallel.For(0, Camera.VSize, y => 
        {
            for (int x = 0; x < Camera.HSize; x++)
            {
                int remaining = 4;
                Ray ray = Camera.RayForPixel(x, y);
                Color color = ColorAt(ray, remaining);
                canvas.WritePixel(x, y, color);
            }
        });

        return canvas;
    }
}