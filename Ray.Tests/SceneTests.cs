using System;
using System.IO;
using System.Runtime.CompilerServices;
using ImageMagick;
using Xunit;

namespace Ray.Tests
{
    public sealed class SceneTests
    {
        static string GetCurrentDirectory([CallerFilePath] string callerFilePath = "")
            => Path.GetDirectoryName(callerFilePath)!;

        static MagickImage Convert(Image image)
            => new MagickImage(
                image.GetRgbData(),
                new PixelReadSettings(image.Width, image.Height, StorageType.Char, PixelMapping.RGB)
                );

        static void ApproveRenderResult([CallerMemberName] string name = "")
        {
            var image = RayTracer.RenderSceneToImage(Path.Combine(GetCurrentDirectory(), $"..\\Ray\\Scenes\\{name}.yaml"));
            var approvedPath = Path.Combine(GetCurrentDirectory(), $"Images\\{name}.Approved.png");

            if (!File.Exists(approvedPath))
            {
                image.Write(approvedPath);
                throw new Exception("Approved render saved");
            }

            using var approved = new MagickImage(approvedPath);

            using var unapproved = Convert(image);
            unapproved.Write(Path.Combine(GetCurrentDirectory(), $"Images\\{name}.Unapproved.png"));

            using var difference = new MagickImage();
            var error = approved.Compare(unapproved, ErrorMetric.Absolute, difference);
            difference.Write(Path.Combine(GetCurrentDirectory(), $"Images\\{name}.Difference.png"));

            if (error > 0)
            {
                throw new Exception("Difference detected");
            }
        }

        [Fact]
        public void Cylinder()
        {
            ApproveRenderResult();
        }

        [Fact]
        public void Eyes()
        {
            ApproveRenderResult();
        }

        [Fact]
        public void Glass()
        {
            ApproveRenderResult();
        }

        [Fact]
        public void Gooch()
        {
            ApproveRenderResult();
        }
    }
}
