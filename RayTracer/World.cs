using System.Transactions;

namespace RayTracerChallenge.RayTracer;

public class World
{
    public List<Light> Lights { get; set; }
    public List<Shape> Shapes { get; set; }
    public Camera Camera { get; set; }
    public Color Background { get; set; } = new Color(0, 0, 0);  // 光线什么都没打到时的颜色
    
    /// <summary>
    /// 初始化世界
    /// </summary>
    /// <param name="lights">光源列表</param>
    /// <param name="shapes">形状列表</param>
    /// <param name="camera">相机</param>
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
        var light = new PointLight(
            Tuple4.Point(-10, 10, -10),
            new Color(1, 1, 1)
            );
        
        // 物体
        var s1 = new Sphere();
        s1.Material = new Material(
            color: new Color(0.8, 1.0, 0.6), 
            diffuse: 0.7, 
            specular: 0.2);
        
        var s2 = new Sphere();
        s2.Transform = Transformations.Scaling(0.5, 0.5, 0.5);
        
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
        
        xs.Sort((a, b) => a.T.CompareTo(b.T));
        
        return xs;
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
    /// <returns></returns>
    public Color Lighting(
        Material material, 
        Shape obj,
        List<Light> lights, 
        Tuple4 point, 
        Tuple4 eyeV, 
        Tuple4 normalV)
    {
        Color black = new Color(0, 0, 0);
        Color totalDiffuseSpecular = new Color(0, 0, 0);
        Color color = material.Pattern is not null 
            ? material.Pattern.PatternAtShape(obj, point) 
            : material.Color;

        Color ambient = color * material.Ambient;

        foreach (var light in Lights)
        {
            Color sum = black;
            
            // 结合材质颜色和光源颜色
            Color effectiveColor = color * light.Intensity;

            for (int v = 0; v < light.VSteps; v++)
            for (int u = 0; u < light.USteps; u++)
            {
                var lightPos = light.PointOnLight(u, v);
                if (IsShadowed(lightPos, point)) continue;

                // 找到交点到光源的向量
                Tuple4 lightV = (lightPos - point).Normalize();
    
                // 入射光线和表面法向量的余弦相似度
                double lightDotNormal = lightV.Dot(normalV);
        
                // 相似度为负表示法向量和入射光方向相反（这个面不能被光照到），颜色为黑色
                if (lightDotNormal < 0) continue;

                // 表面颜色乘上散射强度和余弦相似度，相似度越大表明这个面越靠近光源
                sum += effectiveColor * material.Diffuse * lightDotNormal;

                // reflect_dot_eye represents the cosine of the angle between the
                // reflection vector and the eye vector. A negative number means the
                // light reflects away from the eye.
                Tuple4 reflectV = (-lightV).Reflect(normalV);
                double reflectDotEye = reflectV.Dot(eyeV);

                if (reflectDotEye > 0)
                {
                    // compute the specular contribution
                    double factor = Math.Pow(reflectDotEye, material.Shininess);
                    sum += light.Intensity * material.Specular * factor;
                }
            }

            totalDiffuseSpecular += sum / light.Samples;
        }
        return ambient + totalDiffuseSpecular;
    }
    
