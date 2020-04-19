#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Ray
{
    abstract class Object
    {
        readonly Matrix transformation, transposed;

        protected Object(Material material = null, IEnumerable<Matrix> transformations = null, IEnumerable<Hatch> hatches = null)
        {
            Material = material;
            Hatches = (hatches ?? Enumerable.Empty<Hatch>()).ToList().AsReadOnly();

            transformation = (transformations ?? Enumerable.Empty<Matrix>()).Aggregate(Matrix.Identity, (a, b) => a * b);
            transposed = transformation.Transpose();
        }

        public IEnumerable<Hit> Intersect(Ray ray)
        {
            Ray transformed = ray.Transform(transformation);
            IEnumerable<Hit> hits = IntersectTransformed(transformed);
            return hits.Select(hit => hit.Transform(transposed.Multiply(hit.Normal), ray));
        }

        protected abstract IEnumerable<Hit> IntersectTransformed(Ray ray);

        public virtual UV MapTexture(Vector position)
            => throw new NotSupportedException(GetType().Name + " does not support texture mapping.");

        public ReadOnlyCollection<Hatch> Hatches { get; }

        public Material Material { get; }
    }
}
