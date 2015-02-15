using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ray
{
    sealed class Ray
    {
        readonly Vector origin, direction;

        public Ray(Vector origin, Vector direction)
        {
            this.origin = origin;
            this.direction = direction;
        }

        public Vector At(double time)
        {
            return Origin + time * Direction;
        }

        public Ray Transform(Matrix transformation)
        {
            return new Ray(
                transformation.Multiply(origin, 1.0),
                transformation.Multiply(direction));
        }

        public Vector Origin
        {
            get { return origin; }
        }

        public Vector Direction
        {
            get { return direction; }
        }
    }

    sealed class Hit : IComparable<Hit>
    {
        public const double TimeEpsilon = 0.01;

        readonly double time;
        readonly Vector normal, position;

        readonly Ray ray;
        readonly Object obj;

        readonly UV? textureCoordinates;

        public Hit(double time, Vector normal, Ray ray, Object obj, UV? textureCoordinates = null)
        {
            if (ray == null)
                throw new ArgumentNullException("ray");

            if (obj == null)
                throw new ArgumentNullException("obj");

            this.time = time;
            this.normal = MakeNormalFaceEye(ray, normal);
            this.position = ray.At(time);

            this.ray = ray;
            this.obj = obj;

            if (obj.Material.IsTextured)
                this.textureCoordinates = textureCoordinates ?? obj.MapTexture(position);
        }

        static Vector MakeNormalFaceEye(Ray ray, Vector normal)
        {
            Vector viewDir = -ray.Direction.Normalize();
            normal = normal.Normalize();

            return normal.Dot(viewDir) >= 0.0 ? normal : -normal;
        }

        public double Time
        {
            get { return time; }
        }

        public Vector Normal
        {
            get { return normal; }
        }

        public Vector Position
        {
            get { return position; }
        }

        public Ray Ray
        {
            get { return ray; }
        }

        public Object Object
        {
            get { return obj; }
        }

        public UV? TextureCoordinates
        {
            get { return textureCoordinates; }
        }

        public int CompareTo(Hit other)
        {
            return time.CompareTo(other.time);
        }

        public Hit Transform(Vector normal)
        {
            return new Hit(time, normal, ray, obj, textureCoordinates);
        }

        public Hit Transform(Vector normal, Ray ray)
        {
            return new Hit(time, normal, ray, obj, textureCoordinates);
        }
    }
}
