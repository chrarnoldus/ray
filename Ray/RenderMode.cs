using System;
using System.Linq;

namespace Ray
{
    abstract class RenderMode
    {
        protected RenderMode(Scene scene)
        {
            Scene = scene ?? throw new ArgumentNullException("scene");
        }

        protected bool IsLightBlocked(Object obj, Light light, Vector position)
        {
            if (!Scene.Shadows) return false;

            Ray ray = new Ray(light.Position, (position - light.Position).Normalize());

            Hit minHit = Scene.Objects
                .SelectMany(other => other.Intersect(ray))
                .Where(hit => hit.Time >= Hit.TimeEpsilon)
                .Min();

            return minHit != null && minHit.Object != obj;
        }

        protected Vector CalculateSpecularColor(Hit hit, Vector lightColor, Vector lightDir)
        {
            Material material = hit.Object.Material;
            Vector viewDir = -hit.Ray.Direction, reflectDir = lightDir.Reflect(hit.Normal);

            Vector color = material.SpecularColor * lightColor * Math.Pow(Math.Max(0.0, reflectDir.Dot(viewDir)), material.Shininess);

            return color;
        }

        protected Vector CalculateReflectionColor(Hit hit, int recursionDepth)
        {
            Material material = hit.Object.Material;

            if (material.IsReflective)
            {
                Vector viewDir = -hit.Ray.Direction;

                Vector color = material.SpecularColor * Scene.Retrace(hit, viewDir.Reflect(hit.Normal), recursionDepth);

                return color;
            }

            return Vector.AllZero;
        }

        private Vector? Refract(Vector rayDir, Vector normal, double refraction)
        {
            double underSqrt = 1.0 - (1.0 - Math.Pow(rayDir.Dot(normal), 2.0)) / Math.Pow(refraction, 2.0);

            if (underSqrt < 0.0) return null;

            return (rayDir - normal * rayDir.Dot(normal)) / refraction - normal * Math.Sqrt(underSqrt);
        }

        protected Vector CalculateRefractionColor(Hit hit, int recursionDepth)
        {
            Material material = hit.Object.Material;

            if (material.IsTransparant)
            {
                Vector rayDir = hit.Ray.Direction, normal = hit.Normal, position = hit.Position;
                double refraction = material.Refraction;

                Vector reflectDir = rayDir.Reflect(normal);

                Vector? transmissionDir;
                double incidence, intensity;

                if (rayDir.Dot(normal) < 0.0)
                {
                    transmissionDir = Refract(rayDir, normal, refraction).Value;
                    incidence = -rayDir.Dot(normal);
                    intensity = 1.0;
                }
                else
                {
                    intensity = Math.Exp(-hit.Time);
                    transmissionDir = Refract(rayDir, -normal, 1.0 / refraction);

                    if (transmissionDir.HasValue)
                        incidence = transmissionDir.Value.Dot(normal);
                    else
                        return intensity * Scene.Retrace(hit, reflectDir, recursionDepth);
                }

                double reflectance = material.Reflectance + (1.0 - material.Reflectance) * Math.Pow(1.0 - incidence, 5.0);

                return intensity * (
                    reflectance * Scene.Retrace(hit, reflectDir, recursionDepth) +
                    (1.0 - reflectance) * Scene.Retrace(hit, transmissionDir.Value, recursionDepth));
            }

            return Vector.AllZero;
        }

        public abstract Vector CalculateColor(Hit hit, int recursionDepth);

        protected Scene Scene { get; }
    }

    sealed class NullRenderMode : RenderMode
    {
        public NullRenderMode(Scene scene)
            : base(scene)
        {

        }

        public override Vector CalculateColor(Hit hit, int recursionDepth)
            => Scene.BackgroundColor;
    }
}
