using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using YamlDotNet.RepresentationModel;

namespace Ray
{
    sealed class RayTracer
    {
        YamlNode doc;

        Scene scene;

        IEnumerable<Matrix> ParseTransformations(YamlNode node)
        {
            foreach (YamlNode transformationNode in node.GetChildren())
            {
                IList<YamlNode> children = transformationNode.GetChildren();

                string type = children[0].GetValue();

                // Als transformations are inverted, because they are done on rays
                switch (type)
                {
                    case "rotatex":
                        yield return Matrix.RotateX(Angle.FromDegrees(-children[1].ParseDouble()));
                        break;
                    case "rotatey":
                        yield return Matrix.RotateY(Angle.FromDegrees(-children[1].ParseDouble()));
                        break;
                    case "rotatez":
                        yield return Matrix.RotateZ(Angle.FromDegrees(-children[1].ParseDouble()));
                        break;
                    case "scale":
                        yield return Matrix.Scale(Vector.AllOne / children[1].ParseVector());
                        break;
                    case "translate":
                        yield return Matrix.Translate(-children[1].ParseVector());
                        break;
                    default:
                        Console.WriteLine("Warning: unknown transformation at " + transformationNode.Start + ", ignored.");
                        break;
                }
            }
        }

        Camera ParseCamera(YamlNode node)
        {
            IList<YamlNode> viewSizeNodes = node.GetChild("viewSize").GetChildren();

            Camera camera = new Camera(
                eye: node.GetChild("eye").ParseVector(),
                center: node.GetChild("center").ParseVector(),
                up: node.GetChild("up").ParseVector(),
                viewWidth: viewSizeNodes[0].ParseInt32(),
                viewHeight: viewSizeNodes[1].ParseInt32());

            return camera;
        }

        int ParseSupersamplingFactor(YamlNode node)
            => node.GetChild("factor").ParseInt32();

        Light ParseLight(YamlNode node)
            => new Light(
                position: node.GetChild("position").ParseVector(),
                color: node.GetChild("color").ParseVector());

        Material ParseMaterial(YamlNode node)
            => new Material(
                color: node.TryParseChild("color", YamlExtensions.ParseVector, Vector.AllOne),
                ambient: node.GetChild("ka").ParseDouble(),
                diffuse: node.GetChild("kd").ParseDouble(),
                specular: node.GetChild("ks").ParseDouble(),
                shininess: node.GetChild("n").ParseInt32(),
                refraction: node.TryParseChild("eta", (Func<YamlNode, double?>)(etaNode => etaNode.ParseDouble())),
                textureName: node.TryParseChild("texture", YamlExtensions.GetValue));

        IEnumerable<Hatch> ParseHatches(YamlNode node)
        {
            foreach (YamlNode hatchNode in node.GetChildren())
            {
                IList<YamlNode> children = hatchNode.GetChildren();

                yield return new Hatch(
                    normal: children[0].ParseVector(),
                    distance: children[1].ParseDouble(),
                    weight: children[2].ParseDouble());
            }
        }

        CsgOperator ParseOperator(YamlNode node)
        {
            switch (node.GetValue())
            {
                case "unite":
                    return CsgOperators.Unite;
                case "intersect":
                    return CsgOperators.Intersect;
                case "except":
                    return CsgOperators.Except;
                default:
                    Console.WriteLine("Unknown CSG operator at " + node.Start + ", using unite.");
                    goto case "unite";
            }
        }

        Object ParseObject(YamlNode node)
        {
            Material material = node.TryParseChild("material", ParseMaterial);

            IEnumerable<Matrix> transformations = node.TryParseChild("transformations", ParseTransformations);
            IEnumerable<Hatch> hatches = node.TryParseChild("hatches", ParseHatches);

            string type = node.GetChild("type").GetValue();

            switch (type)
            {
                case "sphere":
                    return new Sphere(material,
                        position: node.TryParseChild("position", YamlExtensions.ParseVector),
                        radius: node.TryParseChild("radius", YamlExtensions.ParseDouble, 1.0),
                        transformations: transformations, hatches: hatches);
                case "cylinder":
                    return new Cylinder(material, transformations, hatches);
                case "csg":
                    return new CsgObject(
                        left: ParseObject(node.GetChild("left")),
                        right: ParseObject(node.GetChild("right")),
                        csgOperator: ParseOperator(node.GetChild("operator")),
                        transformations: transformations);
                case "triangle":
                    return new Triangle(material,
                        vertexA: node.GetChild("a").ParseVector(),
                        vertexB: node.GetChild("b").ParseVector(),
                        vertexC: node.GetChild("c").ParseVector(),
                        transformations: transformations, hatches: hatches);
                case "model":
                    ObjParser parser = new ObjParser(
                        node.GetChild("name").GetValue(),
                        node.GetChild("smooth").ParseBoolean(),
                        material, transformations, hatches);
                    return parser.ReadModel();
                default:
                    return null;
            }
        }

