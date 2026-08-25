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