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

        public double R
        {
            get { return X; }
        }

        public double G
        {
            get { return Y; }
        }

        public double B
        {
            get { return Z; }
        }

        public double this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return X;
                    case 1:
                        return Y;
                    case 2:
                        return Z;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public Vector Add(Vector other)
        {
            return new Vector(
                X + other.X,
                Y + other.Y,
                Z + other.Z);
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return a.Add(b);
        }

        public Vector Subtract(Vector other)
        {
            return new Vector(
                X - other.X,
                Y - other.Y,
                Z - other.Z);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return a.Subtract(b);
        }

        public static Vector operator -(Vector vector)
        {
            return -1 * vector;
        }

        public Vector Multiply(Vector other)
        {
            return new Vector(
                X * other.X,
                Y * other.Y,
                Z * other.Z);
        }

        public Vector Multiply(double multiplier)
        {
            return new Vector(
                X * multiplier,
                Y * multiplier,
                Z * multiplier);
        }

        public static Vector operator *(Vector a, Vector b)
        {
            return a.Multiply(b);
        }

        public static Vector operator *(Vector a, double b)
        {
            return a.Multiply(b);
        }

        public static Vector operator *(double a, Vector b)
        {
            return b.Multiply(a);
        }

        public Vector Divide(Vector other)
        {
            return new Vector(
                X / other.X,
                Y / other.Y,
                Z / other.Z);
        }

        public Vector Divide(double divisor)
        {
            return new Vector(
                X / divisor,
                Y / divisor,
                Z / divisor);
        }

        public static Vector operator /(Vector a, Vector b)
        {
            return a.Divide(b);
        }

        public static Vector operator /(Vector a, double b)
        {
            return a.Divide(b);
        }

        public double Dot(Vector other)
        {
            return X * other.X + Y * other.Y + Z * other.Z;
        }

        public Vector Cross(Vector other)
        {
            return new Vector(
                Y * other.Z - Z * other.Y,
                Z * other.X - X * other.Z,
                X * other.Y - Y * other.X);
        }

        public Vector Reflect(Vector normal)
        {
            return -this + 2 * Dot(normal) * normal;
        }

        public double LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        public double Length()
        {
            return Math.Sqrt(LengthSquared());
        }

        public double Distance(Vector other)
        {
            return (this - other).Length();
        }

        public Vector Normalize()
        {
            return this / Length();
        }

        public Vector Clamp(double min = 0.0, double max = 1.0)
        {
            return new Vector(
                Math.Max(Math.Min(X, max), min),
                Math.Max(Math.Min(Y, max), min),
                Math.Max(Math.Min(Z, max), min));
        }

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
        {
            get { return new Vector(); }
        }

        public static Vector AllOne
        {
            get { return new Vector(1.0); }
        }

        public static Vector UnitX
        {
            get { return new Vector(1.0, 0.0, 0.0); }
        }

        public static Vector UnitY
        {
            get { return new Vector(0.0, 1.0, 0.0); }
        }

        public static Vector UnitZ
        {
            get { return new Vector(0.0, 0.0, 1.0); }
        }
    }

    struct Angle
    {
        readonly double radians;

        Angle(double radians)
        {
            this.radians = radians;
        }

        public double Degrees
        {
            get { return radians / Math.PI * 180.0; }
        }

        public double Radians
        {
            get { return radians; }
        }

        public static Angle FromDegrees(double degrees)
        {
            return new Angle(degrees / 180.0 * Math.PI);
        }

        public static Angle FromRadians(double radians)
        {
            return new Angle(radians);
        }
    }
}
