namespace RayTracerChallenge.Tests;

using RayTracer;

public class Environment
{
    public Tuple4 Gravity { get; set; }
    public Tuple4 Wind { get; set; }

    public Environment(Tuple4 gravity, Tuple4 wind)
    {
        Gravity = gravity;
        Wind = wind;
    }
}

public class Projectile
{
    public Tuple4 Position { get; set; }
    public Tuple4 Velocity { get; set; }

    public Projectile(Tuple4 position, Tuple4 velocity)
    {
        Position = position;
        Velocity = velocity;
    }
}

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
                List<Intersection> xs = sphere.Intersect(r);

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

                List<Intersection> xs = shape.Intersect(ray);
                Intersection? intersection = shape.Hit(xs);
                if (intersection is not null)
                {
                    var hitPoint = ray.Position(intersection.T);
                    var normal = shape.NormalAt(hitPoint);
                    var eyeV = -ray.Direction;

                    var color = World.Lighting(shape.Material, shape, new List<Light>{ light }, hitPoint, eyeV, normal, new List<bool>());
                    canvas.WritePixel(x, y, color);
                }
            }
        }
        canvas.SavePng("./light_and_shading_sphere.png");
    }

    public static void MakingAScene()
    {
        // 地面（拍扁的球）
        var floor = Sphere.Default();
        floor.Transform = Transformations.Scaling(10, 0.01, 10);
        floor.Material = new Material(color: new Color(1, 0.9, 0.9), specular: 0);
        
        // 左边的墙
        var leftWall = Sphere.Default();
        leftWall.Transform = Transformations.Translation(0, 0, 5) *
                             Transformations.RotationY(-45) *
                             Transformations.RotationX(90) *
                             Transformations.Scaling(10, 0.01, 10);
        leftWall.Material = floor.Material;

        // 右边的墙
        var rightWall = Sphere.Default();
        rightWall.Transform = Transformations.Translation(0, 0, 5) *
                              Transformations.RotationY(45) *
                              Transformations.RotationX(90) *
                              Transformations.Scaling(10, 0.01, 10);
        rightWall.Material = floor.Material;

        // 中间的球
        var middle = Sphere.Default();
        middle.Transform = Transformations.Translation(-0.5, 1, 0.5);
        middle.Material = new Material(
            color: new Color(0.1, 1, 0.5),
            diffuse: 0.7,
            specular: 0.3);
        
        // 右边的球
        var right = Sphere.Default();
        right.Transform = Transformations.Translation(1.5, 0.5, -0.5) * 
                          Transformations.Scaling(0.5, 0.5, 0.5);
        right.Material = new Material(
            color: new Color(0.5, 1, 0.1),
            diffuse: 0.7,
            specular: 0.3);
        
        // 左边的球
        var left = Sphere.Default();
        left.Transform = Transformations.Translation(-1.5, 0.33, -0.75) *
                         Transformations.Scaling(0.33, 0.33, 0.33);
        left.Material = new Material(
            color: new Color(1, 0.8, 0.1),
            diffuse: 0.7,
            specular: 0.3);
        
        // 构建场景
        var shapes = new List<Shape> { floor, leftWall, rightWall, left, middle, right };
        
        var camera = new Camera(1000, 500, Math.PI / 3);
        var cameraTransform = Transformations.ViewTransformation(
            Tuple4.Point(0, 1.5, -5), 
            Tuple4.Point(0, 1, 0), 
            Tuple4.Vector(0, 1, 0));
        camera.Transform = cameraTransform;
        
        var light1 = new Light(Tuple4.Point(-10, 10, -10), new Color(1, 1, 1));
        
        var world = new World(new List<Light> { light1 }, shapes, camera);
        var canvas = world.Render();
        canvas.SavePng("./making_a_scene_with_shadow.png");
    }

    public static void BarkBark()
    {
        // 背景墙（x = 2）
        var background = Sphere.Default();
        background.Transform = Transformations.Translation(0, 5, 5) *
                               Transformations.Scaling(0.01, 50, 50);
        background.Material = new Material(color: new Color(0.5, 0.5, 0.5), specular: 0);

        // 青色球（画面左侧）
        var ball1 = Sphere.Default();
        ball1.Transform = Transformations.Translation(5, 4, 2) *
                          Transformations.Scaling(1, 1, 1);
        ball1.Material = new Material(color: new Color(0, 0.9, 0.9), specular: 0.3);
        
        // 蓝
        var ball2 = Sphere.Default();
        ball2.Transform = Transformations.Translation(5, 5, 3) *
                          Transformations.Scaling(0.5, 0.7, 1);
        ball2.Material = new Material(color: new Color(0.1, 0.1, 0.7), specular: 0.3);
        
        // 黄
        var ball3 = Sphere.Default();
        ball3.Transform = Transformations.Translation(5, 5.9, 3) *
                          Transformations.RotationX(90) *
                          Transformations.Scaling(0.2, 0.2, 0.5);
        ball3.Material = new Material(color: new Color(1, 0.9, 0), specular: 0.3);
        
        // 绿
        var ball4 = Sphere.Default();
        ball4.Transform = Transformations.Translation(5, 5.7, 4) *
                          Transformations.RotationX(-30) *
                          Transformations.Scaling(0.2, 0.2, 0.5);
        ball4.Material = new Material(color: new Color(0.2, 0.9, 0.3), specular: 0.3);
        
        // 白
        var ball5 = Sphere.Default();
        ball5.Transform = Transformations.Translation(5, 5.2, 4.3) *
                          Transformations.RotationX(-20) *
                          Transformations.Scaling(0.2, 0.2, 0.5);
        ball5.Material = new Material(color: new Color(1, 1, 1), specular: 0.3);
        
        // 红
        var ball6 = Sphere.Default();
        ball6.Transform = Transformations.Translation(5, 5, 4.3) *
                          Transformations.Scaling(0.2, 0.2, 0.5);
        ball6.Material = new Material(color: new Color(1, 0, 0), specular: 0.3);

        // 光源
        var light = new Light(Tuple4.Point(9, 3.5, 0), new Color(1, 1, 1));

        var camera = new Camera(1000, 500, Math.PI / 2.5);
        camera.Transform = Transformations.ViewTransformation(
            Tuple4.Point(13, 5, 5),
            Tuple4.Point(2, 5, 5),
            Tuple4.Vector(0, 1, 0));

        var w = new World(
            new List<Light> { light },
            new List<Shape> { background, ball1, ball2, ball3, ball4, ball5, ball6 },
            camera);

        w.Render().SavePng("./bark_bark.png");
    }
    
    public static void MakingASceneWithPlane()
    {
        var pattern = new RadialGradientPattern(new Color(1, 0, 0), new Color(0, 0, 1));
        pattern.Transform = Transformations.Scaling(0.1, 0.1, 0.1);
        
        // 地面（平面）
        var floor = new Plane();
        floor.Material = new Material(color: new Color(1, 0.9, 0.9), specular: 0, pattern: pattern);
        
        // 中间的墙
        var middleWall = new Plane();
        middleWall.Transform = Transformations.Translation(0, 0, 1) *
                               Transformations.RotationX(-90);
        middleWall.Material = floor.Material;
        
        // 左边的墙
        var leftWall = new Plane();
        leftWall.Transform = Transformations.Translation(0, 0, 2) *
                             Transformations.RotationY(-30) *
                             Transformations.RotationX(-90);
        leftWall.Material = floor.Material;
        
        // 右边的墙
        var rightWall = new Plane();
        rightWall.Transform = Transformations.Translation(0, 0, 2) *
                              Transformations.RotationY(30) *
                              Transformations.RotationX(-90);
        rightWall.Material = floor.Material;
        
        // 天花板
        var ceil = new Plane();
        ceil.Transform = Transformations.Translation(0, 3, 0) * 
                         Transformations.RotationX(-180);
        ceil.Material = floor.Material;

        // 中间的球
        var middle = Sphere.Default();
        middle.Transform = Transformations.Translation(-0.5, 1, 0.5);
        middle.Material = new Material(
            color: new Color(0.1, 1, 0.5),
            diffuse: 0.7,
            specular: 0.3,
            pattern: pattern);
        
        // 右边的球
        var right = Sphere.Default();
        right.Transform = Transformations.Translation(1.5, 0.5, -0.5) * 
                          Transformations.Scaling(0.5, 0.5, 0.5);
        right.Material = new Material(
            color: new Color(0.5, 1, 0.1),
            diffuse: 0.7,
            specular: 0.3);
        
        // 左边的球
        var left = Sphere.Default();
        left.Transform = Transformations.Translation(-1.5, 0.33, -0.75) *
                         Transformations.Scaling(0.33, 0.33, 0.33);
        left.Material = new Material(
            color: new Color(1, 0.8, 0.1),
            diffuse: 0.7,
            specular: 0.3);
        
        // 构建场景
        var shapes = new List<Shape> { floor, leftWall, middleWall, rightWall, ceil, left, middle, right };
        
        var camera = new Camera(1000, 500, Math.PI / 3);
        var cameraTransform = Transformations.ViewTransformation(
            Tuple4.Point(0, 1.5, -5), 
            Tuple4.Point(0, 1, 0), 
            Tuple4.Vector(0, 1, 0));
        camera.Transform = cameraTransform;
        
        var light1 = new Light(Tuple4.Point(-3, 2, -5), new Color(1, 1, 1));
        
        var world = new World(new List<Light> { light1 }, shapes, camera);
        var canvas = world.Render();
        canvas.SavePng("./making_a_scene_with_shadow_and_plane.png");
    }
    
    
    public static void PuttingPatterns()
    {
        var pattern = new RadialGradientPattern(new Color(1, 0, 0), new Color(0, 0, 1));
        
        // 地面（平面）
        var floor = new Plane();
        floor.Material = new Material(color: new Color(1, 0.9, 0.9), specular: 0, pattern: pattern);

        // 中间的球
        var middle = Sphere.Default();
        middle.Transform = Transformations.Translation(-0.5, 1, 0.5);
        middle.Material = new Material(
            color: new Color(0.1, 1, 0.5),
            diffuse: 0.7,
            specular: 0.3,
            pattern: pattern);
        
        // 右边的球
        var right = Sphere.Default();
        right.Transform = Transformations.Translation(1.5, 0.5, -0.5) * 
                          Transformations.Scaling(0.5, 0.5, 0.5);
        right.Material = new Material(
            color: new Color(0.5, 1, 0.1),
            diffuse: 0.7,
            specular: 0.3);
        
        // 左边的球
        var left = Sphere.Default();
        left.Transform = Transformations.Translation(-1.5, 0.33, -0.75) *
                         Transformations.Scaling(0.33, 0.33, 0.33);
        left.Material = new Material(
            color: new Color(1, 0.8, 0.1),
            diffuse: 0.7,
            specular: 0.3);
        
        // 构建场景
        var shapes = new List<Shape> { floor, left, middle, right };
        
        var camera = new Camera(1000, 500, Math.PI / 3);
        var cameraTransform = Transformations.ViewTransformation(
            Tuple4.Point(0, 1.5, -5), 
            Tuple4.Point(0, 1, 0), 
            Tuple4.Vector(0, 1, 0));
        camera.Transform = cameraTransform;
        
        var light1 = new Light(Tuple4.Point(-3, 2, -5), new Color(1, 1, 1));
        
        var world = new World(new List<Light> { light1 }, shapes, camera);
        var canvas = world.Render();
        canvas.SavePng("./making_a_scene_with_patterns.png");
    }
}