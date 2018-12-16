using System;

namespace Ray
{
    sealed class PhongRenderMode : RenderMode
    {
        public PhongRenderMode(Scene scene)
            : base(scene)
        {

        }

        public override Vector CalculateColor(Hit hit, int recursionDepth)
        {
            Material material = hit.Object.Material;

            Vector position = hit.Position;
            Vector normal = hit.Normal;
            Vector viewDir = -hit.Ray.Direction;

            Vector totalColor = Vector.AllZero;

            foreach (Light light in Scene.Lights)
            {
                Vector lightDir = (light.Position - position).Normalize();
                Vector baseColor = light.Color * material.GetTextureColor(hit.TextureCoordinates);

                bool lightBlocked = IsLightBlocked(hit.Object, light, position);

                Vector diffuseColor = lightBlocked ? Vector.AllZero :
                    material.DiffuseColor * baseColor * Math.Max(0.0, normal.Dot(lightDir));

                Vector specularColor = lightBlocked ? Vector.AllZero :
                    CalculateSpecularColor(hit, light.Color, lightDir);

                Vector ambientColor = material.AmbientColor * baseColor;

                totalColor += diffuseColor + specularColor + ambientColor;
            }

            totalColor += CalculateReflectionColor(hit, recursionDepth);
            totalColor += CalculateRefractionColor(hit, recursionDepth);

            return totalColor;
        }
    }
}