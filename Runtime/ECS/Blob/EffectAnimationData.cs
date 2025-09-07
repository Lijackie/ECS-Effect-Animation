using Unity.Entities;
using Unity.Mathematics;

namespace EffectAnimation
{
    public struct EffectAnimationData : IComponentData
    {
        public int FrameRate;
        public int MaxKey;
        public BlobArray<Layer> Layers;
        public BlobArray<float4> AtlasRects;

        public struct Layer : IComponentData
        {
            public BlobArray<int> Textures;
            public BlobArray<AnimationEntry> Animations;
        }

        public struct AnimationEntry : IComponentData
        {
            public int Frame;
            public int Type;
            public float2 Position;
            public BlobArray<float2> XY;
            public BlobArray<float2> UVs;
            public float Aniframe;
            public int Anitype;
            public float Delay;
            public float Angle;
            public float4 Color;
            public float SrcAlpha;
            public float DstAlpha;
        }
    }
}