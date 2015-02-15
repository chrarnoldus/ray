using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ray
{
    sealed class Material
    {
        readonly Vector ambientColor, diffuseColor, specularColor;

        readonly int shininess;

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
            this.ambientColor = ambientColor;
            this.diffuseColor = diffuseColor;
            this.specularColor = specularColor;

            this.shininess = shininess;
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

        public Vector AmbientColor
        {
            get { return ambientColor; }
        }

        public Vector DiffuseColor
        {
            get { return diffuseColor; }
        }

        public bool IsReflective
        {
            get
            {
                return
                    specularColor.R != 0.0 ||
                    specularColor.G != 0.0 ||
                    specularColor.B != 0.0;
            }
        }

        public Vector SpecularColor
        {
            get { return specularColor; }
        }

        public int Shininess
        {
            get { return shininess; }
        }

        public bool IsTransparant
        {
            get { return refraction.HasValue; }
        }

        public double Refraction
        {
            get { return refraction.Value; }
        }

        public double Reflectance
        {
            get { return reflectance.Value; }
        }

        public bool IsTextured
        {
            get { return texture != null; }
        }

        public Vector GetTextureColor(UV? coordinates)
        {
            return texture != null && coordinates.HasValue
                ? texture[coordinates.Value] : Vector.AllOne;
        }
    }
}
