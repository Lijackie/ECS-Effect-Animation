using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor;
using UnityEngine;
using Unity.Entities.Hybrid.Baking;

namespace EffectAnimation
{
    public class EffectLoader
    {
        private FileStream fs;
        private BinaryReader br;
        private int version;
        private AnimationFile anim;
        private GameObject go;

        private string effectPath;
        private string baseName;
        private string basePath;

        private Dictionary<string, int> textureIdLookup = new Dictionary<string, int>();
        private List<string> textureNames = new List<string>();
        private List<Texture2D> textures = new List<Texture2D>();
        private List<Sprite> sprites = new List<Sprite>();

        private MonoAnimationEntry LoadAnimationEntry()
        {
            var entry = new MonoAnimationEntry();

            entry.Frame = br.ReadInt32();
            entry.Type = br.ReadInt32();
            entry.Position = br.ReadVector2();

            var uv = br.ReadFloatArray(8);
            var xy = br.ReadFloatArray(8);

            entry.UVs = new Vector2[4];
            entry.UVs[0] = new Vector2(0, 0);
            entry.UVs[1] = new Vector2(1, 0);
            entry.UVs[2] = new Vector2(0, 1);
            entry.UVs[3] = new Vector2(1, 1);

            entry.XY = new Vector2[4];
            entry.XY[0] = new Vector2(xy[0], -xy[4]);
            entry.XY[1] = new Vector2(xy[1], -xy[5]);
            entry.XY[2] = new Vector2(xy[3], -xy[7]);
            entry.XY[3] = new Vector2(xy[2], -xy[6]);

            entry.Aniframe = br.ReadSingle();
            entry.Anitype = br.ReadInt32();
            entry.Delay = br.ReadSingle();
            entry.Angle = br.ReadSingle() / (1024f / 360f);
            entry.Color = br.ReadColor2();
            entry.SrcAlpha = br.ReadInt32();
            entry.DstAlpha = br.ReadInt32();
            entry.MTPreset = br.ReadInt32();

            //Debug.Log($"{entry.SrcAlpha} {entry.DstAlpha}");

            return entry;
        }

        private MonoLayer LoadLayer()
        {
            var layer = new MonoLayer();
            layer.TextureCount = br.ReadInt32();
            layer.Textures = new List<int>(layer.TextureCount);

            for (var i = 0; i < layer.TextureCount; i++)
            {
                var tex = br.ReadKoreanString(128);

                if (!textureNames.Contains(tex))
                {
                    layer.Textures.Add(textureNames.Count);
                    textureIdLookup.Add(tex, textureNames.Count);
                    textureNames.Add(tex);
                }
                else
                {
                    layer.Textures.Add(textureIdLookup[tex]);
                }
            }

            layer.AnimationCount = br.ReadInt32();
            layer.Animations = new List<MonoAnimationEntry>(layer.AnimationCount);

            for (var i = 0; i < layer.AnimationCount; i++)
            {
                layer.Animations.Add(LoadAnimationEntry());
            }

            return layer;
        }

        public AnimationFile Load(string path)
        {
            effectPath = path;
            baseName = Path.GetFileNameWithoutExtension(path);
            basePath = Path.GetDirectoryName(path);

            anim = ScriptableObject.CreateInstance(typeof(AnimationFile)) as AnimationFile;

            if (!File.Exists(path))
            {
                Debug.LogError("Could not load str animation at path: " + path);
                return null;
            }

            fs = new FileStream(path, FileMode.Open);
            br = new BinaryReader(fs);

            var header = new string(br.ReadChars(4));
            if (header != "STRM")
                throw new Exception("Not effect");

            version = br.ReadInt32();
            if (version != 148)
                throw new Exception($"Effect version is {version}, but we only support version 148!");

            anim.FrameRate = br.ReadInt32();
            anim.MaxKey = br.ReadInt32();
            anim.LayerCount = br.ReadInt32();

            anim.Layers = new List<MonoLayer>(anim.LayerCount);

            fs.Seek(16, SeekOrigin.Current); //skip display, group, type, I think

            for (var i = 0; i < anim.LayerCount; i++)
                anim.Layers.Add(LoadLayer());

            if (File.Exists(Path.Combine("Assets/Effects/", baseName + ".asset")))
                AssetDatabase.DeleteAsset(Path.Combine("Assets/Effects/", baseName + ".asset"));
            AssetDatabase.CreateAsset(anim, Path.Combine("Assets/Effects/", baseName + ".asset"));

            MakePrefab(anim, baseName);

            fs.Close();

            return anim;
        }

