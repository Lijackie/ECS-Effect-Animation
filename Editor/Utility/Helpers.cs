using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace EffectAnimation
{
    public static class DirectoryHelper
    {
        public static string GetRelativeDirectory(string root, string directory)
        {
            if (!root.EndsWith("/") && !root.EndsWith("\\"))
                root += "/";

            if (!directory.EndsWith("/") && !directory.EndsWith("\\"))
                directory += "/";

            //Debug.Log($"{root} {directory}");

            var uri1 = new Uri(root);
            var uri2 = new Uri(directory);

            return uri1.MakeRelativeUri(uri2).ToString();
        }
    }

    public static class PathHelper
    {
        public static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
    }

    public static class BinarySerializationExtensions
    {
        public static string ReadKoreanString(this BinaryReader br, int len)
        {
            var str = Encoding.GetEncoding(949).GetString(br.ReadBytes(len));
            if (str.Contains("\0"))
                str = str.Split('\0')[0];
            return str;
        }

        public static Vector2 ReadVector2(this BinaryReader br)
        {
            return new Vector2(br.ReadSingle(), br.ReadSingle());
        }

        public static float[] ReadFloatArray(this BinaryReader br, int count)
        {
            var arr = new float[count];
            for (var i = 0; i < count; i++)
                arr[i] = br.ReadSingle();
            return arr;
        }

        public static Color ReadColor2(this BinaryReader br)
        {
            return new Color(br.ReadSingle() / 255f, br.ReadSingle() / 255f, br.ReadSingle() / 255f, br.ReadSingle() / 255f);
        }
    }

    public static class VectorHelper
    {
        public static Vector2[] DefaultQuadUVs()
        {
            return new[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 0), new Vector2(1, 0) };
        }

        public static Vector3 CalcQuadNormal(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
        {
            var c1 = Vector3.Cross((v2 - v1), (v3 - v1));
            var c2 = Vector3.Cross((v4 - v2), (v1 - v2));
            var c3 = Vector3.Cross((v3 - v4), (v2 - v4));
            var c4 = Vector3.Cross((v1 - v3), (v4 - v3));

            return Vector3.Normalize((c1 + c2 + c3 + c4) / 4);
        }

        public static Vector3 CalcNormal(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            var side1 = v2 - v1;
            var side2 = v3 - v1;

            return Vector3.Cross(side1, side2).normalized;
        }

        public static Vector3 FlipY(this Vector3 v)
        {
            v.y = -v.y;
            return v;
        }

        public static Quaternion FlipY(this Quaternion q)
        {
            var euler = q.eulerAngles;
            return Quaternion.Euler(-euler.FlipY());
        }

        public static Vector2 Rotate(Vector2 v, float delta)
        {
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }

        public static Vector2 Clamp(Vector2 v, Vector2 min, Vector2 max)
        {
            return new Vector2(
                Mathf.Clamp(v.x, min.x, max.x),
                Mathf.Clamp(v.y, min.y, max.y)
            );
        }
    }
}