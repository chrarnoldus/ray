using System;
using System.Collections.Generic;

namespace Ray
{
    sealed class Triangle : Object
    {
        public Triangle(Material material, Vector vertexA, Vector vertexB, Vector vertexC,
            IEnumerable<Matrix> transformations = null, IEnumerable<Hatch> hatches = null)
            : base(material, transformations, hatches)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            VertexA = vertexA;
            VertexB = vertexB;
            VertexC = vertexC;

            FacetNormal = (vertexB - vertexA).Cross(vertexC - vertexA).Normalize();
        }

        protected override IEnumerable<Hit> IntersectTransformed(Ray ray)
        {
            // Based on:
            // Fundamentals of Computer Graphics
            // Section 4.4.2

            Vector
                c1 = VertexA - VertexB,
                c2 = VertexA - VertexC,
                c3 = ray.Direction,
                c4 = VertexA - ray.Origin;

            double M =
                c1.X * (c2.Y * c3.Z - c3.Y * c2.Z) +
                c1.Y * (c3.X * c2.Z - c2.X * c3.Z) +
                c1.Z * (c2.X * c3.Y - c2.Y * c3.X);

            double time = -1 * (
                c2.Z * (c1.X * c4.Y - c4.X * c1.Y) +
                c2.Y * (c4.X * c1.Z - c1.X * c4.Z) +
                c2.X * (c1.Y * c4.Z - c4.Y * c1.Z)) / M;

            double gamma = (
                c3.Z * (c1.X * c4.Y - c4.X * c1.Y) +
                c3.Y * (c4.X * c1.Z - c1.X * c4.Z) +
                c3.X * (c1.Y * c4.Z - c4.Y * c1.Z)) / M;

            if (gamma < 0.0 || gamma > 1.0)
                yield break;

            double beta = (
                c4.X * (c2.Y * c3.Z - c3.Y * c2.Z) +
                c4.Y * (c3.X * c2.Z - c2.X * c3.Z) +
                c4.Z * (c2.X * c3.Y - c2.Y * c3.X)) / M;

            if (beta < 0.0 || beta > 1.0 - gamma)
                yield break;

            yield return new Hit(time, FacetNormal, ray, this);
        }

        public Vector VertexA { get; }

        public Vector VertexB { get; }

        public Vector VertexC { get; }

        public Vector FacetNormal { get; }
    }
}
