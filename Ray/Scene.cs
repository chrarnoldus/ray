#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ray
{
    sealed class Scene
    {
        public Vector Retrace(Hit hit, Vector direction, int recursionDepth)
        {
            if (recursionDepth >= MaxRecursionDepth)
                return Vector.AllZero;

            Ray ray = new Ray(hit.Position, direction);
            return Trace(ray, recursionDepth + 1);
        }

        Vector Trace(Ray ray, int recursionDepth = 0)
        {
            Hit minHit = Objects
                .SelectMany(obj => obj.Intersect(ray))
                .Where(hit => hit.Time >= Hit.TimeEpsilon)
                .Min();

            if (minHit == null)
                return BackgroundColor;

            return RenderMode.CalculateColor(minHit, recursionDepth);
        }

        void RenderLine(Image image, int y)
        {
            double samples = Math.Pow(SupersamplingFactor, 2.0);
            double offset = 1.0 / (SupersamplingFactor + 1.0);

            for (int x = 0; x < image.Width; x++)
            {
                Vector average = Vector.AllZero;

                for (int i = 1; i <= SupersamplingFactor; i++)
                {
                    for (int j = 1; j <= SupersamplingFactor; j++)
                    {
                        double sampleX = x + i * offset;
                        double sampleY = y + j * offset;

                        Ray ray = Camera.CalculateRay(sampleX, sampleY);
                        Vector color = Trace(ray).Clamp();

                        average += color / samples;
                    }
                }

                image[x, y] = average;
            }
        }

        public Image Render()
        {
            if (Camera == null)
                throw new InvalidOperationException("Camera not set");

            if (RenderMode == null)
                throw new InvalidOperationException("Render mode not set");

            if (SupersamplingFactor < 1)
                throw new InvalidOperationException("Supersampling factor invalid");

            if (MaxRecursionDepth < 0)
                throw new InvalidOperationException("Maximum recursion depth invalid");

            Image image = new Image(Camera.ViewWidth, Camera.ViewHeight);

            Parallel.For(0, image.Height, y => RenderLine(image, y));

            return image;
        }

        public List<Light> Lights { get; } = new List<Light>();

        public List<Object> Objects { get; } = new List<Object>();

        public Vector BackgroundColor { get; set; }

        public Camera Camera { get; set; }

        public RenderMode RenderMode { get; set; }

        public bool Shadows { get; set; }

        public int MaxRecursionDepth { get; set; }

        public int SupersamplingFactor { get; set; }
    }
}
