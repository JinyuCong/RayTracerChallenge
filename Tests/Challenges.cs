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
        var t = new Tuple4();
        
        var start = t.Point(0, 1, 0);
        var velocity = t.Vector(1, 1.8, 0).Normalize() * 11.25;
        var p = new Projectile(start, velocity);

        var gravity = t.Vector(0, -0.1, 0);
        var wind = t.Vector(-0.01, 0, 0);
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
}