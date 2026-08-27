namespace RayTracerChallenge.RayTracer;

public class World
{
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

        // find the direction to the light source
        Tuple4 lightV = (light.Position - point).Normalize();

        // compute the ambient contribution
        Color ambient = effectiveColor * material.Ambient;

        // light_dot_normal represents the cosine of the angle between the
        // light vector and the normal vector. A negative number means the
        // light is on the other side of the surface.
        double lightDotNormal = lightV.Dot(normalV);
        
        if (lightDotNormal < 0)
        {
            diffuse = black;
            specular = black;
        }
        else
        {
            // compute the diffuse contribution
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
}