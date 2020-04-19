using System;

namespace Ray
{
    struct Vector
    {
        public Vector(double xyz)
            : this(xyz, xyz, xyz)
        {

        }

        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }

        public double R => X;

        public double G => Y;

        public double B => Z;

        public double this[int i]
            => i switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => throw new IndexOutOfRangeException(),
            };

        public Vector Add(Vector other)
            => new Vector(
                X + other.X,
                Y + other.Y,
                Z + other.Z);

        public static Vector operator +(Vector a, Vector b)
            => a.Add(b);

        public Vector Subtract(Vector other)
            => new Vector(
                X - other.X,
                Y - other.Y,
                Z - other.Z);

        public static Vector operator -(Vector a, Vector b)
            => a.Subtract(b);

        public static Vector operator -(Vector vector)
            => -1 * vector;

        public Vector Multiply(Vector other)
            => new Vector(
                X * other.X,
                Y * other.Y,
                Z * other.Z);

        public Vector Multiply(double multiplier)
            => new Vector(
                X * multiplier,
                Y * multiplier,
                Z * multiplier);

        public static Vector operator *(Vector a, Vector b)
            => a.Multiply(b);

        public static Vector operator *(Vector a, double b)
            => a.Multiply(b);

        public static Vector operator *(double a, Vector b)
            => b.Multiply(a);

        public Vector Divide(Vector other)
            => new Vector(
                X / other.X,
                Y / other.Y,
                Z / other.Z);

        public Vector Divide(double divisor)
            => new Vector(
                X / divisor,
                Y / divisor,
                Z / divisor);

        public static Vector operator /(Vector a, Vector b)
            => a.Divide(b);

        public static Vector operator /(Vector a, double b)
            => a.Divide(b);

        public double Dot(Vector other)
            => X * other.X + Y * other.Y + Z * other.Z;

        public Vector Cross(Vector other)
            => new Vector(
                Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z,
                X * other.Y - Y * other.X);

        public Vector Reflect(Vector normal)
            => -this + 2 * Dot(normal) * normal;

        public double LengthSquared()
            => X * X + Y * Y + Z * Z;

        public double Length()
            => Math.Sqrt(LengthSquared());

        public double Distance(Vector other)
            => (this - other).Length();

        public Vector Normalize()
            => this / Length();

        public Vector Clamp(double min = 0.0, double max = 1.0)
            => new Vector(
                Math.Max(Math.Min(X, max), min),
                Math.Max(Math.Min(Y, max), min),
                Math.Max(Math.Min(Z, max), min));

        public Vector Rotate(Vector rotation, Angle angle)
        {
            rotation = rotation.Normalize();
            double rads = angle.Radians;

            // Rodrigues' rotation formula
            return
                this * Math.Cos(rads) +
                rotation.Cross(this) * Math.Sin(rads) +
                rotation * rotation.Dot(this) * (1.0 - Math.Cos(rads));
        }

        public static Vector AllZero
            => new Vector();

        public static Vector AllOne
            => new Vector(1.0);

        public static Vector UnitX
            => new Vector(1.0, 0.0, 0.0);

        public static Vector UnitY
            => new Vector(0.0, 1.0, 0.0);

        public static Vector UnitZ
            => new Vector(0.0, 0.0, 1.0);
    }

    struct Angle
    {
        readonly double radians;

        Angle(double radians)
        {
            this.radians = radians;
        }

        public double Degrees
            => radians / Math.PI * 180.0;

        public double Radians
            => radians;

        public static Angle FromDegrees(double degrees)
            => new Angle(degrees / 180.0 * Math.PI);

        public static Angle FromRadians(double radians)
            => new Angle(radians);
    }
}