        public void MakePrefab(AnimationFile anim, string name)
        {
            var prefabPath = $"Assets/Effects/Prefabs/{name}.prefab";
            if (File.Exists(prefabPath))
                AssetDatabase.DeleteAsset($"Assets/Effects/Prefabs/{name}.prefab");

            var obj = new GameObject(name);
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            obj.AddComponent<LinkedEntityGroupAuthoring>();
            var meshFilter = obj.AddComponent<MeshFilter>();
            var meshRenderer = obj.AddComponent<MeshRenderer>();
            var effect = obj.AddComponent<EffectAnimationAuthoring>();
            effect.Anim = anim;
            effect.objects = new List<GameObject>();

            meshFilter.mesh = quad.GetComponent<MeshFilter>().sharedMesh;

            //var material = await Addressables.LoadAssetAsync<Material>("Assets/Effect Animation/Shaders/EffectAnimationAlpha.mat").Task;
            //var material = Resources.Load<Material>("Shaders/Effect/EffectAnimationAlpha");
            var material = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.ying.ecseffectanimation/Shaders/EffectAnimationAlpha.mat");
            
            if (material == null)
            {
                material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Effect Animation/Shaders/EffectAnimationAlpha.mat");
            }

            meshRenderer.material = material;

            for (int i = 0; i < anim.LayerCount; i++)
            {
                var layer = new GameObject($"layer {i}");

                effect.objects.Add(layer);
                layer.transform.parent = obj.transform;

                var filter = layer.AddComponent<MeshFilter>();
                var renderer = layer.AddComponent<MeshRenderer>();
                layer.AddComponent<EffectAnimationDataAuthoring>();

                filter.mesh = quad.GetComponent<MeshFilter>().sharedMesh;
                renderer.material = material;
            }

            PrefabUtility.SaveAsPrefabAssetAndConnect(obj, prefabPath, InteractionMode.AutomatedAction);
            UnityEngine.Object.DestroyImmediate(quad);
            UnityEngine.Object.DestroyImmediate(obj);
        }

        public void MakeAtlas(string path)
        {
            var atlasName = $"{baseName.Replace("\\", "_")}_atlas";
            var atlasPath = Path.Combine(path, atlasName + ".png");

            //if (!Directory.Exists(path))
            //    Directory.CreateDirectory(path);

            //if (!Directory.Exists("Assets/Effects/Sprites/" + baseName))
            //    Directory.CreateDirectory("Assets/Effects/Sprites/" + baseName);

            for (var i = 0; i < textureNames.Count; i++)
            {
                var texout = TextureImportHelper.GetOrImportTextureToProject(textureNames[i], basePath, "Assets/Effects/Textures/" + baseName, true);
                textures.Add(texout);
            }

            TextureImportHelper.SetTexturesReadable(textures);

            var extratexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            textures.Add(extratexture);

            var supertexture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            supertexture.name = atlasName;

            var atlasRects = supertexture.PackTextures(textures.ToArray(), 2, 4096, false);

            if (File.Exists(atlasPath))
            {
                File.Delete(atlasPath);
                ////we still needed to make the atlas to get the rects. We assume they're the same, but they might not be?
                //GameObject.DestroyImmediate(supertexture);
                //atlas = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
                //return;
            }

            TextureImportHelper.PatchAtlasEdges(supertexture, atlasRects);

            anim.Atlas = supertexture;
            anim.AtlasRects = atlasRects;

            //atlas = TextureImportHelper.SaveAndUpdateTexture(supertexture, atlasPath);
            AssetDatabase.AddObjectToAsset(supertexture, anim);

            sprites = new List<Sprite>(textureNames.Count);
            for (var i = 0; i < textureNames.Count; i++)
            {
                var texrect = new Rect(atlasRects[i].x * supertexture.width, atlasRects[i].y * supertexture.height, atlasRects[i].width * supertexture.width, atlasRects[i].height * supertexture.height);

                var sprite = Sprite.Create(supertexture, texrect, new Vector2(0.5f, 0.5f), 50, 0, SpriteMeshType.FullRect);

                sprite.name = Path.GetFileNameWithoutExtension(textureNames[i]);

                AssetDatabase.AddObjectToAsset(sprite, anim);

                //AssetDatabase.CreateAsset(sprite, "Assets/Effects/Sprites/" + baseName + "/" + sprite.name);
            }

            AssetDatabase.SetMainObject(anim, Path.Combine("Assets/Effects/", baseName + ".asset"));

            AssetDatabase.SaveAssets();
        }
    }
}