        GoochParameters ParseGoochParameters(YamlNode node)
            => new GoochParameters(
                blue: node.GetChild("b").ParseDouble(),
                yellow: node.GetChild("y").ParseDouble(),
                alpha: node.GetChild("alpha").ParseDouble(),
                beta: node.GetChild("beta").ParseDouble());

        RenderMode ParseRenderMode(YamlNode renderModeNode)
        {
            string renderMode = renderModeNode.GetValue();

            switch (renderMode)
            {
                case "phong":
                    return new PhongRenderMode(scene);
                case "gooch":
                    GoochParameters parameters = ParseGoochParameters(
                        doc.GetChild("GoochParameters"));
                    return new GoochRenderMode(scene, parameters);
                case "copperplate":
                    return new CopperplateRenderMode(scene);
                case "none":
                    return new NullRenderMode(scene);
                default:
                    Console.WriteLine("Warning: unknown render mode, using Phong.");
                    goto case "phong";
            }
        }

        void ParseScene()
        {
            scene.Camera = doc.TryParseChild("Camera", ParseCamera)
                ?? new Camera(doc.GetChild("Eye").ParseVector());

            scene.BackgroundColor = doc.TryParseChild("Background", YamlExtensions.ParseVector);

            scene.RenderMode = doc.TryParseChild("RenderMode", ParseRenderMode)
                ?? new PhongRenderMode(scene);

            scene.Shadows = doc.TryParseChild("Shadows", YamlExtensions.ParseBoolean);

            scene.MaxRecursionDepth = doc.TryParseChild("MaxRecursionDepth", YamlExtensions.ParseInt32);

            scene.SupersamplingFactor = doc.TryParseChild("SuperSampling", ParseSupersamplingFactor, 1);

            foreach (YamlNode lightNode in doc.GetChild("Lights").GetChildren())
                scene.Lights.Add(ParseLight(lightNode));

            foreach (YamlNode objectNode in doc.GetChild("Objects").GetChildren())
            {
                Object obj = ParseObject(objectNode);

                if (obj != null)
                    scene.Objects.Add(obj);
                else
                    Console.WriteLine("Warning: found object of unknown type, ignored.");
            }
        }

        YamlStream ReadYamlStream(string inputFileName)
        {
            YamlStream stream = new YamlStream();
            using (StreamReader reader = new StreamReader(inputFileName))
            {
                stream.Load(reader);
            }
            return stream;
        }

        void ReadScene(string inputFileName)
        {
            scene = new Scene();

            YamlStream stream = ReadYamlStream(inputFileName);

            if (stream.Documents.Count > 0)
            {
                doc = stream.Documents[0].RootNode;
                ParseScene();
            }
            else Console.WriteLine("Warning: no YAML documents.");

            if (stream.Documents.Count > 1)
                Console.WriteLine("Warning: unexpected YAML document, ignored.");

            Console.WriteLine("YAML parsing results: " + scene.Objects.Count + " objects read.");
        }

        void RenderToFile(string outputFileName)
        {
            Console.WriteLine("Tracing...");

            Stopwatch watch = Stopwatch.StartNew();
            Image image = scene.Render();
            watch.Stop();

            Console.WriteLine("Tracing time: " + watch.Elapsed.ToString("g"));

            Console.WriteLine("Writing image to " + outputFileName + "...");
            image.Write(outputFileName);

            Console.WriteLine("Done.");
        }

        static void Main(string[] args)
        {
            string name = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);

            if (args.Length < 1 || args.Length > 2)
            {
                Console.WriteLine("Usage: {0} in-file [out-file.png]", name);
                return;
            }

            string inputFileName = args[0];
            string outputFileName = args.Length > 1 ? args[1] : Path.ChangeExtension(inputFileName, "png");

            RayTracer rayTracer = new RayTracer();
            rayTracer.ReadScene(inputFileName);
            rayTracer.RenderToFile(outputFileName);
        }
    }
}
