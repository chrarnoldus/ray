#nullable disable
using System;

namespace Ray
{
    sealed class Material
    {
        readonly double? refraction, reflectance;

        readonly Image texture;

        public Material(Vector color, double ambient, double diffuse, double specular, int shininess,
            double? refraction = null, string textureName = null)
            : this(ambient * color, diffuse * color, specular * Vector.AllOne, shininess, refraction, textureName)
        {

        }

        public Material(Vector ambientColor, Vector diffuseColor, Vector specularColor, int shininess,
            double? refraction = null, string textureName = null)
        {
            AmbientColor = ambientColor;
            DiffuseColor = diffuseColor;
            SpecularColor = specularColor;

            Shininess = shininess;
            this.refraction = refraction;

            if (refraction.HasValue)
            {
                if (refraction < 1.0)
                    throw new ArgumentOutOfRangeException("refraction");

                reflectance = Math.Pow(refraction.Value - 1.0, 2.0)
                    / Math.Pow(refraction.Value + 1.0, 2.0);
            }

            if (!string.IsNullOrEmpty(textureName))
                texture = Image.Read(textureName);
        }

        public Vector AmbientColor { get; }

        public Vector DiffuseColor { get; }

        public bool IsReflective
        {
            get
            {
                return
                    SpecularColor.R != 0.0 ||
                    SpecularColor.G != 0.0 ||
                    SpecularColor.B != 0.0;
            }
        }

        public Vector SpecularColor { get; }

        public int Shininess { get; }

        public bool IsTransparant
            => refraction.HasValue;

        public double Refraction
            => refraction.Value;

        public double Reflectance
            => reflectance.Value;

        public bool IsTextured
            => texture != null;

        public Vector GetTextureColor(UV? coordinates)
            => texture != null && coordinates.HasValue
                ? texture[coordinates.Value] : Vector.AllOne;
    }
}
