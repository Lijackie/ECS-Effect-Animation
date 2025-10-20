using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace EffectAnimation
{
    public class EffectAnimationAuthoring : MonoBehaviour
    {
        public AnimationFile Anim;
        public bool Loop;
        public List<GameObject> objects;

        public class Baker : Baker<EffectAnimationAuthoring>
        {
            public override void Bake(EffectAnimationAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent(entity, new EffectAnimation
                {
                    Data = BakeEffectAnimationDataToBlob(this, authoring.Anim),
                    Loop = authoring.Loop,
                    Alpha = authoring.Anim.name + "alpha",
                    Premulitiply = authoring.Anim.name + "premulitiply",
                    Additive = authoring.Anim.name + "additive",
                });
                AddComponent<EffectAnimationFinish>(entity);

                var buffer = AddBuffer<EffectChildrenBuffer>(entity);
                foreach (var o in authoring.objects)
                {
                    buffer.Add(new EffectChildrenBuffer
                    {
                        Value = GetEntity(o, TransformUsageFlags.Dynamic)
                    });
                }
            }

            private BlobAssetReference<EffectAnimationData> BakeEffectAnimationDataToBlob(IBaker baker, AnimationFile animationFile)
            {
                BlobBuilder builder = new BlobBuilder(Allocator.Temp);
                ref EffectAnimationData data = ref builder.ConstructRoot<EffectAnimationData>();

                data.FrameRate = animationFile.FrameRate;
                data.MaxKey = animationFile.MaxKey;

                BlobBuilderArray<EffectAnimationData.Layer> layers = builder.Allocate(ref data.Layers, animationFile.Layers.Count);

                for (int i = 0; i < animationFile.Layers.Count; i++)
                {
                    BlobBuilderArray<int> textures = builder.Allocate(ref layers[i].Textures, animationFile.Layers[i].Textures.Count);
                    BlobBuilderArray<EffectAnimationData.AnimationEntry> animationEntry = builder.Allocate(ref layers[i].Animations, animationFile.Layers[i].Animations.Count);

                    for (int j = 0; j < animationFile.Layers[i].Textures.Count; j++)
                    {
                        textures[j] = animationFile.Layers[i].Textures[j];
                    }

                    for (int k = 0; k < animationFile.Layers[i].Animations.Count; k++)
                    {
                        animationEntry[k].Frame = animationFile.Layers[i].Animations[k].Frame;
                        animationEntry[k].Type = animationFile.Layers[i].Animations[k].Type;
                        animationEntry[k].Position = animationFile.Layers[i].Animations[k].Position;
                        animationEntry[k].Aniframe = animationFile.Layers[i].Animations[k].Aniframe;
                        animationEntry[k].Anitype = animationFile.Layers[i].Animations[k].Anitype;
                        animationEntry[k].Delay = animationFile.Layers[i].Animations[k].Delay;
                        animationEntry[k].Angle = animationFile.Layers[i].Animations[k].Angle;
                        var color = animationFile.Layers[i].Animations[k].Color;
                        animationEntry[k].Color = new float4(color.r, color.g, color.b, color.a);
                        animationEntry[k].SrcAlpha = animationFile.Layers[i].Animations[k].SrcAlpha;
                        animationEntry[k].DstAlpha = animationFile.Layers[i].Animations[k].DstAlpha;

                        BlobBuilderArray<float2> XY = builder.Allocate(ref animationEntry[k].XY, animationFile.Layers[i].Animations[k].XY.Length);
                        BlobBuilderArray<float2> UVs = builder.Allocate(ref animationEntry[k].UVs, animationFile.Layers[i].Animations[k].UVs.Length);

                        for (int a = 0; a < animationFile.Layers[i].Animations[k].XY.Length; a++)
                        {
                            XY[a] = animationFile.Layers[i].Animations[k].XY[a];
                        }

                        for (int b = 0; b < animationFile.Layers[i].Animations[k].UVs.Length; b++)
                        {
                            UVs[b] = animationFile.Layers[i].Animations[k].UVs[b];
                        }
                    }
                }

                // uv ¦ì¸m
                BlobBuilderArray<float4> atlasRects = builder.Allocate(ref data.AtlasRects, animationFile.AtlasRects.Length);
                for (int i = 0; i < atlasRects.Length; i++)
                {
                    var rect = animationFile.AtlasRects[i];
                    atlasRects[i] = new float4(rect.width, rect.height, rect.x, rect.y);
                }


                BlobAssetReference<EffectAnimationData> blobReference = builder.CreateBlobAssetReference<EffectAnimationData>(Allocator.Persistent);
                baker.AddBlobAsset(ref blobReference, out var hash);
                builder.Dispose();

                baker.DependsOn(animationFile);

                return blobReference;
            }
        }
    }

    public struct EffectAnimation : IComponentData
    {
        public float Time;
        public int Frame;
        public bool Loop;
        public FixedString32Bytes Alpha;
        public FixedString32Bytes Premulitiply;
        public FixedString32Bytes Additive;
        public BlobAssetReference<EffectAnimationData> Data;
    }

    public struct EffectAnimationFinish : IComponentData
    {
        public bool Value;
    }

    public struct EffectChildrenBuffer : IBufferElementData
    {
        public Entity Value;
    }
}