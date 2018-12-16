using System;
using System.Collections.Generic;

namespace Ray
{
    sealed class Cylinder : Object
    {
        public Cylinder(Material material, IEnumerable<Matrix> transformations = null, IEnumerable<Hatch> hatches = null)
            : base(material, transformations, hatches)
        {
            if (material == null)
                throw new ArgumentNullException("material");
        }

        Vector CalculateNormal(Vector position)
            => position - new Vector(0.0, position.Y, 0.0);

        IEnumerable<Hit> IntersectSides(Ray ray)
        {
            double a =
                Math.Pow(ray.Direction.X, 2.0) +
                Math.Pow(ray.Direction.Z, 2.0);

            double b =
                2.0 * ray.Origin.X * ray.Direction.X +
                2.0 * ray.Origin.Z * ray.Direction.Z;

            double c =
                Math.Pow(ray.Origin.X, 2.0) +
                Math.Pow(ray.Origin.Z, 2.0) - 1.0;

            double d = AbcSolver.CalculateDiscriminant(a, b, c);

            if (d < 0.0)
                yield break;

            double t = AbcSolver.SolveMin(a, b, c, d);
            Vector position = ray.At(t);

            if (position.Y >= -0.5 && position.Y <= 0.5)
                yield return new Hit(t, CalculateNormal(position), ray, this);

            t = AbcSolver.SolveMax(a, b, c, d);
            position = ray.At(t);

            if (position.Y >= -0.5 && position.Y <= 0.5)
                yield return new Hit(t, CalculateNormal(position), ray, this);
        }

        Hit IntersectCap(Ray ray, double y)
        {
            double time = (y - ray.Origin.Y) / ray.Direction.Y;

            if (double.IsInfinity(time) || double.IsNaN(time))
                return null;

            Vector position = ray.At(time);

            if (Math.Pow(position.X, 2.0) + Math.Pow(position.Z, 2.0) > 1.0)
                return null;

            return new Hit(time, Vector.UnitY, ray, this);
        }

        protected override IEnumerable<Hit> IntersectTransformed(Ray ray)
        {
            Hit withBottom = IntersectCap(ray, -0.5);
            if (withBottom != null)
                yield return withBottom;

            Hit withTop = IntersectCap(ray, 0.5);
            if (withTop != null)
                yield return withTop;

            foreach (Hit hit in IntersectSides(ray))
                yield return hit;
        }
    }
}
