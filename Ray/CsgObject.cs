using System;
using System.Collections.Generic;

namespace Ray
{
    delegate IEnumerable<Hit> CsgOperator(IEnumerable<Hit> leftHits, IEnumerable<Hit> rightHits);

    static class CsgOperators
    {
        public static IEnumerable<Hit> Unite(IEnumerable<Hit> leftHits, IEnumerable<Hit> rightHits)
        {
            return new IntervalUnion(Interval.FromHits(leftHits), Interval.FromHits(rightHits));
        }

        public static IEnumerable<Hit> Intersect(IEnumerable<Hit> leftHits, IEnumerable<Hit> rightHits)
        {
            return new IntervalIntersection(Interval.FromHits(leftHits), Interval.FromHits(rightHits));
        }

        public static IEnumerable<Hit> Except(IEnumerable<Hit> leftHits, IEnumerable<Hit> rightHits)
        {
            return new IntervalDifference(Interval.FromHits(leftHits), Interval.FromHits(rightHits));
        }
    }

    sealed class CsgObject : Object
    {
        readonly Object left, right;

        readonly CsgOperator csgOperator;

        public CsgObject(Object left, Object right, CsgOperator csgOperator, IEnumerable<Matrix> transformations = null)
            : base(transformations: transformations)
        {
            this.left = left ?? throw new ArgumentNullException("left");
            this.right = right ?? throw new ArgumentNullException("right");
            this.csgOperator = csgOperator ?? throw new ArgumentNullException("csgOperator");
        }

        protected override IEnumerable<Hit> IntersectTransformed(Ray ray)
        {
            return csgOperator(left.Intersect(ray), right.Intersect(ray));
        }
    }
}