    /// <summary>
    /// 用于判断世界中的一个点是否被物体遮挡（在阴影中）
    /// </summary>
    /// <param name="lightPos">世界中的一个光源</param>
    /// <param name="point">世界中的一个点</param>
    /// <returns>在阴影中 — true，不在阴影中 — false</returns>
    public bool IsShadowed(Tuple4 lightPos, Tuple4 point)
    {
        // 光线向量
        Tuple4 v = lightPos - point;
        
        // 光源到这个点的距离
        double distance = v.Magnitude();
        
        // 光线方向为向量归一化
        Tuple4 direction = v.Normalize();
        
        Ray lightRay = new Ray(point, direction);
        
        // 计算光线和世界交点
        List<Intersection> intersections = IntersectWorld(lightRay)
            .Where(i => i.Object.CastsShadow)
            .ToList();
        
        Intersection? hit = Intersection.Hit(intersections);

        return hit is not null && hit.T < distance;
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

        Intersection? hitPoint = Intersection.Hit(xs);
        
        if (hitPoint is null)
        {
            return Background;  // 无交点返回背景色
        }
        
        var compsHit = hitPoint.PrepareComputations(r, xs);
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
    /// 计算交点折射光线能在这个点上叠加的颜色
    /// </summary>
    /// <param name="comps"></param>
    /// <param name="remaining">还剩多少次反射机会，防止两个镜面无限反射</param>
    /// <returns></returns>
    public Color RefractedColor(Computation comps, int remaining)
    {
        // 防止无限反射
        if (remaining <= 0)
        {
            return new Color(0, 0, 0);
        }
        // 透明度为0那折射叠加的颜色就是0（不叠加任何颜色）
        if (comps.Object.Material.Transparency == 0)
        {
            return new Color(0, 0, 0);
        }

        // 若入射角过大则只有全反射没有折射
        var nRatio = comps.N1 / comps.N2;
        var cosI = comps.EyeV.Dot(comps.NormalV);
        var sin2T = nRatio * nRatio * (1 - cosI * cosI);
        if (sin2T > 1)
        {
            return new Color(0, 0, 0);
        }
        
        // 找到折射在交点上叠加的颜色
        var cosT = Math.Sqrt(1.0 - sin2T);
        // 折射光线的方向
        var direction = comps.NormalV * (nRatio * cosI - cosT) - comps.EyeV * nRatio;
        // 折射光线
        var refractRay = new Ray(comps.UnderPoint, direction);
        return ColorAt(refractRay, remaining - 1) * comps.Object.Material.Transparency;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="comps"></param>
    /// <returns></returns>
    public static double Schlick(Computation comps)
    {
        // 视线和法线的余弦相似度
        var cos = comps.EyeV.Dot(comps.NormalV);
        
        // 只有介质1的密度大于介质2的密度时全反射才会发生（光从水里射向空气）
        if (comps.N1 > comps.N2)
        {
            var n = comps.N1 / comps.N2;
            var sin2T = n * n * (1.0 - cos * cos);
            if (sin2T > 1.0)
                return 1.0;

            var cosT = Math.Sqrt(1.0 - sin2T);
            cos = cosT;
        }

        var r0 = Math.Pow((comps.N1 - comps.N2) / (comps.N1 + comps.N2), 2);
        return r0 + (1 - r0) * Math.Pow(1 - cos, 5);
    }
    
    /// <summary>
    /// 通过对一个交点的计算得到这个点的颜色
    /// </summary>
    /// <param name="comps">Computation</param>
    /// <param name="remaining">还剩多少次反射机会，防止两个镜面无限反射</param>
    /// <returns>点的颜色(Color)</returns>
    public Color ShadeHit(Computation comps, int remaining)
    {
        Color surface = Lighting(  // 计算这个像素物体表面本来的颜色
            comps.Object.Material, comps.Object, 
            this.Lights, comps.OverPoint, comps.EyeV, 
            comps.NormalV);
        Color reflected = ReflectedColor(comps, remaining);  // 计算反射给这个点叠加的颜色
        Color refracted = RefractedColor(comps, remaining);  // 计算折射给这个点叠加的颜色

        var material = comps.Object.Material;
        if (material.Reflective > 0 && material.Transparency > 0)
        {
            var reflectance = Schlick(comps);
            return surface +
                   reflected * reflectance +
                   refracted * (1 - reflectance);
        }

        return surface + reflected + refracted;
    }

    /// <summary>
    /// 渲染世界
    /// </summary>
    /// <param name="remaining">每条视线最多允许的反射/折射次数，玻璃多的场景需要调大</param>
    /// <returns>画布</returns>
    public Canvas Render(int remaining = 5)
    {
        Canvas canvas = new Canvas(Camera.HSize, Camera.VSize);
        
        Parallel.For(0, Camera.VSize, y => 
        {
            for (int x = 0; x < Camera.HSize; x++)
            {
                Ray[] rays = Camera.RayForPixel(x, y);
                
                Color colorSum = new Color(0, 0, 0);
                foreach (var ray in rays)
                {
                    colorSum += ColorAt(ray, remaining);
                }

                Color colorAverage = colorSum / rays.Length;
                
                canvas.WritePixel(x, y, colorAverage);
            }
        });

        return canvas;
    }
}