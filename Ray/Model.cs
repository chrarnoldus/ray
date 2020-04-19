#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ray
{
    sealed class Model : Object
    {
        readonly ReadOnlyCollection<Triangle> triangles;
        readonly ReadOnlyCollection<Tuple<Vector, Vector, Vector>> normalsPerTriangle;

        readonly bool smooth;

        public Model(IEnumerable<Triangle> triangles, IEnumerable<Tuple<Vector, Vector, Vector>> normalsPerTriangle,
            bool smooth = false, IEnumerable<Matrix> transformations = null)
            : base(transformations: transformations)
        {
            if (triangles == null)
                throw new ArgumentNullException(nameof(triangles));

            if (normalsPerTriangle == null)
                throw new ArgumentNullException(nameof(normalsPerTriangle));

            this.triangles = triangles.ToList().AsReadOnly();
            this.normalsPerTriangle = normalsPerTriangle.ToList().AsReadOnly();
            this.smooth = smooth;

            if (this.triangles.Count != this.normalsPerTriangle.Count)
                throw new ArgumentException("Triangle count not equal to normals per triangle count.");
        }

        bool IntersectsBoundingSphere(Ray ray)
        {
            double a = ray.Direction.Dot(ray.Direction);
            double b = 2.0 * ray.Origin.Dot(ray.Direction);
            double c = ray.Origin.Dot(ray.Origin) - 1.0;

            double d = AbcSolver.CalculateDiscriminant(a, b, c);

            return d >= 0.0;
        }

        Vector InterpolateNormal(Hit hit, Tuple<Vector, Vector, Vector> vertexNormals)
        {
            Triangle triangle = (Triangle)hit.Object;
            Vector a = triangle.VertexA, b = triangle.VertexB, c = triangle.VertexC;
            Vector p = hit.Position;

            // Based on barycentric coordinates
            // See: http://en.wikipedia.org/wiki/Barycentric_coordinate_system_(mathematics)

            // A point p on a triangle can be represented as follows:
            // p = labdaA * a + labdaB * b + labdaC * c
            // where labdaA + labdaB + labdaC = 1

            // These labdas can be calculated using the formulae:
            double labdaA =
                ((b.Y - c.Y) * (p.X - c.X) + (c.X - b.X) * (p.Y - c.Y)) /
                ((b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y));

            double labdaB =
                ((c.Y - a.Y) * (p.X - c.X) + (a.X - c.X) * (p.Y - c.Y)) /
                ((b.Y - c.Y) * (a.X - c.X) + (c.X - b.X) * (a.Y - c.Y));

            double labdaC = 1.0 - labdaA - labdaB;

            // Linearly interpolate normal using barycentric weights:
            return
                labdaA * vertexNormals.Item1 +
                labdaB * vertexNormals.Item2 +
                labdaC * vertexNormals.Item3;
        }

        protected override IEnumerable<Hit> IntersectTransformed(Ray ray)
        {
            if (!IntersectsBoundingSphere(ray))
                yield break;

            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle triangle = triangles[i];

                foreach (Hit hit in triangle.Intersect(ray))
                {
                    if (smooth)
                    {
                        Tuple<Vector, Vector, Vector> vertexNormals = normalsPerTriangle[i];
                        yield return hit.Transform(InterpolateNormal(hit, vertexNormals));
                    }
                    else
                        yield return hit;
                }
            }
        }
    }
}
