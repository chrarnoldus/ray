using System;

namespace Ray
{
    // Based on:
    // A Simple Viewing Geometry
    // http://www-graphics.stanford.edu/courses/cs348b-99/viewgeom.html
    sealed class Camera
    {
        readonly Vector H, V;

        public Camera(Vector eye)
            : this(
            eye: eye,
            center: new Vector(200.0, 200.0, 0.0),
            up: new Vector(0.0, 1.0, 0.0),
            viewWidth: 400,
            viewHeight: 400)
        {

        }

        public Camera(Vector eye, Vector center, Vector up, int viewWidth, int viewHeight)
        {
            if (viewWidth < 0)
                throw new ArgumentOutOfRangeException("viewWidth");

            if (viewHeight < 0)
                throw new ArgumentOutOfRangeException("viewHeight");

            Eye = eye;
            Center = center;
            Up = up;

            ViewWidth = viewWidth;
            ViewHeight = viewHeight;

            Vector G = center - eye;
            Vector A = G.Cross(up), B = A.Cross(G);

            H = A.Normalize() * viewWidth / 2.0 * up.Length();
            V = B.Normalize() * viewHeight / 2.0 * up.Length();
        }

        public Ray CalculateRay(double x, double y)
        {
            double sx = x / (ViewWidth - 1), sy = y / (ViewHeight - 1);
            Vector pixel = Center + (2 * sx - 1) * H + (1 - 2 * sy) * V;

            return new Ray(Eye, (pixel - Eye).Normalize());
        }

        public Vector Eye { get; }

        public Vector Center { get; }

        public Vector Up { get; }

        public int ViewWidth { get; }

        public int ViewHeight { get; }
    }
}
