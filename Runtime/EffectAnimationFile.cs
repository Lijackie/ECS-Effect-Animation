using System.Collections.Generic;
using System;
using UnityEngine;

namespace EffectAnimation
{
    [Serializable]
    public class AnimationFile : ScriptableObject
    {
        public int FrameRate;
        public int MaxKey;
        public int LayerCount;
        public List<MonoLayer> Layers;
        public Texture2D Atlas;
        public Rect[] AtlasRects;
    }

    [Serializable]
    public class MonoLayer
    {
        public int TextureCount;
        public List<int> Textures;
        public int AnimationCount;
        public List<MonoAnimationEntry> Animations;
    }

    [Serializable]
    public class MonoAnimationEntry
    {
        public int Frame;
        public int Type;
        public Vector2 Position;
        public Vector2[] UVs;
        public Vector2[] XY;
        public float Aniframe;
        public int Anitype;
        public float Delay;
        public float Angle;
        public Color Color;
        public float SrcAlpha;
        public float DstAlpha;
        public float MTPreset;
    }
}