using System;

namespace Ray
{
    sealed class Matrix
    {
        readonly double[,] elements;

        Matrix(double[,] matrix)
        {
            elements = matrix;
        }

        public Matrix Multiply(Matrix other)
        {
            double[,] result = new double[4, 4];

            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    for (int i = 0; i < 4; i++)
                        result[row, column] += elements[row, i] * other.elements[i, column];
                }
            }

            return new Matrix(result);
        }

        public unsafe Vector Multiply(Vector vector, double w = 0.0)
        {
            double x = vector.X, y = vector.Y, z = vector.Z;

            Vector result = new Vector();
            double* ptr = (double*)(&result);

            for (int row = 0; row < 3; row++)
            {
                ptr[row] += elements[row, 0] * x;
                ptr[row] += elements[row, 1] * y;
                ptr[row] += elements[row, 2] * z;
                ptr[row] += elements[row, 3] * w;
            }

            return result;
        }

        public static Matrix operator *(Matrix a, Matrix b)
            => a.Multiply(b);

        public Matrix Transpose()
        {
            double[,] result = new double[4, 4];

            for (int row = 0; row < 4; row++)
            {
                for (int column = 0; column < 4; column++)
                {
                    result[column, row] = elements[row, column];
                }
            }

            return new Matrix(result);
        }

        public static Matrix Scale(Vector scale)
        {
            return new Matrix(new double[4, 4]
            {
                { scale.X,     0.0,     0.0, 0.0 },
                {     0.0, scale.Y,     0.0, 0.0 },
                {     0.0,     0.0, scale.Z, 0.0 },
                {     0.0,     0.0,     0.0, 1.0 },
            });
        }

        public static Matrix Translate(Vector translation)
        {
            return new Matrix(new double[4, 4]
            {
                { 1.0, 0.0, 0.0, translation.X },
                { 0.0, 1.0, 0.0, translation.Y },
                { 0.0, 0.0, 1.0, translation.Z },
                { 0.0, 0.0, 0.0,           1.0 },
            });
        }

        public static Matrix RotateX(Angle angle)
        {
            double rads = angle.Radians;

            return new Matrix(new double[4, 4]
            {
                { 1.0,            0.0,             0.0, 0.0 },
                { 0.0, Math.Cos(rads), -Math.Sin(rads), 0.0 },
                { 0.0, Math.Sin(rads),  Math.Cos(rads), 0.0 },
                { 0.0,            0.0,             0.0, 1.0 },
            });
        }

        public static Matrix RotateY(Angle angle)
        {
            double rads = angle.Radians;

            return new Matrix(new double[4, 4]
            {
                {  Math.Cos(rads), 0.0, Math.Sin(rads), 0.0 },
                {             0.0, 1.0,            0.0, 0.0 },
                { -Math.Sin(rads), 0.0, Math.Cos(rads), 0.0 },
                {             0.0, 0.0,            0.0, 1.0 },
            });
        }

        public static Matrix RotateZ(Angle angle)
        {
            double rads = angle.Radians;

            return new Matrix(new double[4, 4]
            {
                { Math.Cos(rads), -Math.Sin(rads), 0.0, 0.0 },
                { Math.Sin(rads),  Math.Cos(rads), 0.0, 0.0 },
                {            0.0,             0.0, 1.0, 0.0 },
                {            0.0,             0.0, 0.0, 1.0 },
            });
        }

        public static Matrix Identity { get; } = new Matrix(new double[4, 4]
            {
                { 1.0, 0.0, 0.0, 0.0 },
                { 0.0, 1.0, 0.0, 0.0 },
                { 0.0, 0.0, 1.0, 0.0 },
                { 0.0, 0.0, 0.0, 1.0 },
            });
    }
}
