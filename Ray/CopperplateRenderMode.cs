using System;

namespace Ray
{
    sealed class Hatch
    {
        readonly Vector normal;
        readonly double distance, weight;

        public Hatch(Vector normal, double distance, double weight)
        {
            this.normal = normal.Normalize();
            this.distance = distance;
            this.weight = weight;
        }

        public Vector Normal
        {
            get { return normal; }
        }

        public double Distance
        {
            get { return distance; }
        }

        public double Weight
        {
            get { return weight; }
        }
    }

    // Based on:
    // Leister (1994)
    // Computer Generated Copper Plates
    // http://www.nr.no/~wolfgang/copper/copper.html
    sealed class CopperplateRenderMode : RenderMode
    {
        readonly PhongRenderMode phong;

        public CopperplateRenderMode(Scene scene)
            : base(scene)
        {
            phong = new PhongRenderMode(scene);
        }

        public override Vector CalculateColor(Hit hit, int recursionDepth)
        {
            Vector position = hit.Position;

            double combinedValue = 0.0;

            foreach (Hatch hatch in hit.Object.Hatches)
            {
                double f = position.Dot(hatch.Normal);

                // Equation 3
                double value = hatch.Weight * (f >= 0 ?
                    (f % hatch.Distance) / hatch.Distance :
                    (hatch.Distance + f % hatch.Distance) / hatch.Distance);

                // Equation 4
                combinedValue = value >= 0 ? Math.Max(combinedValue, value) : Math.Min(combinedValue, -value);
            }

            Vector phongColor = phong.CalculateColor(hit, Scene.MaxRecursionDepth);
            combinedValue += (phongColor.R + phongColor.G + phongColor.B) / 3.0;

            return combinedValue >= 0.5 ? Vector.AllOne : Vector.AllZero;
        }
    }
}
