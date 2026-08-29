using System.Diagnostics.Tracing;

namespace RayTracerChallenge.Tests;

using RayTracer;

public static class Challenges
{
    private static Projectile Tick(Environment env, Projectile proj)
    {
        var pos = proj.Position + proj.Velocity;
        var vel = proj.Velocity + env.Gravity + env.Wind;
        return new Projectile(pos, vel);
    }
    
    public static void ProjectileTrajectory()
    {
        var start = Tuple4.Point(0, 1, 0);
        var velocity = Tuple4.Vector(1, 1.8, 0).Normalize() * 11.25;
        var p = new Projectile(start, velocity);

        var gravity = Tuple4.Vector(0, -0.1, 0);
        var wind = Tuple4.Vector(-0.01, 0, 0);
        var e = new Environment(gravity, wind);

        var c = new Canvas(900, 550);

        Color red = new Color(1, 0, 0);
        
        for (int i = 0; i < 1000; i++)
        {
            var pos = p.Position;
            int x = (int)pos.X; 
            int y = c.Height - 1 - (int)pos.Y;
            
            var newProj = Tick(e, p);
            p = newProj;

            if (x < c.Width && x >= 0 && y < c.Height && y >= 0)
            {
                c.WritePixel(x, y, red);
            }

        }
        
        c.SavePpm("./projectile.ppm");
    }

    public static void Clock()
    {
        int w = 90;
        int h = 90;
        var canvas = new Canvas(w, h);
        double radius = 3.0 / 8.0 * w;
        
        var p = Tuple4.Point(0, 0, 0); // 点
        var T = Transformations.Translation(0, 0, 1);  // 平移
        var S = Transformations.Scaling(0, 0, radius);  // 缩放 
        var rot30 = Transformations.RotationY(30);  // 旋转
        p = S * T * p;  // 将点放到(0, 0, 33.75) 十二点位置
        
        var white = new Color(1, 1, 1);
        
        canvas.WritePixel((int)p.X + 45, (int)p.Z + 45, white);
        
        for (int i = 0; i < 12; i++)
        {
            p = rot30 * p;  // 每次旋转30°
            canvas.WritePixel((int)p.X+45, (int)p.Z+45, white);
        }
        
        canvas.SavePpm("./clock.ppm");
    }

    public static void SilhouetteSphere()
    {
        var rayOrigin = Tuple4.Point(0, 0, -5);
        double wallZ = 10.0;
        double wallSize = 7.0;

        int canvasPixels = 100;
        double pixelSize = wallSize / canvasPixels;
        double half = wallSize / 2;

        Canvas canvas = new Canvas(canvasPixels, canvasPixels);
        Color color = new Color(1, 0, 0);
        Sphere sphere = Sphere.Default();

        sphere.Transform = Transformations.Translation(0.3, -0.4, 0) * Transformations.Scaling(0.5, 1, 1);
        
        for (int y = 0; y < canvasPixels; y++)
        {
            double worldY = half - pixelSize * y;  // 计算这个像素在世界中的y坐标
            for (int x = 0; x < canvasPixels; x++)
            {
                double worldX = -half + pixelSize * x;  // 计算这个像素在世界中的x坐标
                var position = Tuple4.Point(worldX, worldY, wallZ);

                Ray r = new Ray(rayOrigin, (position - rayOrigin).Normalize());
                Intersection[] xs = sphere.Intersect(r);

                Intersection? intersection = sphere.Hit(xs);
                if (intersection is not null)
                {
                    canvas.WritePixel(x, y, color);
                }
            }
            canvas.SavePng("./silhouette_sphere.png");
        }
    }

    public static void LightAndShadingSphere()
    {
        // 初始化球
        Sphere shape = Sphere.Default();
        shape.Material.Color = new Color(1, 0.2, 1);

        // 初始化光源
        var lightPosition = Tuple4.Point(-10, 10, -10);
        var lightColor = new Color(1, 1, 1);
        var light = new Light(lightPosition, lightColor);
        
        // 初始化画布
        int canvasPixels = 500;
        Canvas canvas = new Canvas(canvasPixels, canvasPixels);
        double wallSize = 4;
        double wallZ = 3;
        double pixelSize = wallSize / canvasPixels;
        double half = wallSize / 2;
        
        // 初始化视线原点
        var rayOrigin = Tuple4.Point(0, 0, -5);

        for (int y = 0; y < canvas.Height; y++)
        {
            double worldY = half - pixelSize * y;  // 计算这个像素在世界中的y坐标
            for (int x = 0; x < canvas.Width; x++)
            {
                double worldX = -half + pixelSize * x;  // 计算这个像素在世界中的x坐标

                var position = Tuple4.Point(worldX, worldY, wallZ);  // 这个像素在世界中的坐标
                var ray = new Ray(rayOrigin, (position - rayOrigin).Normalize());  // 这个像素到视线原点的光线

                Intersection[] xs = shape.Intersect(ray);
                Intersection? intersection = shape.Hit(xs);
                if (intersection is not null)
                {
                    var hitPoint = ray.Position(intersection.T);
                    var normal = shape.NormalAt(hitPoint);
                    var eyeV = -ray.Direction;

                    var color = World.Lighting(shape.Material, light, hitPoint, eyeV, normal);
                    canvas.WritePixel(x, y, color);
                }
            }
        }
        canvas.SavePng("./light_and_shading_sphere.png");
    }
}