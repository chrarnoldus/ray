#nullable disable
using System;

namespace Ray
{
    sealed class Ray
    {
        public Ray(Vector origin, Vector direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public Vector At(double time)
            => Origin + time * Direction;

        public Ray Transform(Matrix transformation)
            => new Ray(
                transformation.Multiply(Origin, 1.0),
                transformation.Multiply(Direction));

        public Vector Origin { get; }

        public Vector Direction { get; }
    }

    sealed class Hit : IComparable<Hit>
    {
        public const double TimeEpsilon = 0.01;

        public Hit(double time, Vector normal, Ray ray, Object obj, UV? textureCoordinates = null)
        {
            if (ray == null)
                throw new ArgumentNullException("ray");

            Time = time;
            Normal = MakeNormalFaceEye(ray, normal);
            Position = ray.At(time);

            Ray = ray;
            Object = obj ?? throw new ArgumentNullException("obj");

            if (obj.Material.IsTextured)
                TextureCoordinates = textureCoordinates ?? obj.MapTexture(Position);
        }

        static Vector MakeNormalFaceEye(Ray ray, Vector normal)
        {
            Vector viewDir = -ray.Direction.Normalize();
            normal = normal.Normalize();

            return normal.Dot(viewDir) >= 0.0 ? normal : -normal;
        }

        public double Time { get; }

        public Vector Normal { get; }

        public Vector Position { get; }

        public Ray Ray { get; }

        public Object Object { get; }

        public UV? TextureCoordinates { get; }

        public int CompareTo(Hit other)
            => Time.CompareTo(other.Time);

        public Hit Transform(Vector normal)
            => new Hit(Time, normal, Ray, Object, TextureCoordinates);

        public Hit Transform(Vector normal, Ray ray)
            => new Hit(Time, normal, ray, Object, TextureCoordinates);
    }
}
