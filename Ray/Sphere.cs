#nullable disable
using System;
using System.Collections.Generic;

namespace Ray
{
    static class AbcSolver
    {
        public static double CalculateDiscriminant(double a, double b, double c)
        {
            return b * b - (4.0 * a * c);
        }

        public static double SolveMin(double a, double b, double c, double d)
        {
            return (-b - Math.Sqrt(d)) / (2.0 * a);
        }

        public static double SolveMax(double a, double b, double c, double d)
        {
            return (-b + Math.Sqrt(d)) / (2.0 * a);
        }
    }

    sealed class Sphere : Object
    {
        readonly Vector position;
        readonly double radius;

        public Sphere(Material material, Vector position = default(Vector), double radius = 1.0,
            IEnumerable<Matrix> transformations = null, IEnumerable<Hatch> hatches = null)
            : base(material, transformations, hatches)
        {
            if (material == null)
                throw new ArgumentNullException(nameof(material));

            this.position = position;
            this.radius = Math.Abs(radius);
        }

        Vector CalculateNormal(Ray ray, double t)
        {
            return ray.At(t) - position;
        }

        protected override IEnumerable<Hit> IntersectTransformed(Ray ray)
        {
            double a = ray.Direction.Dot(ray.Direction);
            double b = 2.0 * (ray.Origin - position).Dot(ray.Direction);
            double c = (ray.Origin - position).Dot(ray.Origin - position) - radius * radius;

            double d = AbcSolver.CalculateDiscriminant(a, b, c);

            if (d < 0.0)
                yield break;

            double t = AbcSolver.SolveMin(a, b, c, d);
            Vector normal = CalculateNormal(ray, t);
            yield return new Hit(t, normal, ray, this);

            t = AbcSolver.SolveMax(a, b, c, d);
            normal = CalculateNormal(ray, t);
            yield return new Hit(t, normal, ray, this);
        }

        public override UV MapTexture(Vector position)
        {
            Vector relative = position - this.position;

            // Based on:
            // Fundamentals of Computer Graphics
            // Section 11.2

            double theta = Math.Acos(relative.Z / radius);
            double phi = Math.Atan2(relative.Y, relative.X);

            if (phi < 0.0)
                phi += 2.0 * Math.PI;

            return new UV(
                u: phi / (2.0 * Math.PI),
                v: (Math.PI - theta) / Math.PI);
        }
    }
}
