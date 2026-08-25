namespace RayTracerChallenge.Tests;

using RayTracer;
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