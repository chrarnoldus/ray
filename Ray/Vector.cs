using System;

namespace Ray
{
    struct Vector
    {
        readonly double x, y, z;

        public Vector(double xyz)
            : this(xyz, xyz, xyz)
        {

        }

        public Vector(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double X
        {
            get { return x; }
        }

        public double Y
        {
            get { return y; }
        }

        public double Z
        {
            get { return z; }
        }

        public double R
        {
            get { return x; }
        }

        public double G
        {
            get { return y; }
        }

        public double B
        {
            get { return z; }
        }

        public double this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return x;
                    case 1:
                        return y;
                    case 2:
                        return z;
                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }

        public Vector Add(Vector other)
        {
            return new Vector(
                x + other.x,
                y + other.y,
                z + other.z);
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return a.Add(b);
        }

        public Vector Subtract(Vector other)
        {
            return new Vector(
                x - other.x,
                y - other.y,
                z - other.z);
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
                x * other.x,
                y * other.y,
                z * other.z);
        }

        public Vector Multiply(double multiplier)
        {
            return new Vector(
                x * multiplier,
                y * multiplier,
                z * multiplier);
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
                x / other.x,
                y / other.y,
                z / other.z);
        }

        public Vector Divide(double divisor)
        {
            return new Vector(
                x / divisor,
                y / divisor,
                z / divisor);
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
            return x * other.x + y * other.y + z * other.z;
        }

        public Vector Cross(Vector other)
        {
            return new Vector(
                y * other.z - z * other.y,
                z * other.x - x * other.z,
                x * other.y - y * other.x);
        }

        public Vector Reflect(Vector normal)
        {
            return -this + 2 * Dot(normal) * normal;
        }

        public double LengthSquared()
        {
            return x * x + y * y + z * z;
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
                Math.Max(Math.Min(x, max), min),
                Math.Max(Math.Min(y, max), min),
                Math.Max(Math.Min(z, max), min));
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
