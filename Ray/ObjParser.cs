#nullable disable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;

namespace Ray
{
    sealed class ObjParser
    {
        readonly ReadOnlyCollection<string[]> lines;

        readonly bool smooth;
        readonly Material defaultMaterial;
        readonly IEnumerable<Matrix> transformations;
        readonly IEnumerable<Hatch> hatches;

        public ObjParser(string fileName, bool smooth = false, Material defaultMaterial = null,
            IEnumerable<Matrix> transformations = null, IEnumerable<Hatch> hatches = null)
        {
            this.smooth = smooth;
            this.defaultMaterial = defaultMaterial;
            this.transformations = transformations;
            this.hatches = hatches;

            lines = ReadTokensPerLine(fileName).ToList().AsReadOnly();
        }

        static IEnumerable<string[]> ReadTokensPerLine(string fileName)
            => File.ReadAllLines(fileName).Select(line => line.Split(null as string[], StringSplitOptions.RemoveEmptyEntries));

        static Vector ParseVector(string[] tokens)
            => new Vector(
                double.Parse(tokens[1], CultureInfo.InvariantCulture),
                double.Parse(tokens[2], CultureInfo.InvariantCulture),
                double.Parse(tokens[3], CultureInfo.InvariantCulture));

        List<Vector> vertices;
        List<ObjFace> faces;
        List<Triangle> triangles;

        public Model ReadModel()
        {
            vertices = ReadVertices().ToList();
            UnitizeModel();

            faces = ReadFaces().ToList();
            triangles = CreateTriangles().ToList();

            IEnumerable<Tuple<Vector, Vector, Vector>> normalsPerTriangle =
                CalculateNormalsPerTriangle();

            return new Model(triangles, normalsPerTriangle, smooth, transformations);
        }

        IEnumerable<Vector> ReadVertices()
        {
            foreach (string[] tokens in lines)
            {
                if (tokens.FirstOrDefault() == "v")
                    yield return ParseVector(tokens);
            }
        }

        // Scale model to unit sphere on origin
        void UnitizeModel()
        {
            Vector minPoint = vertices.Aggregate((a, b) => new Vector(
                Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z)));

            Vector maxPoint = vertices.Aggregate((a, b) => new Vector(
                Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z)));

            Vector center = (minPoint + maxPoint) / 2.0;
            double radius = minPoint.Distance(maxPoint) / 2.0;

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector vertex = vertices[i];
                vertices[i] = (vertex - center) / radius;
            }
        }

        struct ObjFace
        {
            readonly int index1, index2, index3;

            public ObjFace(int index1, int index2, int index3, Material material)
            {
                this.index1 = index1;
                this.index2 = index3;
                this.index3 = index2;
                this.Material = material;
            }

            public int Index1 { get { return index1; } }
            public int Index2 { get { return index2; } }
            public int Index3 { get { return index3; } }

            public Material Material { get; }
        }

        IEnumerable<ObjFace> ReadFaces()
        {
            Dictionary<string, Material> materials = new Dictionary<string, Material>();
            Material currentMaterial = null;

            foreach (string[] tokens in lines)
            {
                switch (tokens.FirstOrDefault())
                {
                    case "mtllib":
                        materials = ReadMaterials(tokens[1]);
                        break;
                    case "usemtl":
                        materials.TryGetValue(tokens[1], out currentMaterial);
                        break;
                    case "f":
                        if (tokens.Length != 4)
                            throw new NotSupportedException("Only triangle faces are supported.");

                        List<int> indices = tokens.Skip(1) // "f"
                            .Select(token => token.Split('/')[0])
                            .Select(indexToken => int.Parse(indexToken, CultureInfo.InvariantCulture) - 1 /* Indices start from 1 */)
                            .ToList();

                        yield return new ObjFace(indices[0], indices[1], indices[2], currentMaterial);
                        break;
                }
            }
        }

        struct ObjMaterial
        {
            public ObjMaterial(string name)
                : this()
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentNullException(nameof(name));

                this.Name = name;
            }

            public string Name { get; }
            public bool IsNamed { get { return Name != null; } }

            public Vector AmbientColor { get; set; }
            public Vector DiffuseColor { get; set; }
            public Vector SpecularColor { get; set; }

            public int Shininess { get; set; }

            public Material ToMaterial()
                => new Material(AmbientColor, DiffuseColor, SpecularColor, Shininess);
        }

        static Dictionary<string, Material> ReadMaterials(string fileName)
        {
            IEnumerable<string[]> lines = ReadTokensPerLine(fileName);
            Dictionary<string, Material> materials = new Dictionary<string, Material>();

            ObjMaterial current = new ObjMaterial();

            foreach (string[] tokens in lines)
            {
                switch (tokens.FirstOrDefault())
                {
                    case "newmtl":
                        if (current.IsNamed)
                            materials[current.Name] = current.ToMaterial();
                        current = new ObjMaterial(name: tokens[1]);
                        break;
                    case "Ka":
                        current.AmbientColor = ParseVector(tokens);
                        break;
                    case "Kd":
                        current.DiffuseColor = ParseVector(tokens);
                        break;
                    case "Ks":
                        current.SpecularColor = ParseVector(tokens);
                        break;
                    case "Ns":
                        current.Shininess = (int)double.Parse(tokens[1], CultureInfo.InvariantCulture);
                        break;
                }
            }

            if (current.IsNamed)
                materials[current.Name] = current.ToMaterial();

            return materials;
        }

        IEnumerable<Triangle> CreateTriangles()
        {
            foreach (ObjFace face in faces)
            {
                Triangle triangle = new Triangle(face.Material ?? defaultMaterial,
                    vertices[face.Index1], vertices[face.Index2], vertices[face.Index3],
                    hatches: hatches);

                yield return triangle;
            }
        }

        IEnumerable<Tuple<Vector, Vector, Vector>> CalculateNormalsPerTriangle()
        {
            List<List<Vector>> normalsPerVertex = new List<List<Vector>>();

            for (int i = 0; i < vertices.Count; i++)
                normalsPerVertex.Add(new List<Vector>());

            for (int i = 0; i < faces.Count; i++)
            {
                ObjFace face = faces[i];
                Triangle triangle = triangles[i];

                normalsPerVertex[face.Index1].Add(triangle.FacetNormal);
                normalsPerVertex[face.Index2].Add(triangle.FacetNormal);
                normalsPerVertex[face.Index3].Add(triangle.FacetNormal);
            }

            List<Vector> vertexNormals = normalsPerVertex
                .Select(list => (list.Aggregate((a, b) => a + b) / list.Count).Normalize())
                .ToList();

            return faces.Select(face => Tuple.Create(
                vertexNormals[face.Index1],
                vertexNormals[face.Index2],
                vertexNormals[face.Index3]));
        }
    }
}
