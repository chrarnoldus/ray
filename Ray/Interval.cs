using System.Collections.Generic;
using System.Linq;

namespace Ray
{
    abstract class Interval : IEnumerable<Hit>
    {
        readonly List<Hit> hits = new List<Hit>();

        public abstract bool Contains(Hit hit);

        public IEnumerator<Hit> GetEnumerator()
        {
            return hits.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return hits.GetEnumerator();
        }

        protected List<Hit> Hits
        {
            get { return hits; }
        }

        sealed class SimpleInterval : Interval
        {
            readonly double min = double.PositiveInfinity, max = double.NegativeInfinity;

            public SimpleInterval(IEnumerable<Hit> hits)
            {
                Hit minHit = hits.Min(), maxHit = hits.Max();

                if (minHit != null)
                {
                    Hits.Add(minHit);
                    min = max = minHit.Time;
                }

                if (maxHit != minHit)
                {
                    Hits.Add(maxHit);
                    max = maxHit.Time;
                }
            }

            public override bool Contains(Hit hit)
            {
                return hit.Time >= min && hit.Time <= max;
            }
        }

        public static Interval FromHits(IEnumerable<Hit> hits)
        {
            return hits as Interval ?? new SimpleInterval(hits);
        }
    }

    sealed class IntervalUnion : Interval
    {
        readonly Interval left, right;

        public IntervalUnion(Interval left, Interval right)
        {
            this.left = left;
            this.right = right;

            // Do not store occluded hits
            foreach (Hit hit in left)
            {
                if (!right.Contains(hit))
                    Hits.Add(hit);
            }

            foreach (Hit hit in right)
            {
                if (!left.Contains(hit))
                    Hits.Add(hit);
            }
        }

        public override bool Contains(Hit hit)
        {
            return left.Contains(hit) || right.Contains(hit);
        }
    }

    sealed class IntervalIntersection : Interval
    {
        readonly Interval left, right;

        public IntervalIntersection(Interval left, Interval right)
        {
            this.left = left;
            this.right = right;

            foreach (Hit hit in left)
            {
                if (right.Contains(hit))
                    Hits.Add(hit);
            }

            foreach (Hit hit in right)
            {
                if (left.Contains(hit))
                    Hits.Add(hit);
            }
        }

        public override bool Contains(Hit hit)
        {
            return left.Contains(hit) && right.Contains(hit);
        }
    }

    sealed class IntervalDifference : Interval
    {
        readonly Interval left, right;

        public IntervalDifference(Interval left, Interval right)
        {
            this.left = left;
            this.right = right;

            foreach (Hit hit in left)
            {
                if (!right.Contains(hit))
                    Hits.Add(hit);
            }

            // Also store some hits with right shape in order to close left shape
            foreach (Hit hit in right)
            {
                if (left.Contains(hit))
                    Hits.Add(hit);
            }
        }

        public override bool Contains(Hit hit)
        {
            return left.Contains(hit) && !right.Contains(hit);
        }
    }
}
