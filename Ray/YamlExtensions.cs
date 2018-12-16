using System;
using System.Collections.Generic;
using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Ray
{
    static class YamlExtensions
    {
        public static string GetValue(this YamlNode node)
        {
            try
            {
                return ((YamlScalarNode)node).Value;
            }
            catch
            {
                throw new YamlException("Failed to get value from node at " + node.Start);
            }
        }

        public static YamlNode TryGetChild(this YamlNode node, string key)
        {
            YamlNode child;
            node.GetKeyedChildren().TryGetValue((YamlScalarNode)key, out child);
            return child;
        }

        public static YamlNode GetChild(this YamlNode node, string key)
        {
            try
            {
                return node.GetKeyedChildren()[(YamlScalarNode)key];
            }
            catch
            {
                throw new YamlException("Failed to get child with key " + key + " from node at " + node.Start);
            }
        }

        public static IDictionary<YamlNode, YamlNode> GetKeyedChildren(this YamlNode node)
        {
            try
            {
                return ((YamlMappingNode)node).Children;
            }
            catch
            {
                throw new YamlException("Failed to get children of node at " + node.Start);
            }
        }

        public static IList<YamlNode> GetChildren(this YamlNode node)
        {
            try
            {
                return ((YamlSequenceNode)node).Children;
            }
            catch
            {
                throw new YamlException("Failed to get children of node at " + node.Start);
            }
        }

        public static T TryParseChild<T>(this YamlNode node, string key, Func<YamlNode, T> parser, T defaultValue = default(T))
        {
            YamlNode child = node.TryGetChild(key);
            return child != null ? parser(child) : defaultValue;
        }

        public static bool ParseBoolean(this YamlNode node)
        {
            try
            {
                return bool.Parse(((YamlScalarNode)node).Value);
            }
            catch
            {
                throw new YamlException("Failed to parse boolean from node at " + node.Start);
            }
        }

        public static double ParseDouble(this YamlNode node)
        {
            try
            {
                return double.Parse(((YamlScalarNode)node).Value, CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new YamlException("Failed to parse double from node at " + node.Start);
            }
        }

        public static int ParseInt32(this YamlNode node)
        {
            try
            {
                return int.Parse(((YamlScalarNode)node).Value, CultureInfo.InvariantCulture);
            }
            catch
            {
                throw new YamlException("Failed to parse integer from node at " + node.Start);
            }
        }

        public static Vector ParseVector(this YamlNode node)
        {
            try
            {
                IList<YamlNode> children = node.GetChildren();

                return new Vector(
                    children[0].ParseDouble(),
                    children[1].ParseDouble(),
                    children[2].ParseDouble());
            }
            catch
            {
                throw new YamlException("Failed to parse triple from node at " + node.Start);
            }
        }
    }
}
