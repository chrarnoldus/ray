using System.Drawing;
using System.Drawing.Imaging;

namespace Ray
{
    struct UV
    {
        public UV(double u, double v)
        {
            U = u;
            V = v;
        }

        public double U { get; }

        public double V { get; }
    }

    sealed class Image
    {
        readonly Vector[,] pixels;

        public Image(int width, int height)
        {
            pixels = new Vector[width, height];
        }

        public int Width
            => pixels.GetLength(0);

        public int Height
            => pixels.GetLength(1);

        public Vector this[int x, int y]
        {
            get { return pixels[x, y]; }
            set { pixels[x, y] = value; }
        }

        public Vector this[UV coordinates]
        {
            get
            {
                int x = (int)(coordinates.U * (Width - 1)),
                    y = (int)((1.0 - coordinates.V) * (Height - 1));

                return pixels[x, y];
            }
        }

        public void Write(string fileName)
        {
            using (Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb))
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        Vector color = this[x, y];

                        bitmap.SetPixel(x, y, Color.FromArgb(
                            (int)(color.R * 255),
                            (int)(color.G * 255), 
                            (int)(color.B * 255)));
                    }
                }

                bitmap.Save(fileName);
            }
        }

        public static Image Read(string fileName)
        {
            using (Bitmap bitmap = new Bitmap(fileName))
            {
                Image image = new Image(bitmap.Width, bitmap.Height);

                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        Color color = bitmap.GetPixel(x, y);

                        image[x, y] = new Vector(
                            color.R / 255.0, 
                            color.G / 255.0, 
                            color.B / 255.0);
                    }
                }

                return image;
            }
        }
    }
}
