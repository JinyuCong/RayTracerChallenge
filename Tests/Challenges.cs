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
        Sphere sphere = new Sphere();

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

                Intersection? intersection = Intersection.Hit(xs);
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
        Sphere shape = new Sphere();
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
                Intersection? intersection = Intersection.Hit(xs);
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
        var floor = new Sphere();
        floor.Transform = Transformations.Scaling(10, 0.01, 10);
        floor.Material = new Material(color: new Color(1, 0.9, 0.9), specular: 0);
        
        // 左边的墙
        var leftWall = new Sphere();
        leftWall.Transform = Transformations.Translation(0, 0, 5) *
                             Transformations.RotationY(-45) *
                             Transformations.RotationX(90) *
                             Transformations.Scaling(10, 0.01, 10);
        leftWall.Material = floor.Material;

        // 右边的墙
        var rightWall = new Sphere();
        rightWall.Transform = Transformations.Translation(0, 0, 5) *
                              Transformations.RotationY(45) *
                              Transformations.RotationX(90) *
                              Transformations.Scaling(10, 0.01, 10);
        rightWall.Material = floor.Material;

        // 中间的球
        var middle = new Sphere();
        middle.Transform = Transformations.Translation(-0.5, 1, 0.5);
        middle.Material = new Material(
            color: new Color(0.1, 1, 0.5),
            diffuse: 0.7,
            specular: 0.3);
        
        // 右边的球
        var right = new Sphere();
        right.Transform = Transformations.Translation(1.5, 0.5, -0.5) * 
                          Transformations.Scaling(0.5, 0.5, 0.5);
        right.Material = new Material(
            color: new Color(0.5, 1, 0.1),
            diffuse: 0.7,
            specular: 0.3);
        
        // 左边的球
        var left = new Sphere();
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
        var background = new Sphere();
        background.Transform = Transformations.Translation(0, 5, 5) *
                               Transformations.Scaling(0.01, 50, 50);
        background.Material = new Material(color: new Color(0.5, 0.5, 0.5), specular: 0);

        // 青色球（画面左侧）
        var ball1 = new Sphere();
        ball1.Transform = Transformations.Translation(5, 4, 2) *
                          Transformations.Scaling(1, 1, 1);
        ball1.Material = new Material(color: new Color(0, 0.9, 0.9), specular: 0.3);
        
        // 蓝
        var ball2 = new Sphere();
        ball2.Transform = Transformations.Translation(5, 5, 3) *
                          Transformations.Scaling(0.5, 0.7, 1);
        ball2.Material = new Material(color: new Color(0.1, 0.1, 0.7), specular: 0.3);
        
        // 黄
        var ball3 = new Sphere();
        ball3.Transform = Transformations.Translation(5, 5.9, 3) *
                          Transformations.RotationX(90) *
                          Transformations.Scaling(0.2, 0.2, 0.5);
        ball3.Material = new Material(color: new Color(1, 0.9, 0), specular: 0.3);
        
        // 绿
        var ball4 = new Sphere();
        ball4.Transform = Transformations.Translation(5, 5.7, 4) *
                          Transformations.RotationX(-30) *
                          Transformations.Scaling(0.2, 0.2, 0.5);
        ball4.Material = new Material(color: new Color(0.2, 0.9, 0.3), specular: 0.3);
        
        // 白
        var ball5 = new Sphere();
        ball5.Transform = Transformations.Translation(5, 5.2, 4.3) *
                          Transformations.RotationX(-20) *
                          Transformations.Scaling(0.2, 0.2, 0.5);
        ball5.Material = new Material(color: new Color(1, 1, 1), specular: 0.3);
        
        // 红
        var ball6 = new Sphere();
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
        var middle = new Sphere();
        middle.Transform = Transformations.Translation(-0.5, 1, 0.5);
        middle.Material = new Material(
            color: new Color(0.1, 1, 0.5),
            diffuse: 0.7,
            specular: 0.3,
            pattern: pattern);
        
        // 右边的球
        var right = new Sphere();
        right.Transform = Transformations.Translation(1.5, 0.5, -0.5) * 
                          Transformations.Scaling(0.5, 0.5, 0.5);
        right.Material = new Material(
            color: new Color(0.5, 1, 0.1),
            diffuse: 0.7,
            specular: 0.3);
        
        // 左边的球
        var left = new Sphere();
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
        var middle = new Sphere();
        middle.Transform = Transformations.Translation(-0.5, 1, 0.5);
        middle.Material = new Material(
            color: new Color(0.1, 1, 0.5),
            diffuse: 0.7,
            specular: 0.3,
            pattern: pattern);
        
        // 右边的球
        var right = new Sphere();
        right.Transform = Transformations.Translation(1.5, 0.5, -0.5) * 
                          Transformations.Scaling(0.5, 0.5, 0.5);
        right.Material = new Material(
            color: new Color(0.5, 1, 0.1),
            diffuse: 0.7,
            specular: 0.3);
        
        // 左边的球
        var left = new Sphere();
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
    
    public static void PuttingReflections()
    {
        // 地面（平面，0.5反射率）
        var floor = new Plane();
        var pattern = new CheckerPattern(new Color(0.2, 0.2, 0.2), new Color(1, 1, 1));
        floor.Material = new Material(
            color: new Color(1, 0.9, 0.9), 
            specular: 0, reflective: 0.5, pattern: pattern);

        // 中间的球（0.5反射率）
        var middle = new Sphere();
        middle.Transform = Transformations.Translation(-0.5, 1, 0.5);
        middle.Material = new Material(
            color: new Color(0.1, 1, 0.5),
            diffuse: 0.7,
            specular: 0.3,
            reflective: 0.5);
        
        // 右边的球
        var right = new Sphere();
        right.Transform = Transformations.Translation(1.5, 0.5, -0.5) * 
                          Transformations.Scaling(0.5, 0.5, 0.5);
        right.Material = new Material(
            color: new Color(0.5, 1, 0.1),
            diffuse: 0.7,
            specular: 0.3);
        
        // 左边的球
        var left = new Sphere();
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
        canvas.SavePng("./reflection_scene.png");
    }
    
    public static void PuttingRefractions()
    {
        var floor = new Plane();
        floor.Material.Pattern = new CheckerPattern(new Color(0.7, 0.2, 0.2), new Color(1, 1, 1));
        floor.Material.Reflective = 0.5;
        
        var wall = new Plane();
        wall.Transform = Transformations.Translation(-3, 0, -3) * 
                         Transformations.RotationY(45) *
                         Transformations.RotationX(-90);
        var wallPattern = new GradientPattern(new (0, 0.8, 0.6), new(0.8, 0.6, 1));
        wallPattern.Transform = Transformations.Scaling(15, 1, 1);
        wall.Material.Pattern = wallPattern;
        
        var ball1 = new Sphere();
        ball1.Transform = Transformations.Translation(4, 2, 4) *
                          Transformations.Scaling(2, 2, 2);
        ball1.Material = new Material(
            ambient: 0,
            diffuse: 0,
            specular: 0.9,
            transparency: 0.9,
            reflective: 0.9,
            shininess: 300,
            refractiveIndex: 1.5);
        ball1.CastsShadow = false;
        
        var ball2 = new Sphere();
        ball2.Transform = Transformations.Translation(5, 1, 1);
        ball2.Material = new Material(
            color: new Color(0, 0.7, 0.6),
            ambient: 0.2,
            diffuse: 0,
            specular: 0.9,
            transparency: 0.9,
            reflective: 0.9,
            shininess: 300,
            refractiveIndex: 1.5);
        ball2.CastsShadow = false;
        
        var ball3 = new Sphere();
        ball3.Transform = Transformations.Translation(-1, 3, 1) *
                          Transformations.Scaling(3, 3, 3);
        ball3.Material = new Material(
            ambient: 0,
            diffuse: 0,
            specular: 0.9,
            transparency: 0.9,
            reflective: 0.9,
            shininess: 300,
            refractiveIndex: 1.5);
        ball3.CastsShadow = false;
        
        var ball4 = new Sphere();
        ball4.Transform = Transformations.Translation(1, 1, 6);
        ball4.Material = new Material(
            color: new Color(0, 0.6, 1),
            ambient: 0.12,
            diffuse: 0,
            specular: 0.9,
            transparency: 0.9,
            reflective: 0.9,
            shininess: 300,
            refractiveIndex: 1.5);
        ball4.CastsShadow = false;
        
        // 构建场景
        var shapes = new List<Shape> { floor, wall, ball1, ball2, ball3, ball4 };
        
        var camera = new Camera(1920, 1080, Math.PI / 3);
        var cameraTransform = Transformations.ViewTransformation(
            Tuple4.Point(13, 2, 13), 
            Tuple4.Point(1, 1, 0), 
            Tuple4.Vector(0, 1, 0));
        camera.Transform = cameraTransform;
        
        var light = new Light(Tuple4.Point(10, 10, 5), new Color(1, 1, 1));
        
        var world = new World(
            new List<Light> { light }, 
            shapes, 
            camera);
        var canvas = world.Render();
        canvas.SavePng("./glass_balls.png");
    }

    /// <summary>
    /// 渲染封面图
    /// </summary>
    public static void RenderingCover()
    {
        // 相机
        var camera = new Camera(1000, 1000, 0.785);
        camera.Transform = Transformations.ViewTransformation(
            Tuple4.Point(-6, 6, -10),
            Tuple4.Point(6, 0, 6),
            Tuple4.Vector(-0.45, 1, 0));
        
        // 光源
        var light1 = new Light(
            Tuple4.Point(50, 100, -50),
            new Color(1, 1, 1));
        var light2 = new Light(
            Tuple4.Point(-400, 50, -10),
            new Color(0.2, 0.2, 0.2));
        var lights = new List<Light> { light1, light2 };
        
        // 材质
        var whiteMaterial = new Material(
            color: new Color(1, 1, 1),
            diffuse: 0.7,
            ambient: 0.1,
            specular: 0.0,
            reflective: 0.1);
        
        var blueMaterial = new Material(
            color: new Color(0.537, 0.831, 0.914),
            diffuse: 0.7,
            ambient: 0.1,
            specular: 0.0,
            reflective: 0.1);

        var redMaterial = new Material(
            color: new Color(0.941, 0.322, 0.388),
            diffuse: 0.7,
            ambient: 0.1,
            specular: 0.0,
            reflective: 0.1);

        var purpleMaterial = new Material(
            color: new Color(0.373, 0.404, 0.550),
            diffuse: 0.7,
            ambient: 0.1,
            specular: 0.0,
            reflective: 0.1);
        
        // 变换
        var standardTransform = Transformations.Translation(1, -1, 1) * 
                                Transformations.Scaling(0.5, 0.5, 0.5);
        var largeObject = standardTransform * Transformations.Scaling(3.5, 3.5, 3.5);
        var mediumObject = standardTransform * Transformations.Scaling(3, 3, 3);
        var smallObject = standardTransform * Transformations.Scaling(2, 2, 2);
        
        // 背景
        var background = new Plane();
        background.Material = new Material(
            color: new Color(1, 1, 1),
            ambient: 1,
            diffuse: 0,
            specular: 0);
        background.Transform =  Transformations.Translation(0, 0, 500) * 
                                Transformations.RotationX(90);
        
        // 球
        var sphere = new Sphere();
        sphere.Material = new Material(
            color: new Color(0.373, 0.404, 0.550),
            diffuse: 0.2,
            ambient: 0.0,
            specular: 1.0,
            shininess: 200,
            reflective: 0.7,
            transparency: 0.7,
            refractiveIndex: 1.5);
        sphere.Transform = largeObject;
        
        // 所有正方体
        var cube1 = new Cube();
        cube1.Material = whiteMaterial;
        cube1.Transform = Transformations.Translation(4, 0, 0) * mediumObject;

        var cube2 = new Cube();
        cube2.Material = blueMaterial;
        cube2.Transform = Transformations.Translation(8.5, 1.5, -0.5) * largeObject;

        var cube3 = new Cube();
        cube3.Material = redMaterial;
        cube3.Transform = Transformations.Translation(0, 0, 4) * largeObject;

        var cube4 = new Cube();
        cube4.Material = whiteMaterial;
        cube4.Transform = Transformations.Translation(4, 0, 4) * smallObject;
        
        var cube5 = new Cube();
        cube5.Material = purpleMaterial;
        cube5.Transform = Transformations.Translation(7.5, 0.5, 4) * mediumObject;

        var cube6 = new Cube();
        cube6.Material = whiteMaterial;
        cube6.Transform = Transformations.Translation(-0.25, 0.25, 8) * mediumObject;
        
        var cube7 = new Cube();
        cube7.Material = blueMaterial;
        cube7.Transform = Transformations.Translation(4, 1, 7.5) * largeObject;
        
        var cube8 = new Cube();
        cube8.Material = redMaterial;
        cube8.Transform = Transformations.Translation(10, 2, 7.5) * mediumObject;
        
        var cube9 = new Cube();
        cube9.Material = whiteMaterial;
        cube9.Transform = Transformations.Translation(8, 2, 12) * smallObject;
        
        var cube10 = new Cube();
        cube10.Material = whiteMaterial;
        cube10.Transform = Transformations.Translation(20, 1, 9) * smallObject;
        
        var cube11 = new Cube();
        cube11.Material = blueMaterial;
        cube11.Transform = Transformations.Translation(-0.5, -5, 0.25) * largeObject;
        
        var cube12 = new Cube();
        cube12.Material = redMaterial;
        cube12.Transform = Transformations.Translation(4, -4, 0) * largeObject;
        
        var cube13 = new Cube();
        cube13.Material = whiteMaterial;
        cube13.Transform = Transformations.Translation(8.5, -4, 0) * largeObject;
        
        var cube14 = new Cube();
        cube14.Material = whiteMaterial;
        cube14.Transform = Transformations.Translation(0, -4, 4) * largeObject;
        
        var cube15 = new Cube();
        cube15.Material = purpleMaterial;
        cube15.Transform = Transformations.Translation(-0.5, -4.5, 8) * largeObject;
        
        var cube16 = new Cube();
        cube16.Material = whiteMaterial;
        cube16.Transform = Transformations.Translation(0, -8, 4) * largeObject;
        
        var cube17 = new Cube();
        cube17.Material = whiteMaterial;
        cube17.Transform = Transformations.Translation(-0.5, -8.5, 8) * largeObject;

        var shapes = new List<Shape>
        {
            background, sphere, cube1, cube2, cube3, cube4, cube5, cube6, cube7, cube8,
            cube9, cube10, cube11, cube12, cube13, cube14, cube15, cube16, cube17
        };

        var w = new World(lights, shapes, camera);
        var canvas = w.Render();
        canvas.SavePng("./cover_with_anti_alias.png");
    }

    public static void IceCream()
    {
        var camera = new Camera(800, 400, Math.PI / 3);
        camera.Transform = Transformations.ViewTransformation(
            Tuple4.Point(0, 2.2, -6), 
            Tuple4.Point(0, 1, 0), 
            Tuple4.Vector(0, 1, 0));

        var floor = new Plane();
        floor.Material.Color = new Color(0.9, 0.9, 0.9);

        var wall = new Plane();
        wall.Transform = Transformations.Translation(0, 0, 5) *
                         Transformations.RotationX(90);
        wall.Material.Color = new Color(0.85, 0.87, 0.9);

        var cone = new Cone();
        cone.Transform = Transformations.RotationZ(-20) *
                         Transformations.Scaling(0.3, 1, 0.3);
        cone.Minimum = 0;
        cone.Maximum = 1;
        cone.Closed = true;
        cone.Material.Color = new Color(0.89, 0.76, 0.76);

        var ball1 = new Sphere();
        ball1.Transform = Transformations.Translation(0.4, 1.07, 0) *
                          Transformations.Scaling(0.35, 0.35, 0.35);
                          
        ball1.Material.Color = new Color(1, 0.35, 0.35);

        var ball2 = new Sphere();
        ball2.Transform = Transformations.Translation(0.5, 1.45, 0) *
                          Transformations.Scaling(0.25, 0.25, 0.25);
        
        ball2.Material.Color = new Color(0.34, 1, 0.5);

        var shapes = new List<Shape>
        {
            floor, wall, cone, ball1, ball2
        };
        var lights = new List<Light> { new Light(Tuple4.Point(-6, 8, -6), new Color(1, 1, 1)) };

        var world = new World(lights, shapes, camera);
        var canvas = world.Render();
        canvas.SavePng("./ice_cream.png");

    }


    private static Sphere HexagonCorner()
    {
        var corner = new Sphere();
        corner.Transform = Transformations.Translation(0, 0, -1) *
                           Transformations.Scaling(0.25, 0.25, 0.25);
        return corner;
    }

    private static Cylinder HexagonEdge()
    {
        var edge = new Cylinder();
        edge.Minimum = 0;
        edge.Maximum = 1;
        edge.Transform = Transformations.Translation(0, 0, -1) *
                         Transformations.RotationY(-30) *
                         Transformations.RotationZ(-90) *
                         Transformations.Scaling(0.25, 1, 0.25);
        return edge;
    }

    private static Group HexagonSide()
    {
        var side = new Group();
        
        side.AddChild(HexagonCorner());
        side.AddChild(HexagonEdge());

        return side;
    }

    private static Group Hexagon()
    {
        var hex = new Group();

        for (int n = 0; n < 6; n++)
        {
            var side = HexagonSide();
            side.Transform = Transformations.RotationY(n * 60);
            hex.AddChild(side);
        }

        return hex;
    }

    public static void RenderHexagon()
    {
        var camera = new Camera(1000, 1000, Math.PI / 3);
        camera.Transform = Transformations.ViewTransformation(
            Tuple4.Point(0, 2.2, -6), 
            Tuple4.Point(0, 1, 0), 
            Tuple4.Vector(0, 1, 0));
        
        var floor = new Plane();
        floor.Material.Color = new Color(0.9, 0.9, 0.9);

        var hexagon = Hexagon();
        hexagon.Transform = Transformations.Translation(0, 2, 0) *
                            Transformations.RotationX(60);
        
        var shapes = new List<Shape>
        {
            floor, hexagon
        };
        var lights = new List<Light> { new Light(Tuple4.Point(-6, 8, -6), new Color(1, 1, 1)) };

        var world = new World(lights, shapes, camera);
        var canvas = world.Render();
        canvas.SavePng("./hexagon.png");
    }
    
}
