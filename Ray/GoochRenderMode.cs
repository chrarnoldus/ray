namespace Ray
{
    struct GoochParameters
    {
        readonly double blue, yellow, alpha, beta;

        public GoochParameters(double blue, double yellow, double alpha, double beta)
        {
            this.blue = blue;
            this.yellow = yellow;
            this.alpha = alpha;
            this.beta = beta;
        }

        public double Blue
        {
            get { return blue; }
        }

        public double Yellow
        {
            get { return yellow; }
        }

        public double Alpha
        {
            get { return alpha; }
        }

        public double Beta
        {
            get { return beta; }
        }
    }

    sealed class GoochRenderMode : RenderMode
    {
        readonly GoochParameters parameters;

        public GoochRenderMode(Scene scene, GoochParameters parameters)
            : base(scene)
        {
            this.parameters = parameters;
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
                bool lightBlocked = IsLightBlocked(hit.Object, light, position);
                if (lightBlocked) continue;

                Vector lightDir = (light.Position - position).Normalize();
                Vector baseColor = light.Color * material.DiffuseColor *
                    material.GetTextureColor(hit.TextureCoordinates);

                Vector coolColor = new Vector(0.0, 0.0, parameters.Blue) + parameters.Alpha * baseColor;
                Vector warmColor = new Vector(parameters.Yellow, parameters.Yellow, 0.0) + parameters.Beta * baseColor;

                double nl = normal.Dot(lightDir);
                Vector goochColor = coolColor * (1.0 - nl) / 2.0 + warmColor * (1.0 + nl) / 2.0;

                Vector specularColor = CalculateSpecularColor(hit, light.Color, lightDir);

                totalColor += goochColor + specularColor;
            }

            totalColor += CalculateReflectionColor(hit, recursionDepth);
            totalColor += CalculateRefractionColor(hit, recursionDepth);

            return totalColor;
        }
    }
}
