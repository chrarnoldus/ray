using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ray
{
    // Based on:
    // A Simple Viewing Geometry
    // http://www-graphics.stanford.edu/courses/cs348b-99/viewgeom.html
    sealed class Camera
    {
        readonly Vector eye, center, up;

        readonly int viewWidth, viewHeight;

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
            
            this.eye = eye;
            this.center = center;
            this.up = up;

            this.viewWidth = viewWidth;
            this.viewHeight = viewHeight;

            Vector G = center - eye;
            Vector A = G.Cross(up), B = A.Cross(G);

            H = A.Normalize() * viewWidth / 2.0 * up.Length();
            V = B.Normalize() * viewHeight / 2.0 * up.Length();
        }

        public Ray CalculateRay(double x, double y)
        {
            double sx = x / (viewWidth - 1), sy = y / (viewHeight - 1);
            Vector pixel = center + (2 * sx - 1) * H + (1 - 2 * sy) * V;

            return new Ray(eye, (pixel - eye).Normalize());
        }

        public Vector Eye
        {
            get { return eye; }
        }

        public Vector Center
        {
            get { return center; }
        }

        public Vector Up
        {
            get { return up; }
        }

        public int ViewWidth
        {
            get { return viewWidth; }
        }

        public int ViewHeight
        {
            get { return viewHeight; }
        }
    }
}
